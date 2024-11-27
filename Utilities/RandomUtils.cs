using Terraria.Utilities;

namespace LivingWorldMod.Utilities;

public static partial class LWMUtils {
    public static double NextDouble(this UnifiedRandom self, double maxValue) => self.NextDouble() * maxValue;
}