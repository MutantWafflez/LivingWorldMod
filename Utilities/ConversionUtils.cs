using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace LivingWorldMod.Utilities;

// File for util functions for converting types.
public static partial class LWMUtils {
    /// <summary>
    ///     Converts a <see cref="Point" /> type to its <see cref="Point16" /> equivalent.
    /// </summary>
    public static Point16 ToPoint16(this Point point) => new (point.X, point.Y);
}