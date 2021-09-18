using Microsoft.Xna.Framework;
using Terraria;

namespace LivingWorldMod.Custom.Utilities {

    /// <summary>
    /// Utilities class that holds methods which deal with specifically dust.
    /// </summary>
    public static class DustUtilities {

        /// <summary>
        /// Creates a circle of dust at the specified origin with the passed in radius, dust, and
        /// optional angle change. Radius is in terms of pixels.
        /// </summary>
        /// <param name="origin"> The origin of the dust circle in world coordinates. </param>
        /// <param name="radius"> The radius of the dust circle. </param>
        /// <param name="dust"> The dust to duplicate and use. </param>
        /// <param name="angleChange">
        /// The angle change between each dust particle in the circle. Defaults to 5 degrees.
        /// </param>
        public static void CreateCircle(Vector2 origin, float radius, Dust dust, float angleChange = 5) {
            for (float i = 0; i < 360f; i += angleChange) {
                Dust newDust = Dust.CloneDust(dust);
                newDust.position = origin - new Vector2(0, radius).RotatedBy(MathHelper.ToRadians(i));
            }
        }
    }
}