using System.Collections.Generic;
using System.IO;
using System.Linq;
using LivingWorldMod.Custom.Classes.WorldGen;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace LivingWorldMod.Custom.Utilities
{
    /// <summary>
    /// Class that handles ANY extension methods for ANY class.
    /// </summary>
    public static class ExtensionUtils
    {
        public static void ConditionalStringAdd<T>(this WeightedRandom<T> weightedRandom, T curType, bool boolean, double weight = 1)
        {
            if (boolean)
            {
                weightedRandom.Add(curType, weight);
            }
        }

        public static Tile ToTile(this TileNode tn) => Framing.GetTileSafely(tn.position);

        public static Point16 Add(this Point16 point, int p1x, int p1y) => new Point16(point.X + p1x, point.Y + p1y);

        public static IEnumerable<KeyValuePair<TKey, TValue>> Entries<TKey, TValue>(this Dictionary<TKey, TValue> dict)
        {
            return dict.Select(pair => pair);
        }

        public static T TryGet<T>(this TagCompound tag, string key, T defaultValue = default)
        {
            try
            {
                return tag.Get<T>(key);
            }
            catch (IOException e)
            {
                LivingWorldMod.Instance.Logger.Error("IO Exception During TryGet:", e);
                return defaultValue;
            }
        }
    }
}