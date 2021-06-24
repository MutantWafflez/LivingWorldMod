using LivingWorldMod.Content.NPCs.Villagers;
using Terraria.ModLoader;

namespace LivingWorldMod.Custom.Utilities {

    /// <summary>
    /// Class that handles all utility functions for NPCs.
    /// </summary>
    public static class NPCUtilities {

        /// <summary>
        /// Returns whether or not an NPC of the given type is a Villager NPC type.
        /// </summary>
        /// <param name="type"> NPC type to check the Villager status of. </param>
        /// <returns> </returns>
        public static bool IsTypeOfVillager(int type) {
            return NPCLoader.GetNPC(type)?.GetType().IsSubclassOf(typeof(Villager)) ?? false;
        }
    }
}