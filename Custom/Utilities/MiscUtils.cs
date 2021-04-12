using Terraria;

namespace LivingWorldMod.Custom.Utilities {

    /// <summary>
    /// Utility class that handles any methods that do not meet any pre-determined specifications to
    /// be inside of another utility class.
    /// </summary>
    public static class MiscUtils {

        public static T RandFrom<T>(params T[] values) {
            return values[Main.rand.Next(values.Length)];
        }
    }
}