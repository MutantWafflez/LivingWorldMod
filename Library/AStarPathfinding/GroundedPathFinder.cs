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
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Library.AStarPathfinding;

/// <summary>
/// This PathFinder is different from <seealso cref="PathFinder"/>, as it deals specifically with pathfinding
/// on the ground, under the effects of gravity.
/// <para>
/// In short, the "grounded" approach only allows horizontal movement when the next node
/// is directly above a tile, or "on the ground." Upward vertical movement is only allowed within
/// a certain range, and is more costly the bigger the distance from the ground. Basically, the node can
/// only "jump" so high, for an analogy. Downward vertical movement is NOT more costly.
/// </para>
/// </summary>
public class GroundedPathFinder : PathFinder {
    #region Constructors

    public GroundedPathFinder(GridObject[,] grid) : base(grid) { }

    #endregion

    #region Variables Declaration

    /// <summary>
    /// The amount of tiles upwards the pathfinder must move in order for such a movement
    /// to be considered a "jump."
    /// </summary>
    public const int MinimumHeightToBeConsideredJump = 2;

    /// <summary>
    /// The amount of tiles upwards/downwards the pathfinder must move *exactly* in order
    /// for such a movement to be considered a "step."
    /// </summary>
    public const int ExactHeightToBeConsideredStep = 1;

    #endregion

    #region Methods

    protected override bool EnsureStartAndEndValidity() => base.EnsureStartAndEndValidity() && RectangleHasTileBelow(start.X, start.Y, rectSizeX) && RectangleHasTileBelow(end.X, end.Y, rectSizeX);

