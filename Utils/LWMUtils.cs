using Terraria;
using Terraria.Utilities;

namespace LivingWorldMod.Utils
{
    internal static class LWMUtils
    {
        public static void ConditionalStringAdd<T>(this WeightedRandom<T> weightedRandom, T curType, bool boolean, double weight = 1)
        {
            if (boolean)
            {
                weightedRandom.Add(curType, weight);
            }
        }
    }
}