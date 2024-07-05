using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes;

/// <summary>
///     Typical A* pathfinder with the stipulation of gravity for determining adjacent nodes.
///     Main usage is for Town NPCs.
/// </summary>
/// <remarks>
///     Inspired by generic C# implementation by Gustavo Franco:
///     https://www.codeproject.com/articles/15307/a-algorithm-implementation-in-c.
/// </remarks>
public class TownNPCPathfinder {
    public record struct PathNode(UPoint16 NodePos, UPoint16 ParentNodePos, NodeMovementType MovementType);

    public enum NodeMovementType : byte {
        PureHorizontal,
        StepUp,
        StepDown,
        Jump,
        Fall
    }

    private readonly struct TileData(byte weight, TileFlags flags) {
        public readonly byte weight = weight;
        public readonly TileFlags flags = flags;

        public override string ToString() => $"Weight: {weight}, Flags: {flags}";
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct InternalNode {
        public ushort f;
        public ushort g;
        public UPoint16 parentPos;
        public byte nodeStatusInteger;
    }

    private readonly struct PointGrid<T>(T[,] grid)
        where T : struct {
        public ref T this[UPoint16 point] => ref grid[point.x, point.y];
    }

    [Flags]
    private enum TileFlags : byte {
        Empty = 0,

        /// <summary>
        ///     For tiles/spaces that are completely solid, and cannot be moved through.
        ///     This includes non-actuated solid tiles.
        /// </summary>
        Solid = 1,

        /// <summary>
        ///     For tiles/spaces that can't be navigated through, but aren't necessarily
        ///     solid.
        /// </summary>
        Impassable = 2,

        /// <summary>
        ///     For tiles/spaces that are classified as platforms, and only have collision from the top.
        /// </summary>
        Platform = 4,

        /// <summary>
        ///     For tiles that can be stepped up if the pathfinder is coming from the right, and moving
        ///     to the left. Vice versa for stepping down.
        /// </summary>
        CanStepWhenComingFromRight = 8,

        /// <summary>
        ///     For tiles that can be stepped up if the pathfinder is coming from the left, and moving
        ///     to the right. Vice versa for stepping down.
        /// </summary>
        CanStepWhenComingFromLeft = 16,

        /// <summary>
        ///     For solid tiles that are in their half-block slope form.
        /// </summary>
        HalfTile = 32
    }

    private const int MaxNodeSearch = 2000;
    private const int ManhattanEstimateTuneValue = 2;

    private const int MaxJumpHeight = 7;
    private const int MaxFallDistance = 11;
    private const int AdditionalCostForStairs = 15;

    private readonly UPoint16 _topLeftOfGrid;
    private readonly ushort _rectSizeX;
    private readonly ushort _rectSizeY;
    private readonly ushort _gridSizeX;
    private readonly ushort _gridSizeY;
    private readonly PointGrid<InternalNode> _nodeGrid;
    private readonly PointGrid<TileData> _tileGrid;
    private readonly PriorityQueue<UPoint16, UPoint16> _openQueue;
    private UPoint16 _start;
    private UPoint16 _end;
    private bool _pathfindingStopped;
    private bool _reachedGoal;
    private int _nodesClosed;
    private byte _currentOpenNodeValue = 1;
    private byte _currentClosedNodeValue = 2;

    public TownNPCPathfinder(UPoint16 topLeftOfGrid, ushort gridSize, ushort rectSizeX, ushort rectSizeY) : this(topLeftOfGrid, gridSize, gridSize, rectSizeX, rectSizeY) { }

    public TownNPCPathfinder(UPoint16 topLeftOfGrid, ushort gridSizeWidth, ushort gridSizeHeight, ushort rectSizeX, ushort rectSizeY) {
        _topLeftOfGrid = topLeftOfGrid;
        _rectSizeX = rectSizeX;
        _rectSizeY = rectSizeY;

        _gridSizeX = gridSizeWidth;
        _gridSizeY = gridSizeHeight;

        _nodeGrid = new PointGrid<InternalNode>(new InternalNode[_gridSizeX, _gridSizeY]);
        _tileGrid = new PointGrid<TileData>(GenerateTileGrid());
        _openQueue = new PriorityQueue<UPoint16, UPoint16>(Comparer<UPoint16>.Create((pointOne, pointTwo) => _nodeGrid[pointOne].f.CompareTo(_nodeGrid[pointTwo].f)));
    }

    private static byte GetTileMovementCost(Tile tile) {
        byte tileCost = 0;

        if (tile.LiquidAmount <= 0) {
            return tileCost;
        }

        switch (tile.LiquidType) {
            case LiquidID.Water:
                tileCost += 8;
                break;
            case LiquidID.Honey:
                tileCost += 12;
                break;
        }

        return tileCost;
    }

    public List<PathNode> FindPath(UPoint16 startPoint, UPoint16 endPoint) {
        _start = startPoint;
        _end = endPoint;

        if (!StartAndEndPointsAreValid()) {
            return null;
        }

        InitiatePathingVariables();

        DoPathfindingLoop();

        return FinalizePath();
    }

    private TileData[,] GenerateTileGrid() {
        TileData[,] grid = new TileData[_gridSizeX, _gridSizeY];

        for (int i = 0; i < _gridSizeX; i++) {
            for (int j = 0; j < _gridSizeY; j++) {
                Tile tile = Main.tile[(_topLeftOfGrid + new UPoint16(i, j)).Point16];

                bool hasTile = tile.HasTile;
                bool isActuated = tile.IsActuated;
                bool isSolid = Main.tileSolid[tile.TileType];
                bool isPlatform = TileID.Sets.Platforms[tile.TileType];
                bool isClosedDoor = TileLoader.OpenDoorID(tile) > 0;


                if (hasTile && !isActuated && isSolid && !isClosedDoor) {
                    if (!isPlatform) {
                        TileFlags additionalFlags = TileID.Sets.IgnoredByNpcStepUp[tile.TileType] ? TileFlags.Empty : TileFlags.CanStepWhenComingFromLeft | TileFlags.CanStepWhenComingFromRight;
                        if (tile.IsHalfBlock) {
                            additionalFlags |= TileFlags.HalfTile;
                        }

                        grid[i, j] = new TileData(0, TileFlags.Solid | additionalFlags);
                    }
                    else {
                        /*
                        SlopeDownLeft = ◣
                        SlopeDownRight = ◢
                        SlopeUpLeft = ◤
                        SlopeUpRight = ◥
                         */
                        TileFlags slopeFlag = tile.Slope switch {
                            SlopeType.SlopeDownLeft => TileFlags.CanStepWhenComingFromRight,
                            SlopeType.SlopeDownRight => TileFlags.CanStepWhenComingFromLeft,
                            _ => TileFlags.Empty
                        };

                        grid[i, j] = new TileData(0, TileFlags.Platform | slopeFlag);
                    }

                    continue;
                }

                bool hasLava = tile is { LiquidAmount: > 0, LiquidType: LiquidID.Lava };
                bool hasShimmer = tile is { LiquidAmount: >= 200, LiquidType: LiquidID.Shimmer };
                if (!hasTile && (hasLava || hasShimmer)) {
                    grid[i, j] = new TileData(0, TileFlags.Impassable);
                    continue;
                }

                grid[i, j] = new TileData(GetTileMovementCost(tile), 0);
            }
        }

        return grid;
    }

    private bool StartAndEndPointsAreValid() => RectangleHasNoTiles(_start, _rectSizeX, _rectSizeY)
        && RectangleHasNoTiles(_end, _rectSizeX, _rectSizeY)
        && PointOnStandableTile(_start, _rectSizeX)
        && PointOnStandableTile(_end, _rectSizeX);

    private void InitiatePathingVariables() {
        _reachedGoal = _pathfindingStopped = false;
        _nodesClosed = 0;
        _currentOpenNodeValue += 2;
        _currentClosedNodeValue += 2;
        _openQueue.Clear();

        _nodeGrid[_start] = new InternalNode { g = 0, f = ManhattanEstimateTuneValue, parentPos = _start, nodeStatusInteger = _currentOpenNodeValue };

        _openQueue.Enqueue(_start, _start);
    }

    private void DoPathfindingLoop() {
        // TODO: Benchmark, optimize
        while (_openQueue.Count > 0 && !_pathfindingStopped) {
            UPoint16 curNodePos = _openQueue.Dequeue();

            if (_nodeGrid[curNodePos].nodeStatusInteger == _currentClosedNodeValue) {
                continue;
            }

            if (curNodePos == _end) {
                _nodeGrid[curNodePos].nodeStatusInteger = _currentClosedNodeValue;
                _reachedGoal = true;
                break;
            }

            if (_nodesClosed > MaxNodeSearch) {
                _pathfindingStopped = true;
                break;
            }

            // Left or right pure horizontal
            for (sbyte i = -1; i < 2; i += 2) {
                UPoint16 nextNodePos = new(curNodePos.x + i, curNodePos.y);

                if (!RectangleHasNoTiles(nextNodePos, _rectSizeX, _rectSizeY) || !PointOnStandableTile(nextNodePos, _rectSizeX)) {
                    continue;
                }

                DoSuccessorChecksAndCalculations(curNodePos, nextNodePos, 1);
            }

            // One tile move up (step or jump)
            if (RectangleHasNoTiles(new UPoint16(curNodePos.x, curNodePos.y - 1), _rectSizeX, _rectSizeY)) {
                for (sbyte i = -1; i < 2; i += 2) {
                    UPoint16 nextNodePos = new(curNodePos.x + i, curNodePos.y - 1);
                    if (RectangleHasNoTiles(nextNodePos, _rectSizeX, _rectSizeY) && PointOnStandableTile(nextNodePos, _rectSizeX)) {
                        DoSuccessorChecksAndCalculations(curNodePos, nextNodePos, 2);
                    }
                }
            }

            // One tile move down (step or fall)
            for (sbyte i = -1; i < 2; i += 2) {
                UPoint16 nextNodePos = new(curNodePos.x + i, curNodePos.y + 1);
                if (!RectangleHasNoTiles(nextNodePos, _rectSizeX, (ushort)(_rectSizeY + 1)) || !PointOnStandableTile(nextNodePos, _rectSizeX)) {
                    continue;
                }

                DoSuccessorChecksAndCalculations(curNodePos, nextNodePos, 2);
            }

            // Additional cost to dissuade jumping off of or falling through platforms
            // Has the overall effect of making NPC movement more "natural" by preferring to use stairs
            ushort startingPlatformCost = (ushort)(PointOnPlatform(curNodePos, _rectSizeX) ? AdditionalCostForStairs : 0);

            // Falls
            for (sbyte i = -1; i < 2; i++) {
                UPoint16 nextNodePos = new(curNodePos.x + i, curNodePos.y);
                if (!RectangleHasNoTiles(new UPoint16(nextNodePos.x, nextNodePos.y + 1), _rectSizeX, 1) || (i != 0 && !RectangleHasNoTiles(nextNodePos, _rectSizeX, _rectSizeY))) {
                    continue;
                }

                for (nextNodePos.y += 2; nextNodePos.y <= curNodePos.y + MaxFallDistance; nextNodePos.y++) {
                    if (!RectangleHasNoTiles(nextNodePos, _rectSizeX, _rectSizeY)) {
                        break;
                    }

                    if (!PointOnStandableTile(nextNodePos, _rectSizeX)) {
                        continue;
                    }

                    // Add additional weight if the end location is a platform, for more dissuading
                    ushort finalPlatformCost = (ushort)(startingPlatformCost + (PointOnPlatform(nextNodePos, _rectSizeX) ? AdditionalCostForStairs : 0));
                    DoSuccessorChecksAndCalculations(curNodePos, nextNodePos, (ushort)(Math.Abs(i) + finalPlatformCost + (nextNodePos.y - curNodePos.y) * 2));
                    break;
                }
            }

            // Jumps
            for (ushort i = 2; i < MaxJumpHeight; i++) {
                UPoint16 nextNodePos = new(curNodePos.x, curNodePos.y - i);
                if (!RectangleHasNoTiles(nextNodePos, _rectSizeX, _rectSizeY)) {
                    break;
                }

                for (sbyte j = -1; j < 2; j++) {
                    nextNodePos = new UPoint16(curNodePos.x + j, nextNodePos.y);
                    if ((j != 0 && !RectangleHasNoTiles(nextNodePos, _rectSizeX, _rectSizeY)) || !PointOnStandableTile(nextNodePos, _rectSizeX)) {
                        continue;
                    }

                    // Add additional weight if the end location is a platform, for more dissuading
                    ushort finalPlatformCost = (ushort)(startingPlatformCost + (PointOnPlatform(nextNodePos, _rectSizeX) ? AdditionalCostForStairs : 0));
                    DoSuccessorChecksAndCalculations(curNodePos, nextNodePos, (ushort)(Math.Abs(j) + finalPlatformCost + i * 2));
                }
            }
        }
    }

    private void DoSuccessorChecksAndCalculations(UPoint16 curNodePos, UPoint16 nextNodePos, ushort gCostModifier) {
        ushort newG = (ushort)(_nodeGrid[curNodePos].g + _tileGrid[nextNodePos].weight + gCostModifier);

        if (_nodeGrid[nextNodePos].nodeStatusInteger == _currentOpenNodeValue || _nodeGrid[nextNodePos].nodeStatusInteger == _currentClosedNodeValue) {
            if (_nodeGrid[nextNodePos].g <= newG) {
                return;
            }
        }

        _nodeGrid[nextNodePos] = _nodeGrid[nextNodePos] with {
            parentPos = curNodePos,
            g = newG,
            // Manhattan distance heuristic
            f = (ushort)(newG + ManhattanEstimateTuneValue * (Math.Abs(nextNodePos.x - _end.x) + Math.Abs(nextNodePos.y - _end.y))),
            nodeStatusInteger = _currentOpenNodeValue
        };

        _openQueue.Enqueue(nextNodePos, nextNodePos);
    }

    private List<PathNode> FinalizePath() {
        if (!_reachedGoal) {
            _pathfindingStopped = true;
            return null;
        }

        List<PathNode> foundPath = [];
        InternalNode curInternalNode = _nodeGrid[_end];
        PathNode curPathNode = new(_end, curInternalNode.parentPos, NodeMovementType.PureHorizontal);

        while (curPathNode.NodePos != curPathNode.ParentNodePos) {
            foundPath.Add(curPathNode);

            NodeMovementType nextMovementType;
            UPoint16 parentNodePos = curPathNode.ParentNodePos;
            int yNodeDiff = parentNodePos.y - curPathNode.NodePos.y;
            int xNodeDiff = parentNodePos.x - curPathNode.NodePos.x;
            // Remember that to construct the path we go from END to START. So everything is reversed
            switch (yNodeDiff) {
                case 0:
                    //TODO: Further investigate half tile issues
                    nextMovementType = IsStandingOnHalfTile(parentNodePos, _rectSizeX) ? NodeMovementType.StepUp : NodeMovementType.PureHorizontal;
                    //nextMovementType = NodeMovementType.PureHorizontal;
                    break;
                case 1: {
                    nextMovementType = NodeMovementType.Jump;
                    if (xNodeDiff != 0 && CanStep(curPathNode.NodePos, _rectSizeX, xNodeDiff < 0)) {
                        nextMovementType = NodeMovementType.StepUp;
                    }

                    break;
                }
                case > 1:
                    nextMovementType = NodeMovementType.Jump;
                    break;
                case -1: {
                    nextMovementType = NodeMovementType.Fall;
                    if (xNodeDiff != 0 && CanStepDown(parentNodePos, _rectSizeX, xNodeDiff < 0)) {
                        nextMovementType = NodeMovementType.StepDown;
                    }

                    break;
                }
                default:
                    nextMovementType = NodeMovementType.Fall;
                    break;
            }

            curInternalNode = _nodeGrid[parentNodePos];
            curPathNode = new PathNode(curPathNode.ParentNodePos, curInternalNode.parentPos, nextMovementType);
        }

        foundPath.Add(curPathNode);
        _pathfindingStopped = true;
        return foundPath;
    }

    private bool RectangleHasNoTiles(UPoint16 bottomLeft, ushort sizeX, ushort sizeY) {
        if (!RectangleWithinGrid(bottomLeft, sizeX, sizeY)) {
            return false;
        }

        for (ushort i = bottomLeft.x; i < bottomLeft.x + sizeX; i++) {
            for (ushort j = (ushort)(bottomLeft.y - sizeY + 1); j <= bottomLeft.y; j++) {
                TileFlags tileFlags = _tileGrid[new UPoint16(i, j)].flags;
                if (tileFlags.HasFlag(TileFlags.Solid) || tileFlags.HasFlag(TileFlags.Impassable)) {
                    return false;
                }
            }
        }

        return true;
    }

    private bool RectangleWithinGrid(UPoint16 bottomLeft, ushort sizeX, ushort sizeY) {
        ushort outerBoundX = (ushort)(bottomLeft.x + sizeX - 1);
        ushort outerBoundY = (ushort)(bottomLeft.y - sizeY + 1);

        return bottomLeft.x < _gridSizeX && bottomLeft.y < _gridSizeY && outerBoundX < _gridSizeX && outerBoundY < _gridSizeY;
    }

    private IEnumerable<TileFlags> FlagsOfTilesRectangleIsOn(UPoint16 bottomLeft, ushort rectWidth) {
        if (!RectangleWithinGrid(bottomLeft + new UPoint16(0, 1), rectWidth, 1)) {
            yield break;
        }

        for (ushort i = 0; i < rectWidth; i++) {
            yield return _tileGrid[new UPoint16(bottomLeft.x + i, bottomLeft.y + 1)].flags;
        }
    }

    private bool PointOnStandableTile(UPoint16 bottomLeft, ushort rectWidth) =>
        FlagsOfTilesRectangleIsOn(bottomLeft, rectWidth).Any(flags => flags.HasFlag(TileFlags.Solid) || flags.HasFlag(TileFlags.Platform));

    private bool PointOnPlatform(UPoint16 bottomLeft, ushort rectWidth, bool stairCheck = false) => FlagsOfTilesRectangleIsOn(bottomLeft, rectWidth)
        .Any(flags => flags.HasFlag(TileFlags.Platform) && (!stairCheck || (!flags.HasFlag(TileFlags.CanStepWhenComingFromLeft) && !flags.HasFlag(TileFlags.CanStepWhenComingFromRight))));

    private bool IsStandingOnHalfTile(UPoint16 bottomLeft, ushort rectWidth) => FlagsOfTilesRectangleIsOn(bottomLeft, rectWidth).Any(flags => flags.HasFlag(TileFlags.HalfTile));

    private bool CanStep(UPoint16 wantedStepPos, ushort rectWidth, bool fromLeft) => FlagsOfTilesRectangleIsOn(wantedStepPos, rectWidth)
        .Any(flags => flags.HasFlag(fromLeft ? TileFlags.CanStepWhenComingFromLeft : TileFlags.CanStepWhenComingFromRight));

    private bool CanStepDown(UPoint16 startNodePos, ushort rectWidth, bool fromLeft) => !PointOnPlatform(startNodePos, rectWidth, true) && CanStep(startNodePos, rectWidth, !fromLeft);
}