using System;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Utilities;

public static partial class LWMUtils {
    /// <summary>
    ///     Constructs a new <see cref="Rectangle" /> from two points representing opposite corners.
    ///     These corners can be any of the corners - but make sure that they are OPPOSITE
    ///     corners, such as top-left/bottom-right or bottom-left/top-right.
    /// </summary>
    public static Rectangle NewRectFromCorners(Point cornerOne, Point cornerTwo) {
        int minX = Math.Min(cornerOne.X, cornerTwo.X);
        int maxX = Math.Max(cornerOne.X, cornerTwo.X);
        int minY = Math.Min(cornerOne.Y, cornerTwo.Y);
        int maxY = Math.Max(cornerOne.Y, cornerTwo.Y);

        return new Rectangle(
            minX,
            minY,
            maxX - minX,
            maxY - minY
        );
    }
}