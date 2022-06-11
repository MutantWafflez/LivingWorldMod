using LivingWorldMod.Common.Systems;
using LivingWorldMod.Content.NPCs.Villagers;
using LivingWorldMod.Custom.Enums;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Custom.Utilities {
    /// <summary>
    /// Class that handles all utility functions for NPCs.
    /// </summary>
    public static class NPCUtils {
        /// <summary>
        /// Returns the price multiplier that will affect shop prices depending on the status of a
        /// villager's relationship with the players.
        /// </summary>
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