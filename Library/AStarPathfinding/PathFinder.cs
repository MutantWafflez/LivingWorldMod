//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
//  REMAINS UNCHANGED.
//
//  Email:  gustavo_franco@hotmail.com
//
//  Copyright (C) 2006 Franco, Gustavo 
//

// Modified by MutantWafflez for usage within a Terraria XNA/FNA context.
// All Source files in their original form can be found at https://www.codeproject.com/articles/15307/a-algorithm-implementation-in-c

using System;
using System.Collections.Generic;
using LivingWorldMod.Library.AStarPathfinding.Nodes;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Library.AStarPathfinding;

public class PathFinder {
    #region Constructors

    public PathFinder(Point topLeftOfGrid, ushort gridSideLength, int rectSizeX, int rectSizeY) : this(topLeftOfGrid, gridSideLength, gridSideLength, rectSizeX, rectSizeY) { }

    public PathFinder(Point topLeftOfGrid, ushort gridWidth, ushort gridHeight, int rectSizeX, int rectSizeY) {
        _topLeftOfGrid = topLeftOfGrid;
        this.rectSizeX = rectSizeX;
        this.rectSizeY = rectSizeY;

        mGridX = gridWidth;
        mGridY = gridHeight;
        mGridXMinus1 = (ushort)(mGridX - 1);
        mGridYLog2 = (ushort)Math.Log(mGridY, 2);

        // This should be done at the constructor, for now we leave it here.
        if (Math.Log(mGridX, 2) != (int)Math.Log(mGridX, 2) ||
            Math.Log(mGridY, 2) != (int)Math.Log(mGridY, 2)) {
            throw new Exception("Invalid Grid, size in X and Y must be power of 2");
        }

        mCalcGrid = new InternalPathFinderNode[mGridX * mGridY];
        mOpen = new PriorityQueueB<int>(new ComparePFNodeMatrix(mCalcGrid));
    }

    #endregion

    #region Inner Structs/Classes

    public class ComparePFNodeMatrix : IComparer<int> {
        #region Variables Declaration

        private readonly InternalPathFinderNode[] mMatrix;

        #endregion

        #region Constructors

        public ComparePFNodeMatrix(InternalPathFinderNode[] matrix) {
            mMatrix = matrix;
        }

        #endregion

        #region IComparer Members

        public int Compare(int a, int b) {
            if (mMatrix[a].F > mMatrix[b].F) {
                return 1;
            }
            if (mMatrix[a].F < mMatrix[b].F) {
                return -1;
            }
            return 0;
        }

        #endregion
    }

    public readonly struct GridObject {
        private readonly byte _innerByte;

        public GridObject(GridNodeType type, byte movementWeight) {
            // Smushes the movement weight into the higher 6 bits, and the type into the lower 2 bits.
            // Weight Type
            // 000000  00
            _innerByte = (byte)((byte)(movementWeight << 2) + ((byte)type & 0x03));
        }

        public byte MovementWeight => (byte)((uint)_innerByte >> 2);

        public GridNodeType ObjectType => (GridNodeType)(_innerByte & 0x03);

        public override string ToString() => $"Weight: {MovementWeight}, Type: {ObjectType}";
    }

    #endregion

    #region Variables Declaration

    // Heap variables are initializated to default, but I like to do it anyway
    protected readonly PriorityQueueB<int> mOpen;
    protected readonly List<PathFinderNode> mClose = new();
    protected bool mStop;
    protected int mHoriz;
    protected bool mDiagonals = true;
    protected readonly InternalPathFinderNode[] mCalcGrid;
    protected byte mOpenNodeValue = 1;
    protected byte mCloseNodeValue = 2;

