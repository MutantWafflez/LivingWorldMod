using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes;

/// <summary>
/// Typical A* pathfinder with the stipulation of gravity for determining adjacent nodes.
/// Main usage is for Town NPCs.
/// </summary>
/// <remarks>
/// Inspired by generic C# implementation by Gustavo Franco:
/// https://www.codeproject.com/articles/15307/a-algorithm-implementation-in-c.
/// </remarks>
public class TownNPCPathfinder {
    public record struct PathNode(UPoint16 NodePos, UPoint16 ParentNodePos, NodeMovementType MovementType);

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
        public NodeMovementType movementType;
    }

    private record struct OpenNode(UPoint16 NodePos, NodeMovementType MovementType);

    private sealed class PointGrid<T>(T[,] grid)
        where T : struct {
        public ref T this[UPoint16 point] => ref grid[point.x, point.y];
    }

    public enum NodeMovementType : byte {
        PureHorizontal,
        Step,
        Jump,
        Fall
    }

    [Flags]
    private enum TileFlags : byte {
        Empty = 0,

        /// <summary>
        /// For tiles/spaces that are completely solid, and cannot be moved through.
        /// This includes non-actuated solid tiles.
        /// </summary>
        Solid = 1,

        /// <summary>
        /// For tiles/spaces that can't be navigated through, but aren't necessarily
        /// solid.
        /// </summary>
        Impassable = 2,

        /// <summary>
        /// For tiles/spaces that have collision from the top. This primarily includes platforms.
        /// </summary>
        SolidTop = 4,

        /// <summary>
        /// For tiles that can be stepped up if the pathfinder is coming from the right, and moving
        /// to the left.
        /// </summary>
        CanStepWhenComingFromRight = 8,

        /// <summary>
        /// For tiles that can be stepped up if the pathfinder is coming from the left, and moving
        /// to the right.
        /// </summary>
        CanStepWhenComingFromLeft = 16
    }

    private const int MaxNodeSearch = 2000;
    private const int ManhattanEstimateTuneValue = 2;

    private const int MaxJumpHeight = 6;

    private readonly UPoint16 _topLeftOfGrid;
    private readonly ushort _rectSizeX;
    private readonly ushort _rectSizeY;
    private readonly ushort _gridSizeX;
    private readonly ushort _gridSizeY;
    private readonly PointGrid<InternalNode> _nodeGrid;
    private readonly PointGrid<TileData> _tileGrid;
    private readonly PriorityQueue<OpenNode, UPoint16> _openQueue;
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
        _openQueue = new PriorityQueue<OpenNode, UPoint16>(Comparer<UPoint16>.Create((pointOne, pointTwo) => _nodeGrid[pointOne].f.CompareTo(_nodeGrid[pointTwo].f)));
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
                bool isClosedDoor = TileLoader.CloseDoorID(tile) > 0;

                if (hasTile && !isActuated && isSolid && !isClosedDoor) {
                    if (!isPlatform) {
                        grid[i, j] = new TileData(0, TileFlags.Solid | (TileID.Sets.IgnoredByNpcStepUp[tile.TileType] ? TileFlags.Empty : TileFlags.CanStepWhenComingFromLeft | TileFlags.CanStepWhenComingFromRight));
                    }
                    else {
                        TileFlags slopeFlag = tile.Slope switch {
                            SlopeType.SlopeUpRight => TileFlags.CanStepWhenComingFromLeft,
                            SlopeType.SlopeUpLeft => TileFlags.CanStepWhenComingFromRight,
                            _ => TileFlags.Empty
                        };

                        grid[i, j] = new TileData(0, TileFlags.SolidTop | slopeFlag);
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

    private bool StartAndEndPointsAreValid() =>
        RectangleHasNoTiles(_start, _rectSizeX, _rectSizeY)
        && RectangleHasNoTiles(_end, _rectSizeX, _rectSizeY)
        && PointOnStandableTile(_start, _rectSizeX)
        && PointOnStandableTile(_end, _rectSizeX);

    private void InitiatePathingVariables() {
        _reachedGoal = _pathfindingStopped = false;
        _nodesClosed = 0;
        _currentOpenNodeValue += 2;
        _currentClosedNodeValue += 2;
        _openQueue.Clear();

        _nodeGrid[_start] = new InternalNode {
            g = 0,
            f = ManhattanEstimateTuneValue,
            parentPos = _start,
            nodeStatusInteger = _currentOpenNodeValue,
            movementType = NodeMovementType.PureHorizontal
        };

        _openQueue.Enqueue(new OpenNode(_start, NodeMovementType.PureHorizontal), _start);
    }

    private void DoPathfindingLoop() {
        // TODO: Benchmark, optimize
        while (_openQueue.Count > 0 && !_pathfindingStopped) {
            OpenNode curOpenNode = _openQueue.Dequeue();
            UPoint16 curNodePos = curOpenNode.NodePos;

            if (_nodeGrid[curNodePos].nodeStatusInteger == _currentClosedNodeValue) {
                continue;
            }

            if (curNodePos == _end) {
                _nodeGrid[_nodeGrid[curNodePos].parentPos].movementType = curOpenNode.MovementType;
                _nodeGrid[curNodePos].nodeStatusInteger = _currentClosedNodeValue;
                _reachedGoal = true;
                break;
            }

            if (_nodesClosed > MaxNodeSearch) {
                _pathfindingStopped = true;
                break;
            }


            // Direct Horizontal Movement
            for (int k = -1; k < 2; k += 2) {
                UPoint16 nextNodePos = new(curNodePos.x + k, curNodePos.y);

                if (!RectangleHasNoTiles(nextNodePos, _rectSizeX, _rectSizeY) || !PointOnStandableTile(nextNodePos, _rectSizeX)) {
                    continue;
                }

                DoSuccessorChecksAndCalculations(curOpenNode, nextNodePos, 0, NodeMovementType.PureHorizontal);
            }

            // One tile move up (step or jump)
            if (RectangleHasNoTiles(new UPoint16(curNodePos.x, curNodePos.y - 1), _rectSizeX, _rectSizeY)) {
                bool fromLeft = false;
                for (int k = -1; k < 2; k += 2) {
                    UPoint16 nextNodePos = new(curNodePos.x + k, curNodePos.y - 1);
                    if (RectangleHasNoTiles(nextNodePos, _rectSizeX, _rectSizeY) && PointOnStandableTile(nextNodePos, _rectSizeX)) {
                        DoSuccessorChecksAndCalculations(curOpenNode, nextNodePos, 0, NPCCanStepUp(nextNodePos, _rectSizeX, fromLeft) ? NodeMovementType.Step : NodeMovementType.Jump);
                    }

                    fromLeft = !fromLeft;
                }
            }

            // One tile move down
            for (int k = -1; k < 2; k += 2) {
                UPoint16 nextNodePos = new(curNodePos.x + k, curNodePos.y + 1);
                if (!RectangleHasNoTiles(nextNodePos, _rectSizeX, _rectSizeY) || !PointOnStandableTile(nextNodePos, _rectSizeX)) {
                    continue;
                }

                DoSuccessorChecksAndCalculations(curOpenNode, nextNodePos, 0, NodeMovementType.Step);
            }

            // Straight Fall Movement
            AttemptFallFromPos(curNodePos, curOpenNode);

            // Downward Ledge Movement
            for (int k = -1; k < 2; k += 2) {
                UPoint16 startNodePos = new(curNodePos.x + k, curNodePos.y);

                if (!RectangleHasNoTiles(startNodePos, _rectSizeX, _rectSizeY)) {
                    continue;
                }

                AttemptFallFromPos(startNodePos, curOpenNode);
            }

            // Vertical Only Jump Movement
            for (int j = 2; j < MaxJumpHeight; j++) {
                UPoint16 nextNodePos = new(curNodePos.x, curNodePos.y - j);
                if (!RectangleHasNoTiles(nextNodePos, _rectSizeX, _rectSizeY)) {
                    break;
                }

                if (!PointOnStandableTile(nextNodePos, _rectSizeX)) {
                    continue;
                }

                DoSuccessorChecksAndCalculations(curOpenNode, nextNodePos, (ushort)(j * 2), NodeMovementType.Jump);
            }

            // Ledge Jump Movement
            for (int j = 2; j < MaxJumpHeight; j++) {
                UPoint16 nextNodePos = new(curNodePos.x, curNodePos.y - j);

                if (!RectangleHasNoTiles(nextNodePos, _rectSizeX, _rectSizeY)) {
                    break;
                }

                for (int k = -1; k < 2; k += 2) {
                    nextNodePos = new UPoint16(curNodePos.x + k, nextNodePos.y);
                    if (!RectangleHasNoTiles(nextNodePos, _rectSizeX, _rectSizeY) || !PointOnStandableTile(nextNodePos, _rectSizeX)) {
                        continue;
                    }

                    DoSuccessorChecksAndCalculations(curOpenNode, nextNodePos, (ushort)(j * 2), NodeMovementType.Jump);
                }
            }
        }
    }

    private void AttemptFallFromPos(UPoint16 startPos, OpenNode curOpenNode) {
        for (UPoint16 nextNodePos = new(startPos.x, startPos.y + 1); nextNodePos.y < _gridSizeY - 2; nextNodePos.y++) {
            if (!RectangleHasNoTiles(nextNodePos, _rectSizeX, _rectSizeY)) {
                break;
            }
            if (!PointOnStandableTile(nextNodePos, _rectSizeX)) {
                continue;
            }

            DoSuccessorChecksAndCalculations(curOpenNode, nextNodePos, (ushort)Math.Round((nextNodePos.y - startPos.y) * 1.5f), NodeMovementType.Fall);
            break;
        }
    }

    private void DoSuccessorChecksAndCalculations(OpenNode curOpenNode, UPoint16 nextNodePos, ushort gCostModifier, NodeMovementType movementType) {
        UPoint16 curNodePos = curOpenNode.NodePos;
        ushort newG = (ushort)(_nodeGrid[curNodePos].g + _tileGrid[nextNodePos].weight + gCostModifier);

        _nodeGrid[_nodeGrid[curNodePos].parentPos].movementType = curOpenNode.MovementType;
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

        _openQueue.Enqueue(new OpenNode(nextNodePos, movementType), nextNodePos);
    }

    private List<PathNode> FinalizePath() {
        if (!_reachedGoal) {
            _pathfindingStopped = true;
            return null;
        }

        List<PathNode> foundPath = new();
        InternalNode curInternalNode = _nodeGrid[_end];
        PathNode curPathNode = new(_end, curInternalNode.parentPos, NodeMovementType.PureHorizontal);

        while (curPathNode.NodePos != curPathNode.ParentNodePos) {
            foundPath.Add(curPathNode);
            curInternalNode = _nodeGrid[curPathNode.ParentNodePos];
            curPathNode = new PathNode(curPathNode.ParentNodePos, curInternalNode.parentPos, curInternalNode.movementType);
        }

        foundPath.Add(curPathNode);
        _pathfindingStopped = true;
        return foundPath;
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

    private bool RectangleHasNoTiles(UPoint16 bottomLeft, ushort sizeX, ushort sizeY) {
        if (!RectangleWithinGrid(bottomLeft, sizeX, sizeY)) {
            return false;
        }

        for (ushort i = bottomLeft.x; i < bottomLeft.x + sizeX; i++) {
            for (ushort j = bottomLeft.y; j > bottomLeft.y - sizeY; j--) {
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

    private bool PointOnStandableTile(UPoint16 bottomLeft, ushort rectWidth) {
        if (!RectangleWithinGrid(bottomLeft + new UPoint16(0, 1), rectWidth, 1)) {
            return false;
        }

        for (ushort i = 0; i < rectWidth; i++) {
            TileFlags tileFlags = _tileGrid[new UPoint16(bottomLeft.x + i, bottomLeft.y + 1)].flags;
            if (tileFlags.HasFlag(TileFlags.Solid) || tileFlags.HasFlag(TileFlags.SolidTop)) {
                return true;
            }
        }

        return false;
    }

    private bool NPCCanStepUp(UPoint16 wantedStepPos, ushort rectWidth, bool fromLeft) {
        TileFlags wantedFlag = fromLeft ? TileFlags.CanStepWhenComingFromLeft : TileFlags.CanStepWhenComingFromRight;
        for (ushort i = 0; i < rectWidth; i++) {
            if (_tileGrid[new UPoint16(wantedStepPos.x + i, wantedStepPos.y + 1)].flags.HasFlag(wantedFlag)) {
                return true;
            }
        }

        return false;
    }
}