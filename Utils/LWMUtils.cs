using LivingWorldMod.NPCs.Villagers;
using LivingWorldMod.NPCs.Villagers.Quest;
using LivingWorldMod.Tiles.Interactables;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Utilities;
using static Terraria.ModLoader.ModContent;

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
        /// Returns whether or not a given NPC is a type of Quest Villager.
        /// </summary>
        /// <param name="npc">The npc to check.</param>
        public static bool IsTypeOfQuestVillager(this NPC npc)
        {
            if (npc.modNPC?.GetType().BaseType == typeof(QuestVillager))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Finds the closest NPC to any other NPC.
        /// Optionally can find the nearest NPC of a given type if needed.
        /// </summary>
        public static NPC FindNearestNPC(this NPC npc, int npcType = -1)
        {
            float distance = float.MaxValue;
            NPC selectedNPC = null;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i] != npc && Main.npc[i].Distance(npc.Center) < distance && (Main.npc[i].type == npcType || npcType == -1))
                {
                    selectedNPC = Main.npc[i];
                    distance = Main.npc[i].Distance(npc.Center);
                }
            }
            return selectedNPC;
        }

        /// <summary>
        /// Finds the top left tile coordinate of a multi tile.
        /// </summary>
        /// <param name="i">Horizontal tile coordinates</param>
        /// <param name="j">Vertical tile coordinates</param>
        /// <param name="tileType">Multi tile's tile type.</param>
        /// <returns></returns>
        public static Vector2 FindMultiTileTopLeft(int i, int j, int tileType)
        {
            Vector2 topLeftPos = new Vector2(i, j);
            //Finding top left corner
            for (int k = 0; k < 4; k++)
            {
                for (int l = 0; l < 5; l++)
                {
                    if (Framing.GetTileSafely(i - k, j - l).type == TileType<HarpyShrineTile>())
                        topLeftPos = new Vector2(i - k, j - l);
                    else continue;
                }
            }

            return topLeftPos;
        }

        //----------Extension Methods----------
        public static Tile ToTile(this TileNode tn) => Framing.GetTileSafely(tn.position);

        public static Point16 Add(this Point16 point, int p1x, int p1y) => new Point16(point.X + p1x, point.Y + p1y);
    }
}