    //Promoted local variables to member variables to avoid recreation between calls
    protected int mH;
    protected int mLocation;
    protected int mNewLocation;
    protected ushort mLocationX;
    protected ushort mLocationY;
    protected ushort mNewLocationX;
    protected ushort mNewLocationY;
    protected int mCloseNodeCounter;
    protected readonly ushort mGridX;
    protected readonly ushort mGridY;
    protected readonly ushort mGridXMinus1;
    protected readonly ushort mGridYLog2;
    protected bool mFound;
    protected sbyte[,] mDirection = { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 }, { 1, -1 }, { 1, 1 }, { -1, 1 }, { -1, -1 } };
    protected int mEndLocation;
    protected int mNewG;

    // "Parameter" variables
    private readonly Point _topLeftOfGrid;
    protected Point start;
    protected Point end;
    protected int rectSizeX;
    protected int rectSizeY;

    #endregion

    #region Properties

    public bool Stopped {
        get;
        protected set;
    } = true;

    public HeuristicFormula Formula {
        get;
        set;
    } = HeuristicFormula.Manhattan;

    public bool Diagonals {
        get => mDiagonals;
        set {
            mDiagonals = value;
            mDirection = mDiagonals ? new sbyte[,] { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 }, { 1, -1 }, { 1, 1 }, { -1, 1 }, { -1, -1 } } : new sbyte[,] { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 } };
        }
    }

    public bool HeavyDiagonals {
        get;
        set;
    } = false;

    public int HeuristicEstimate {
        get;
        set;
    } = 2;

    public bool PunishChangeDirection {
        get;
        set;
    } = false;

    public bool ReopenCloseNodes {
        get;
        set;
    } = true;

    public bool TieBreaker {
        get;
        set;
    } = false;

    public int SearchLimit {
        get;
        set;
    } = 2000;

    #endregion

    #region Methods

    public List<PathFinderNode> FindPath(Point startPoint, Point endPoint) {
        lock (this) {
            // Is faster if we don't clear the matrix, just assign different values for open and close and ignore the rest
            // I could have user Array.Clear() but using unsafe code is faster, no much but it is.
            //fixed (PathFinderNodeFast* pGrid = tmpGrid) 
            //    ZeroMemory((byte*) pGrid, sizeof(PathFinderNodeFast) * 1000000);

            start = startPoint;
            end = endPoint;

            if (!EnsureStartAndEndValidity()) {
                return null;
            }

            InitiatePathingVariables();

            DoPathfindingLoop();

            return FinalizePath();
        }
    }

    protected virtual bool EnsureStartAndEndValidity() {
        // If the path is outside of the bounds of the grid, return null
        if (!PointWithinGrid(start.X, start.Y) || !PointWithinGrid(end.X, end.Y)) {
            return false;
        }

        // If the end position does not fit the entity, then obviously we can't pathfind to it
        if (!RectangleHasNoTiles(end.X, end.Y, rectSizeX, rectSizeY)) {
            return false;
        }

        return true;
    }

    protected virtual void InitiatePathingVariables() {
        mFound = false;
        mStop = false;
        Stopped = false;
        mCloseNodeCounter = 0;
        mOpenNodeValue += 2;
        mCloseNodeValue += 2;
        mOpen.Clear();
        mClose.Clear();

        mLocation = (start.Y << mGridYLog2) + start.X;
        mEndLocation = (end.Y << mGridYLog2) + end.X;
        mCalcGrid[mLocation].G = 0;
        mCalcGrid[mLocation].F = HeuristicEstimate;
        mCalcGrid[mLocation].PX = (ushort)start.X;
        mCalcGrid[mLocation].PY = (ushort)start.Y;
        mCalcGrid[mLocation].Status = mOpenNodeValue;

        mOpen.Push(mLocation);
    }

    protected virtual void DoPathfindingLoop() {
        while (mOpen.Count > 0 && !mStop) {
            mLocation = mOpen.Pop();

            //Is it in closed list? means this node was already processed
            if (mCalcGrid[mLocation].Status == mCloseNodeValue) {
                continue;
            }

            mLocationX = (ushort)(mLocation & mGridXMinus1);
            mLocationY = (ushort)(mLocation >> mGridYLog2);

            if (mLocation == mEndLocation) {
                mCalcGrid[mLocation].Status = mCloseNodeValue;
                mFound = true;
                break;
            }

            if (mCloseNodeCounter > SearchLimit) {
                Stopped = true;
                return;
            }

            if (PunishChangeDirection) {
                mHoriz = mLocationX - mCalcGrid[mLocation].PX;
            }


            for (int i = 0; i < (mDiagonals ? 8 : 4); i++) {
                mNewLocationX = (ushort)(mLocationX + mDirection[i, 0]);
                mNewLocationY = (ushort)(mLocationY + mDirection[i, 1]);
                mNewLocation = (mNewLocationY << mGridYLog2) + mNewLocationX;

                if (mNewLocationX >= mGridX || mNewLocationY >= mGridY) {
                    continue;
                }

                if (mCalcGrid[mNewLocation].Status == mCloseNodeValue && !ReopenCloseNodes) {
                    continue;
                }

                if (!RectangleHasNoTiles(mNewLocationX, mNewLocationY, rectSizeX, rectSizeY)) {
                    continue;
                }

                if (HeavyDiagonals && i > 3) {
                    mNewG = mCalcGrid[mLocation].G + (int)(GetGridData(mNewLocationX, mNewLocationY).MovementWeight * 2.41);
                }
                else {
                    mNewG = mCalcGrid[mLocation].G + GetGridData(mNewLocationX, mNewLocationY).MovementWeight;
                }

                if (PunishChangeDirection) {
                    if (mNewLocationX - mLocationX != 0) {
                        if (mHoriz == 0) {
                            mNewG += Math.Abs(mNewLocationX - end.X) + Math.Abs(mNewLocationY - end.Y);
                        }
                    }
                    if (mNewLocationY - mLocationY != 0) {
                        if (mHoriz != 0) {
                            mNewG += Math.Abs(mNewLocationX - end.X) + Math.Abs(mNewLocationY - end.Y);
                        }
                    }
                }


                //Is it open or closed?
                if (mCalcGrid[mNewLocation].Status == mOpenNodeValue || mCalcGrid[mNewLocation].Status == mCloseNodeValue) {
                    // The current node has less code than the previous? then skip this node
                    if (mCalcGrid[mNewLocation].G <= mNewG) {
                        continue;
                    }
                }

                mCalcGrid[mNewLocation].PX = mLocationX;
                mCalcGrid[mNewLocation].PY = mLocationY;
                mCalcGrid[mNewLocation].G = mNewG;

                switch (Formula) {
                    default:
                    case HeuristicFormula.Manhattan:
                        mH = HeuristicEstimate * (Math.Abs(mNewLocationX - end.X) + Math.Abs(mNewLocationY - end.Y));
                        break;
                    case HeuristicFormula.MaxDXDY:
                        mH = HeuristicEstimate * Math.Max(Math.Abs(mNewLocationX - end.X), Math.Abs(mNewLocationY - end.Y));
                        break;
                    case HeuristicFormula.DiagonalShortCut:
                        int h_diagonal = Math.Min(Math.Abs(mNewLocationX - end.X), Math.Abs(mNewLocationY - end.Y));
                        int h_straight = Math.Abs(mNewLocationX - end.X) + Math.Abs(mNewLocationY - end.Y);
                        mH = HeuristicEstimate * 2 * h_diagonal + HeuristicEstimate * (h_straight - 2 * h_diagonal);
                        break;
                    case HeuristicFormula.Euclidean:
                        mH = (int)(HeuristicEstimate * Math.Sqrt(Math.Pow(mNewLocationY - end.X, 2) + Math.Pow(mNewLocationY - end.Y, 2)));
                        break;
                    case HeuristicFormula.EuclideanNoSQR:
                        mH = (int)(HeuristicEstimate * (Math.Pow(mNewLocationX - end.X, 2) + Math.Pow(mNewLocationY - end.Y, 2)));
                        break;
                    case HeuristicFormula.Custom1:
                        Point dxy = new(Math.Abs(end.X - mNewLocationX), Math.Abs(end.Y - mNewLocationY));
                        int Orthogonal = Math.Abs(dxy.X - dxy.Y);
                        int Diagonal = Math.Abs((dxy.X + dxy.Y - Orthogonal) / 2);
                        mH = HeuristicEstimate * (Diagonal + Orthogonal + dxy.X + dxy.Y);

                        break;
                }
                if (TieBreaker) {
                    int dx1 = mLocationX - end.X;
                    int dy1 = mLocationY - end.Y;
                    int dx2 = start.X - end.X;
                    int dy2 = start.Y - end.Y;
                    int cross = Math.Abs(dx1 * dy2 - dx2 * dy1);
                    mH = (int)(mH + cross * 0.001);
                }
                mCalcGrid[mNewLocation].F = mNewG + mH;

                //It is faster if we leave the open node in the priority queue
                //When it is removed, it will be already closed, it will be ignored automatically
                //if (tmpGrid[newLocation].Status == 1)
                //{
                //    //int removeX   = newLocation & gridXMinus1;
                //    //int removeY   = newLocation >> gridYLog2;
                //    mOpen.RemoveLocation(newLocation);
                //}

                //if (tmpGrid[newLocation].Status != 1)
                //{
                mOpen.Push(mNewLocation);
                //}
                mCalcGrid[mNewLocation].Status = mOpenNodeValue;
            }


            mCloseNodeCounter++;
            mCalcGrid[mLocation].Status = mCloseNodeValue;
        }
    }

    protected virtual List<PathFinderNode> FinalizePath() {
        if (mFound) {
            mClose.Clear();

            InternalPathFinderNode fNodeTmp = mCalcGrid[(end.Y << mGridYLog2) + end.X];
            PathFinderNode fNode = new() {
                F = fNodeTmp.F,
                G = fNodeTmp.G,
                H = 0,
                PX = fNodeTmp.PX,
                PY = fNodeTmp.PY,
                X = end.X,
                Y = end.Y
            };

            while (fNode.X != fNode.PX || fNode.Y != fNode.PY) {
                mClose.Add(fNode);
                int posX = fNode.PX;
                int posY = fNode.PY;
                fNodeTmp = mCalcGrid[(posY << mGridYLog2) + posX];

                fNode.F = fNodeTmp.F;
                fNode.G = fNodeTmp.G;
                fNode.H = 0;
                fNode.PX = fNodeTmp.PX;
                fNode.PY = fNodeTmp.PY;
                fNode.X = posX;
                fNode.Y = posY;
            }

            mClose.Add(fNode);
            Stopped = true;

            return mClose;
        }
        Stopped = true;
        return null;
    }

    protected GridObject GetGridData(int x, int y) {
        Tile tile = Main.tile[_topLeftOfGrid + new Point(x, y)];

        bool hasTile = tile.HasTile;
        bool isActuated = tile.IsActuated;
        bool isSolid = Main.tileSolid[tile.TileType];
        bool isPlatform = TileID.Sets.Platforms[tile.TileType];
        bool isDoor = TileLoader.OpenDoorID(tile) > 0 || TileLoader.CloseDoorID(tile) > 0;
        bool hasLava = tile is { LiquidAmount: > 0, LiquidType: LiquidID.Lava };
        bool hasShimmer = tile is { LiquidAmount: >= 200, LiquidType: LiquidID.Shimmer };

        return hasTile switch {
            true when !isActuated && isSolid && !isDoor && !isPlatform => new GridObject(GridNodeType.Solid, 0),
            true when !isActuated && isPlatform => new GridObject(GridNodeType.SolidTop, 0),
            false when hasLava || hasShimmer => new GridObject(GridNodeType.Impassable, 0),
            _ => new GridObject(GridNodeType.NonSolid, GetTileMovementCost(tile))
        };
    }

    protected virtual byte GetTileMovementCost(Tile tile) {
        byte tileCost = 1;

        if (TileID.Sets.AvoidedByNPCs[tile.TileType]) {
            tileCost += 8;
        }

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

    /// <summary>
    /// Returns whether or not the passed in rectangular parameters form a rectangle with no tiles in it.
    /// </summary>
    /// <remarks>
    /// Does automatic bound checking for the rectangle.
    /// </remarks>
    protected bool RectangleHasNoTiles(int bottomLeftTileX, int bottomLeftTileY, int sizeX, int sizeY) {
        if (!RectangleWithinGrid(bottomLeftTileX, bottomLeftTileY - sizeY + 1, sizeX, sizeY)) {
            return false;
        }

        for (int i = bottomLeftTileX; i < bottomLeftTileX + sizeX; i++) {
            for (int j = bottomLeftTileY; j > bottomLeftTileY - sizeY; j--) {
                if (GetGridData(i, j).ObjectType is GridNodeType.Solid or GridNodeType.Impassable) {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Whether or not a specific, single point is within the grid.
    /// </summary>
    protected bool PointWithinGrid(int pointX, int pointY) => RectangleWithinGrid(pointX, pointY, 1, 1);

    /// <summary>
    /// Returns whether or not the passed in rectangular parameters form a rectangle within the grid, starting from the top
    /// left.
    /// </summary>
    protected bool RectangleWithinGrid(int topLeftX, int topLeftY, int sizeX, int sizeY) {
        ushort outerBoundX = (ushort)(topLeftX + sizeX - 1);
        ushort outerBoundY = (ushort)(topLeftY + sizeY - 1);

        return (ushort)topLeftX < mGridX && (ushort)topLeftY < mGridY && outerBoundX < mGridX && outerBoundY < mGridY;
    }

    #endregion
}