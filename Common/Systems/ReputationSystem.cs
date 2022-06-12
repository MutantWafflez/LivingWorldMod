using LivingWorldMod.Custom.Enums;
using Microsoft.Xna.Framework;
using System;
using LivingWorldMod.Custom.Utilities;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Common.Systems {
    /// <summary>
    /// System that handles reputations for all of the villages in the mod.
    /// </summary>
    public class ReputationSystem : ModSystem {
        private static int[] villageReputation;
        public const int VillageReputationConstraint = 100;

        /// <summary> Gets & returns the reputation value of the given villager type specified.
        /// </summary> <returns></returns>
        public static int GetVillageReputation(VillagerType villagerType) => villageReputation[(int)villagerType];

        /// <summary> Gets & returns the reputation value of the given villager type specified.
        /// </summary> <returns></returns>
        public static int GetVillageReputation(int villagerType) => villageReputation[villagerType];

        /// <summary>
        /// Changes the value of the specified village type's reputation BY the specified amount.
        /// </summary>
        public static void ChangeVillageReputation(VillagerType villagerType, int changeAmount) {
            villageReputation[(int)villagerType] += changeAmount;
        }

        /// <summary>
        /// Changes the value of the specified village type's reputation BY the specified amount.
        /// </summary>
        public static void ChangeVillageReputation(int villagerType, int changeAmount) {
            villageReputation[villagerType] += changeAmount;
        }

        /// <summary>
        /// Sets the value of the specified village type's reputation TO the specified amount.
        /// </summary>
        public static void SetVillageReputation(VillagerType villagerType, int setValue) {
            villageReputation[(int)villagerType] = setValue;
        }

        /// <summary>
        /// Sets the value of the specified village type's reputation TO the specified amount.
        /// </summary>
        public static void SetVillageReputation(int villagerType, int setValue) {
            villageReputation[villagerType] = setValue;
        }

        public override void Load() {
            villageReputation = new int[NPCUtils.GetTotalVillagerTypeCount()];
        }

        public override void Unload() {
            villageReputation = null;
        }

        public override void PostUpdateEverything() {
            //Clamp Village Reputation
            for (int i = 0; i < NPCUtils.GetTotalVillagerTypeCount(); i++) {
                villageReputation[i] = (int)MathHelper.Clamp(villageReputation[i], -VillageReputationConstraint, VillageReputationConstraint);
            }
        }

        public override void SaveWorldData(TagCompound tag) {
            tag["VillageReputation"] = villageReputation;
        }

        public override void LoadWorldData(TagCompound tag) {
            int[] savedVillageRep = tag.GetIntArray("VillageReputation");
            //Make sure to keep the array size up to date in case a player is loading an old world
            if (savedVillageRep.Length != villageReputation.Length) {
                Array.Resize(ref savedVillageRep, villageReputation.Length);
                villageReputation = savedVillageRep;
            }
            else {
                villageReputation = savedVillageRep;
            }
        }
    }
}