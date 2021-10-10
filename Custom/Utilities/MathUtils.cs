using Microsoft.Xna.Framework;
using System;

namespace LivingWorldMod.Custom.Utilities {

    /// <summary>
    /// Utilities class that handles mathematical methods. Anything to do with Math can be used anywhere.
    /// </summary>
    public static class MathUtils {

        /// <summary>
        /// Linearly interpolates (lerps) the starting color towards the specified target color with
        /// the specified step percentage.
        /// </summary>
        /// <param name="startingColor"> The starting color. </param>
        /// <param name="targetColor"> The color to be moving towards. </param>
        /// <param name="step">
        /// The distance or "step" percent to be moving towards the target color.
        /// </param>
        public static Color ColorLerp(Color startingColor, Color targetColor, float step) {
            Color newColor = new Color(
                (int)Math.Round(MathHelper.Lerp(startingColor.R, targetColor.R, step)),
                (int)Math.Round(MathHelper.Lerp(startingColor.G, targetColor.G, step)),
                (int)Math.Round(MathHelper.Lerp(startingColor.B, targetColor.B, step)),
                (int)Math.Round(MathHelper.Lerp(startingColor.A, targetColor.A, step))
            );

            return newColor;
        }
    }
}