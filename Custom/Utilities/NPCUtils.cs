using LivingWorldMod.Common.Systems;
using LivingWorldMod.Content.NPCs.Villagers;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Structs;
using Microsoft.Xna.Framework;
using System;

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

            ReputationThresholdData thresholds = ReputationSystem.Instance.villageThresholdData[villager.VillagerType];

            float reputationValue = ReputationSystem.Instance.GetNumericVillageReputation(villager.VillagerType);
            float centerPoint = (thresholds.likeThreshold - thresholds.dislikeThreshold) / 2f;

            return MathHelper.Clamp(1 - reputationValue / (ReputationSystem.VillageReputationConstraint - centerPoint) / 2f, 0.67f, 1.67f);
        }

        /// <summary>
        /// Returns the count of all defined villager types as defined by the <seealso cref="VillagerType"/> enum.
        /// </summary>
        /// <returns></returns>
        public static int GetTotalVillagerTypeCount() => Enum.GetValues<VillagerType>().Length;
    }
}