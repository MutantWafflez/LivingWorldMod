using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LivingWorldMod.Common.Systems.BaseSystems;
using LivingWorldMod.Custom.Enums;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace LivingWorldMod.Common.Systems {
    /// <summary>
    /// ModSystem that handles Dialogue for the various types of the villagers, including dialogue weights
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class DialogueSystem : BaseModSystem<DialogueSystem> {
        private Dictionary<VillagerType, WeightedRandom<ModTranslation>> _weightedVillagerDialogue;

        public override void PostSetupContent() {
            _weightedVillagerDialogue = new Dictionary<VillagerType, WeightedRandom<ModTranslation>>();

            Dictionary<string, ModTranslation> translations = typeof(LocalizationLoader).GetField("translations", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null) as Dictionary<string, ModTranslation>;
            translations = translations!.Where(pair => pair.Value.Key.StartsWith($"Mods.{nameof(LivingWorldMod)}.VillagerDialogue")).ToDictionary(pair => pair.Key, pair => pair.Value);
            string[] villagerTypes = Enum.GetNames<VillagerType>();
            string[] reputationType = Enum.GetNames<VillagerRelationship>();
        }
    }
}