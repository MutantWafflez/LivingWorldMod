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

namespace LivingWorldMod.Library.AStarPathfinding.Nodes;

public struct PathFinderNode {
    #region Variables Declaration

    public int F;
    public int G;
    public int H; // f = gone + heuristic
    public int X;
    public int Y;
    public int PX; // Parent
    public int PY;

    #endregion
}