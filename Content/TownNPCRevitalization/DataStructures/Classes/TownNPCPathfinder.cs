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
    private readonly InternalNode[,] _nodeGrid;
    private readonly TileData[,] _tileGrid;
    private readonly PriorityQueue<UPoint16, int> _openQueue;
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

        _nodeGrid = new InternalNode[_gridSizeX, _gridSizeY];
        _tileGrid = GenerateTileGrid();
        _openQueue = new PriorityQueue<UPoint16, int>();
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
                bool isClosedDoor = TileLoader.CloseDoorID(tile) > 0;

                if (hasTile && !isActuated && isSolid && !isClosedDoor) {
                    grid[i, j] = new TileData(0, TileFlags.Solid | (TileID.Sets.IgnoredByNpcStepUp[tile.TileType] ? TileFlags.Empty : TileFlags.CanStepWhenComingFromLeft | TileFlags.CanStepWhenComingFromRight));
                    continue;
                }

                bool isPlatform = TileID.Sets.Platforms[tile.TileType];
                if (hasTile && !isActuated && isPlatform) {
                    TileFlags slopeFlag = tile.Slope switch {
                        SlopeType.SlopeUpRight => TileFlags.CanStepWhenComingFromLeft,
                        SlopeType.SlopeUpLeft => TileFlags.CanStepWhenComingFromRight,
                        _ => TileFlags.Empty
                    };

                    grid[i, j] = new TileData(0, TileFlags.SolidTop | slopeFlag);
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
        RectangleWithinGrid(_start, _rectSizeX, _rectSizeY)
        && RectangleWithinGrid(_end, _rectSizeX, _rectSizeY)
        && RectangleHasNoTiles(_start, _rectSizeX, _rectSizeY)
        && RectangleHasNoTiles(_end, _rectSizeX, _rectSizeY)
        && PointOnStandableTiles(_start, _rectSizeX, true)
        && PointOnStandableTiles(_end, _rectSizeX, true);

    private void InitiatePathingVariables() {
        _reachedGoal = _pathfindingStopped = false;
        _nodesClosed = 0;
        _currentOpenNodeValue += 2;
        _currentClosedNodeValue += 2;
        _openQueue.Clear();

        _nodeGrid[_start.x, _start.y] = new InternalNode {
            g = 0,
            f = ManhattanEstimateTuneValue,
            parentPos = _start,
            nodeStatusInteger = _currentOpenNodeValue
        };

        _openQueue.Enqueue(_start, ManhattanEstimateTuneValue);
    }

    private void DoPathfindingLoop() {
        while (_openQueue.Count > 0 && !_pathfindingStopped) {
            UPoint16 curNodePos = _openQueue.Dequeue();
            if (_nodeGrid[curNodePos.x, curNodePos.y].nodeStatusInteger == _currentClosedNodeValue) {
                continue;
            }

            if (curNodePos == _end) {
                _nodeGrid[curNodePos.x, curNodePos.y].nodeStatusInteger = _currentClosedNodeValue;
                _reachedGoal = true;
                break;
            }

            if (_nodesClosed > MaxNodeSearch) {
                _pathfindingStopped = true;
                return;
            }

            // Horizontal Left
            UPoint16 nextNodePos = new(curNodePos.x - 1, curNodePos.y);
            bool directLeftInBounds = PointWithinGrid(nextNodePos);
            if (directLeftInBounds && RectangleHasNoTiles(nextNodePos, 1, _rectSizeY) && PointOnStandableTiles(nextNodePos, _rectSizeX)) {
                DoSuccessorChecksAndCalculations(curNodePos, nextNodePos, 0, NodeMovementType.PureHorizontal);
            }

            // Horizontal Right
            nextNodePos = new UPoint16(curNodePos.x + 1, curNodePos.y);
            UPoint16 boundCheckPos = nextNodePos + new UPoint16(Math.Max(_rectSizeX - 1, 1), 0);
            bool directRightInBounds = PointWithinGrid(boundCheckPos);
            if (directRightInBounds && RectangleHasNoTiles(boundCheckPos, 1, _rectSizeY) && PointOnStandableTiles(nextNodePos, _rectSizeX)) {
                DoSuccessorChecksAndCalculations(curNodePos, nextNodePos, 0, NodeMovementType.PureHorizontal);
            }

            nextNodePos = new UPoint16(curNodePos.x, curNodePos.y - 1);
            boundCheckPos = nextNodePos + new UPoint16(0, -Math.Max(_rectSizeY - 1, 1));
            bool directUpInBounds = PointWithinGrid(boundCheckPos);
            if (directUpInBounds && RectangleHasNoTiles(boundCheckPos, _rectSizeX, 1)) {
                // Upward Left Step
                nextNodePos = new UPoint16(curNodePos.x - 1, curNodePos.y - 1);
                if (directLeftInBounds && RectangleHasNoTiles(nextNodePos, _rectSizeX, _rectSizeY) && NPCCanStepUp(nextNodePos, _rectSizeX, true)) {
                    DoSuccessorChecksAndCalculations(curNodePos, nextNodePos, 1, NodeMovementType.Step);
                }

                // Upward Right Step
                nextNodePos = new UPoint16(curNodePos.x + 1, curNodePos.y - 1);
                if (directRightInBounds && RectangleHasNoTiles(nextNodePos, _rectSizeX, _rectSizeY) && NPCCanStepUp(nextNodePos, _rectSizeX, false)) {
                    DoSuccessorChecksAndCalculations(curNodePos, nextNodePos, 1, NodeMovementType.Step);
                }

                // Vertical Only Jump Movement
                for (int j = 1; j < MaxJumpHeight; j++) {
                    nextNodePos = new UPoint16(curNodePos.x, curNodePos.y - j);

                    if (!PointWithinGrid(nextNodePos) || !RectangleHasNoTiles(nextNodePos, _rectSizeX, 1)) {
                        break;
                    }

                    if (PointOnStandableTiles(nextNodePos, _rectSizeX)) {
                        DoSuccessorChecksAndCalculations(curNodePos, nextNodePos, (ushort)(j * 2), NodeMovementType.Jump);
                    }
                }

                // Ledge Jump Movement
                for (int j = 1; j < MaxJumpHeight; j++) {
                    nextNodePos = new UPoint16(curNodePos.x, curNodePos.y - j);

                    if (!PointWithinGrid(nextNodePos) || !RectangleHasNoTiles(nextNodePos, _rectSizeX, _rectSizeY)) {
                        break;
                    }

                    // Left
                    nextNodePos = new UPoint16(curNodePos.x - 1, nextNodePos.y);
                    if (directLeftInBounds && RectangleHasNoTiles(nextNodePos, _rectSizeX, _rectSizeY) && PointOnStandableTiles(nextNodePos, _rectSizeX)) {
                        DoSuccessorChecksAndCalculations(curNodePos, nextNodePos, (ushort)(1 + j * 2), NodeMovementType.Jump);
                    }

                    // Right
                    nextNodePos = new UPoint16(curNodePos.x + 1, nextNodePos.y);
                    if (directRightInBounds && RectangleHasNoTiles(nextNodePos, _rectSizeX, _rectSizeY) && PointOnStandableTiles(nextNodePos, _rectSizeX)) {
                        DoSuccessorChecksAndCalculations(curNodePos, nextNodePos, (ushort)(1 + j * 2), NodeMovementType.Jump);
                    }
                }
            }

            bool directDownInBounds = PointWithinGrid(new UPoint16(curNodePos.x, nextNodePos.y));
            if (!directDownInBounds) {
                continue;
            }

            // Down Left Step
            nextNodePos = new UPoint16(curNodePos.x - 1, curNodePos.y + 1);
            if (directLeftInBounds && RectangleHasNoTiles(nextNodePos, _rectSizeX, _rectSizeY) && PointOnStandableTiles(nextNodePos, _rectSizeX, true)) {
                DoSuccessorChecksAndCalculations(curNodePos, nextNodePos, 1, NodeMovementType.Step);
            }

            // Down Right Step
            nextNodePos = new UPoint16(curNodePos.x + 1, curNodePos.y + 1);
            if (directRightInBounds && RectangleHasNoTiles(nextNodePos, _rectSizeX, _rectSizeY) && PointOnStandableTiles(nextNodePos, _rectSizeX, true)) {
                DoSuccessorChecksAndCalculations(curNodePos, nextNodePos, 1, NodeMovementType.Step);
            }

            // Straight Fall
            CheckFallFromPos(curNodePos);

            // Left Ledge Fall
            nextNodePos = new UPoint16(curNodePos.x - 1, curNodePos.y);
            if (directLeftInBounds && RectangleHasNoTiles(nextNodePos, _rectSizeX, _rectSizeY)) {
                CheckFallFromPos(nextNodePos);
            }

            // Right Ledge Fall
            nextNodePos = new UPoint16(curNodePos.x + 1, curNodePos.y);
            if (directRightInBounds && RectangleHasNoTiles(nextNodePos, _rectSizeX, _rectSizeY)) {
                CheckFallFromPos(nextNodePos);
            }
        }
    }

    private void CheckFallFromPos(UPoint16 startPos) {
        for (ushort j = (ushort)(startPos.y + 1); j < _gridSizeY - 2; j++) {
            UPoint16 nextNodePos = new(startPos.x, j);
            if (!PointWithinGrid(nextNodePos) || !RectangleHasNoTiles(nextNodePos, _rectSizeX, 1)) {
                break;
            }

            if (!PointOnStandableTiles(nextNodePos, _rectSizeX, true)) {
                continue;
            }

            DoSuccessorChecksAndCalculations(startPos, nextNodePos, (ushort)(nextNodePos.y - startPos.y), NodeMovementType.Fall);
            break;
        }
    }

    private void DoSuccessorChecksAndCalculations(UPoint16 currentNodePos, UPoint16 nextNodePos, ushort gCostModifier, NodeMovementType movementType) {
        ushort mNewG = (ushort)(_nodeGrid[currentNodePos.x, currentNodePos.y].g + _tileGrid[nextNodePos.x, nextNodePos.y].weight + gCostModifier);
        if (_nodeGrid[nextNodePos.x, nextNodePos.y].nodeStatusInteger == _currentOpenNodeValue || _nodeGrid[nextNodePos.x, nextNodePos.y].nodeStatusInteger == _currentClosedNodeValue) {
            if (_nodeGrid[nextNodePos.x, nextNodePos.y].g <= mNewG) {
                return;
            }
        }

        _nodeGrid[currentNodePos.x, currentNodePos.y].movementType = movementType;
        _nodeGrid[nextNodePos.x, nextNodePos.y] = _nodeGrid[nextNodePos.x, nextNodePos.y] with {
            parentPos = currentNodePos,
            g = mNewG,
            // Manhattan distance heuristic
            f = (ushort)(mNewG + ManhattanEstimateTuneValue * (Math.Abs(nextNodePos.x - _end.x) + Math.Abs(nextNodePos.y - _end.y))),
            nodeStatusInteger = _currentOpenNodeValue
        };

        _openQueue.Enqueue(nextNodePos, _nodeGrid[nextNodePos.x, nextNodePos.y].f);
    }

    private List<PathNode> FinalizePath() {
        if (!_reachedGoal) {
            _pathfindingStopped = true;
            return null;
        }


        List<PathNode> foundPath = new();
        InternalNode curInternalNode = _nodeGrid[_end.x, _end.y];
        NodeMovementType parentConnectType = curInternalNode.movementType;
        PathNode curPathNode = new(_end, curInternalNode.parentPos, NodeMovementType.PureHorizontal);

        while (curPathNode.NodePos != curPathNode.ParentNodePos) {
            foundPath.Add(curPathNode);
            curInternalNode = _nodeGrid[curPathNode.ParentNodePos.x, curPathNode.ParentNodePos.y];
            NodeMovementType oldConnect = curInternalNode.movementType;

            curPathNode = new PathNode(curPathNode.ParentNodePos, curInternalNode.parentPos, parentConnectType);
            parentConnectType = oldConnect;
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
        for (int i = bottomLeft.x; i < bottomLeft.x + sizeX; i++) {
            for (int j = bottomLeft.y; j > bottomLeft.y - sizeY; j--) {
                TileFlags tileFlags = _tileGrid[i, j].flags;
                if (tileFlags.HasFlag(TileFlags.Solid) || tileFlags.HasFlag(TileFlags.Impassable)) {
                    return false;
                }
            }
        }

        return true;
    }

    private bool PointWithinGrid(UPoint16 point) => point.x < _gridSizeX && point.y < _gridSizeY;

    private bool RectangleWithinGrid(UPoint16 bottomLeft, ushort sizeX, ushort sizeY) {
        ushort outerBoundX = (ushort)(bottomLeft.x + sizeX - 1);
        ushort outerBoundY = (ushort)(bottomLeft.y - sizeY + 1);

        return bottomLeft.x < _gridSizeX && bottomLeft.y < _gridSizeY && outerBoundX < _gridSizeX && outerBoundY < _gridSizeY;
    }

    private bool PointOnStandableTiles(UPoint16 bottomLeft, ushort rectWidth, bool boundCheck = false) {
        if (boundCheck && !RectangleWithinGrid(bottomLeft, rectWidth, 1)) {
            return false;
        }

        for (int i = 0; i < rectWidth; i++) {
            TileFlags tileFlags = _tileGrid[bottomLeft.x + i, bottomLeft.y + 1].flags;
            if (tileFlags.HasFlag(TileFlags.Solid) || tileFlags.HasFlag(TileFlags.SolidTop)) {
                return true;
            }
        }

        return false;
    }

    private bool NPCCanStepUp(UPoint16 wantedStepPos, ushort rectWidth, bool fromLeft) {
        if (!PointOnStandableTiles(wantedStepPos, rectWidth)) {
            return false;
        }

        TileFlags gridObjectFlags = _tileGrid[wantedStepPos.x, wantedStepPos.y + 1].flags;
        return gridObjectFlags.HasFlag(fromLeft ? TileFlags.CanStepWhenComingFromLeft : TileFlags.CanStepWhenComingFromRight);
    }
}