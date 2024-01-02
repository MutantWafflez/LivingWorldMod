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

namespace LivingWorldMod.Library.AStarPathfinding;

public enum HeuristicFormula : byte {
    Manhattan = 1,
    MaxDXDY = 2,
    DiagonalShortCut = 3,
    Euclidean = 4,
    EuclideanNoSQR = 5,
    Custom1 = 6
}