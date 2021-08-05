using LivingWorldMod.Common.Systems;
using LivingWorldMod.Content.NPCs.Villagers;
using LivingWorldMod.Custom.Enums;
using Microsoft.Xna.Framework;
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

        /// <summary>
        /// Returns the price multiplier that will affect shop prices depending on the status of a villager's relationship with the players.
        /// </summary>
        /// <param name="currentRelationship"></param>
        /// <returns></returns>
        public static float GetPriceMultiplierFromRep(Villager villager) {
            if (villager.RelationshipStatus == VillagerRelationship.Neutral) {
                return 1f;
            }

            float reputationValue = ReputationSystem.GetVillageReputation(villager.VillagerType);
            float centerPoint = (villager.LikeThreshold - villager.DislikeThreshold) / 2f;

            return MathHelper.Clamp(1 - reputationValue / (ReputationSystem.VillageReputationConstraint - centerPoint) / 2f, 0.67f, 1.67f);
        }
    }
}