using Microsoft.Xna.Framework;

namespace LivingWorldMod.Utilities;

// Utilities class that holds methods which deal with specifically dust.
public static partial class LWMUtils {
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
            Vector2 newPos = origin - new Vector2(0, radius).RotatedBy(MathHelper.ToRadians(i));

            Dust newDust = Dust.NewDustPerfect(newPos, dust.type, dust.velocity, dust.alpha, dust.color, dust.scale);

            newDust.fadeIn = dust.fadeIn;
            newDust.noGravity = dust.noGravity;
            newDust.rotation = dust.rotation;
            newDust.noLight = dust.noLight;
            newDust.frame = dust.frame;
            newDust.shader = dust.shader;
            newDust.customData = dust.customData;
        }
    }

    /// <summary>
    /// Creates a circle of dust at the specified origin with the passed in radius, dust, and
    /// optional angle change. Radius is in terms of pixels.
    /// </summary>
    /// <param name="origin"> The origin of the dust circle in world coordinates. </param>
    /// <param name="radius"> The radius of the dust circle. </param>
    /// <param name="dustID"> The ID of the dust to compose the circle of. </param>
    /// <param name="angleChange">
    /// The angle change between each dust particle in the circle. Defaults to 5 degrees.
    /// </param>
    public static void CreateCircle(Vector2 origin, float radius, int dustID, Vector2? velocity = null, int alpha = 0, Color newColor = default, float scale = 1f, float angleChange = 5) {
        for (float i = 0; i < 360f; i += angleChange) {
            Dust.NewDustPerfect(origin - new Vector2(0, radius).RotatedBy(MathHelper.ToRadians(i)), dustID, velocity, alpha, newColor, scale);
        }
    }
}