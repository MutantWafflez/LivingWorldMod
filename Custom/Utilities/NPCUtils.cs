using LivingWorldMod.Content.NPCs.Villagers;
using LivingWorldMod.Content.NPCs.Villagers.Quest;
using Terraria;

namespace LivingWorldMod.Custom.Utilities
{
    /// <summary>
    /// Utility class that handles anything related to or messed with NPCs.
    /// </summary>
    public static class NPCUtils
    {
        /// <summary>
        /// Returns whether or not a given NPC is a type of Villager.
        /// </summary>
        /// <param name="npc"> The npc to check. </param>
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
        /// <param name="npc"> The npc to check. </param>
        public static bool IsTypeOfQuestVillager(this NPC npc)
        {
            if (npc.modNPC?.GetType().BaseType == typeof(QuestVillager))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Finds the closest NPC to any other NPC. Optionally can find the nearest NPC of a given
        /// type if needed.
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
    }
}