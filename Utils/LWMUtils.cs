using Terraria;
using Terraria.DataStructures;
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

        //----------Extension Methods----------
        public static Tile ToTile(this TileNode tn) => Framing.GetTileSafely(tn.position);
        public static Point16 Add(this Point16 point, int p1x, int p1y) => point + new Point16(p1x, p1y);
    }
}