    protected override void DoPathfindingLoop() {
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


            // Direct Horizontal Movement
            for (int k = -1; k < 2; k += 2) {
                mNewLocationX = (ushort)(mLocationX + k);
                mNewLocationY = mLocationY;
                mNewLocation = (mNewLocationY << mGridYLog2) + mNewLocationX;

                if (RectangleHasTileBelow(mNewLocationX, mNewLocationY, rectSizeX)) {
                    DoSuccessorChecksAndCalculations(0);
                }
            }

            if (RectangleHasNoTiles(mLocationX, mLocationY - 1, rectSizeX, rectSizeY)) {
                // Upward Staircase Movement
                for (int k = -1; k < 2; k += 2) {
                    ushort stairCaseX = (ushort)(mLocationX + k);
                    ushort stairCaseY = (ushort)(mLocationY - 1);

                    if (!RectangleHasTileBelow(stairCaseX, stairCaseY, rectSizeX)) {
                        continue;
                    }

                    mNewLocationX = stairCaseX;
                    mNewLocationY = stairCaseY;
                    mNewLocation = (mNewLocationY << mGridYLog2) + mNewLocationX;
                    DoSuccessorChecksAndCalculations(1);
                }

                // Downward Staircase Movement
                for (int k = -rectSizeX; k < 2; k++) {
                    if (k == 0) {
                        continue;
                    }
                    ushort steppedDownRectX = (ushort)(mLocationX + k);
                    ushort stairCaseY = (ushort)(mLocationY + 1);

                    if (!RectangleHasTileBelow(steppedDownRectX, stairCaseY, rectSizeX)) {
                        continue;
                    }

                    mNewLocationX = steppedDownRectX;
                    mNewLocationY = stairCaseY;
                    mNewLocation = (mNewLocationY << mGridYLog2) + mNewLocationX;
                    DoSuccessorChecksAndCalculations(1);
                }
            }

            // Straight Fall Movement
            for (ushort j = (ushort)(mLocationY + 2); j < mGridY - 2; j++) {
                if (!RectangleHasNoTiles(mLocationX, j, rectSizeX, rectSizeY)) {
                    break;
                }
                if (!RectangleHasTileBelow(mLocationX, j, rectSizeX)) {
                    continue;
                }

                mNewLocationX = mLocationX;
                mNewLocationY = j;
                mNewLocation = (mNewLocationY << mGridYLog2) + mNewLocationX;
                DoSuccessorChecksAndCalculations(mNewLocationY - mLocationY);
                break;
            }

            // Downward Ledge Movement
            for (int k = -1; k < 2; k += 2) {
                ushort downLocationX = (ushort)(mLocationX + k);

                if (!RectangleHasNoTiles(downLocationX, mLocationY + 2, rectSizeX, rectSizeY)) {
                    continue;
                }

                for (ushort j = (ushort)(mLocationY + 2); j < mGridY - 2; j++) {
                    if (!RectangleHasTileBelow(downLocationX, j, rectSizeX)) {
                        continue;
                    }

                    mNewLocationX = downLocationX;
                    mNewLocationY = j;
                    mNewLocation = (mNewLocationY << mGridYLog2) + mNewLocationX;
                    DoSuccessorChecksAndCalculations(mNewLocationY - mLocationY);
                    break;
                }
            }

            // Vertical Only Jump Movement
            for (int j = 2; j < maxUpwardMovement; j++) {
                ushort upLocationY = (ushort)(mLocationY - j);

                if (!RectangleHasNoTiles(mLocationX, upLocationY, rectSizeX, rectSizeY)) {
                    break;
                }

                mNewLocationX = mLocationX;
                mNewLocationY = upLocationY;
                mNewLocation = (mNewLocationY << mGridYLog2) + mNewLocationX;

                if (RectangleHasTileBelow(mNewLocationX, mNewLocationY, rectSizeX)) {
                    DoSuccessorChecksAndCalculations(j * 2);
                }
            }

            // Ledge Jump Movement
            for (int j = 2; j < maxUpwardMovement; j++) {
                ushort upLocationY = (ushort)(mLocationY - j);

                if (!RectangleHasNoTiles(mLocationX, upLocationY, rectSizeX, rectSizeY)) {
                    break;
                }

                for (int k = -1; k < 2; k += 2) {
                    mNewLocationX = (ushort)(mLocationX + k);
                    mNewLocationY = upLocationY;
                    mNewLocation = (mNewLocationY << mGridYLog2) + mNewLocationX;

                    if (RectangleHasTileBelow(mNewLocationX, mNewLocationY, rectSizeX)) {
                        DoSuccessorChecksAndCalculations(Math.Abs(k) * 2 + j * 2);
                    }
                }
            }
        }
    }

    private void DoSuccessorChecksAndCalculations(int gCostModifier) {
        if (mNewLocationX >= mGridX || mNewLocationY >= mGridY) {
            return;
        }

        if (mCalcGrid[mNewLocation].Status == mCloseNodeValue && !ReopenCloseNodes) {
            return;
        }

        if (!RectangleHasNoTiles(mNewLocationX, mNewLocationY, rectSizeX, rectSizeY)) {
            return;
        }

        mNewG = mCalcGrid[mLocation].G + mGrid[mNewLocationX, mNewLocationY].MovementWeight;

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

        mNewG += gCostModifier;

        //Is it open or closed?
        if (mCalcGrid[mNewLocation].Status == mOpenNodeValue || mCalcGrid[mNewLocation].Status == mCloseNodeValue) {
            // The current node has less code than the previous? then skip this node
            if (mCalcGrid[mNewLocation].G <= mNewG) {
                return;
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

    /// <summary>
    /// Returns whether or not the rectangle starting from the bottom left and of the specified width
    /// has ANY WALKABLE tile below it.
    /// </summary>
    private bool RectangleHasTileBelow(int bottomLeftTileX, int bottomLeftTileY, int rectWidth) {
        if (!RectangleWithinGrid(bottomLeftTileX, bottomLeftTileY, rectWidth, 2)) {
            return false;
        }

        for (int i = 0; i < rectWidth; i++) {
            if (mGrid[bottomLeftTileX + i, bottomLeftTileY + 1].ObjectType is GridNodeType.Solid or GridNodeType.SolidTop) {
                return true;
            }
        }

        return false;
    }

    #endregion
}