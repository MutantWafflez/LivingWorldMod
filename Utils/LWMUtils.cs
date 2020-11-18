using LivingWorldMod.NPCs.Villagers;
using Terraria;
using Terraria.DataStructures;
using Terraria.Utilities;

namespace LivingWorldMod.Utilities
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
      
        /// <summary>
        /// Returns whether or not the given player is within a given range of an NPC type.
        /// </summary>
        /// <param name="npcType">The type of NPC checked for.</param>
        /// <param name="range">The distance required to be considered "within range", measured in pixels.</param>
        /// <returns></returns>
        public static bool IsWithinRangeOfNPC(this Player player, int npcType, float range)
        {
            for (int i = 0; i < 200; i++)
            {
                if (!Main.npc[i].active)
                {
                    continue;
                }
                if (Main.npc[i].type == npcType && player.Distance(Main.npc[i].Center) <= range)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns whether or not a given NPC is a type of Villager.
        /// </summary>
        /// <param name="npc">The npc to check.</param>
        public static bool IsTypeOfVillager(this NPC npc)
        {
            if (npc.modNPC?.GetType().BaseType == typeof(Villager))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Finds the closest NPC to any other NPC.
        /// Optionally can find the nearest NPC of a given type if needed.
        /// </summary>
        public static NPC FindNearestNPC(this NPC npc, int npcType = -1) {
            float distance = float.MaxValue;
            NPC selectedNPC = null;
            for (int i = 0; i < Main.maxNPCs; i++) {
                if (Main.npc[i] != npc && Main.npc[i].Distance(npc.Center) < distance && (Main.npc[i].type == npcType || npcType == -1)) {
                    selectedNPC = Main.npc[i];
                    distance = Main.npc[i].Distance(npc.Center);
                }
            }
            return selectedNPC;
        }

        //----------Extension Methods----------
        public static Tile ToTile(this TileNode tn) => Framing.GetTileSafely(tn.position);
        public static Point16 Add(this Point16 point, int p1x, int p1y) => new Point16(point.X + p1x, point.Y + p1y);
    }
}