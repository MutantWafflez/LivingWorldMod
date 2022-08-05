using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using LivingWorldMod.Common.Systems.BaseSystems;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Structs;
using Terraria.GameContent.Events;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using NPCUtils = LivingWorldMod.Custom.Utilities.NPCUtils;

namespace LivingWorldMod.Common.Systems {
    /// <summary>
    /// ModSystem that handles Dialogue for the various types of the villagers, including dialogue weights & event requirements.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class DialogueSystem : BaseModSystem<DialogueSystem> {
        private Dictionary<VillagerType, List<DialogueData>> _villagerDialogue;
        private Dictionary<string, Func<bool>> _eventCheckers;

        public override void Load() {
            _eventCheckers = new Dictionary<string, Func<bool>>() {
                { "Rain", () => Main.raining },
                { "PumpkinMoon", () => Main.pumpkinMoon },
                { "FrostMoon", () => Main.snowMoon },
                { "Eclipse", () => Main.eclipse },
                { "BloodMoon", () => Main.bloodMoon },
                { "WindyDay", () => Main.IsItAHappyWindyDay },
                { "Thunderstorm", () => Main.IsItStorming },
                { "Party", () => BirthdayParty.PartyIsUp },
                { "Lanterns", () => LanternNight.LanternsUp }
            };
        }


        public override void PostSetupContent() {
            _villagerDialogue = new Dictionary<VillagerType, List<DialogueData>>();

            Dictionary<string, ModTranslation> translationDict = typeof(LocalizationLoader).GetField("translations", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null) as Dictionary<string, ModTranslation>;
            Dictionary<string, ModTranslation> allDialogue = translationDict!.Where(pair => pair.Value.Key.StartsWith($"Mods.{nameof(LivingWorldMod)}.VillagerDialogue")).ToDictionary(pair => pair.Key, pair => pair.Value);

            for (VillagerType type = 0; (int)type < NPCUtils.GetTotalVillagerTypeCount(); type++) {
                List<DialogueData> finalDialogueData = new List<DialogueData>();
                Dictionary<string, ModTranslation> typeDialogue = allDialogue.Where(pair => pair.Key.StartsWith($"Mods.{nameof(LivingWorldMod)}.VillagerDialogue.{type}")).ToDictionary(pair => pair.Key, pair => pair.Value);

                foreach ((string key, ModTranslation value) in typeDialogue.Where(pair => !(pair.Key.EndsWith(".Weight") || pair.Key.EndsWith(".Events"))).ToDictionary(pair => pair.Key, pair => pair.Value)) {
                    //If the key ends with ".Text", that means it possible has Weight and Events value(s)
                    if (key.EndsWith(".Text")) {
                        string keyPrefix = key.Remove(key.LastIndexOf(".Text"));
                        string keyWithWeight = keyPrefix + ".Weight";
                        string keyWithEvents = keyPrefix + ".Events";

                        double weight = 1;
                        if (typeDialogue.ContainsKey(keyWithWeight) && double.TryParse(typeDialogue[keyWithWeight].GetDefault(), out double newWeight)) {
                            weight = newWeight;
                        }

                        string[] requiredEvents = null;
                        if (typeDialogue.ContainsKey(keyWithEvents)) {
                            requiredEvents = typeDialogue[keyWithEvents].GetDefault().Split('|');
                        }

                        finalDialogueData.Add(new DialogueData(value, weight, requiredEvents));
                    }
                    else {
                        finalDialogueData.Add(new DialogueData(value, 1, null));
                    }
                }

                _villagerDialogue[type] = finalDialogueData;
            }
        }

        /// <summary>
        /// Selects a piece of localized dialogue for the specified villager type, based on the current relationship status,
        /// any possible events, and weights of each dialogue.
        /// </summary>
        /// <param name="villagerType"> The villager type to get the dialogue line of. </param>
        /// <param name="relationshipStatus"> The current relationship status with the villager type in question. </param>
        /// <param name="dialogueType"> The type of dialogue wanted. </param>
        /// <returns></returns>
        public string GetDialogue(VillagerType villagerType, VillagerRelationship relationshipStatus, DialogueType dialogueType) {
            List<DialogueData> allDialogue = _villagerDialogue[villagerType];
            WeightedRandom<string> dialogueOptions = new WeightedRandom<string>();

            switch (dialogueType) {
                case DialogueType.Normal:
                    foreach (DialogueData data in allDialogue.Where(dialogue => !dialogue.dialogue.Key.StartsWith($"Mods.{nameof(LivingWorldMod)}.VillagerDialogue.{villagerType}.Shop"))) {
                        if (!data.dialogue.Key.Contains($".{relationshipStatus}.")) {
                            continue;
                        }

                        if (data.requiredEvents is { } events && !events.All(requiredEvent => _eventCheckers.ContainsKey(requiredEvent) && _eventCheckers[requiredEvent]())) {
                            continue;
                        }

                        dialogueOptions.Add(data.dialogue.GetTranslation(Language.ActiveCulture), data.weight);
                    }

                    break;
                case DialogueType.ShopInitial:
                case DialogueType.ShopBuy:
                    foreach (DialogueData data in allDialogue.Where(dialogue => dialogue.dialogue.Key.StartsWith($"Mods.{nameof(LivingWorldMod)}.VillagerDialogue.{villagerType}.Shop.{dialogueType.ToString().Replace("Shop", "")}"))) {
                        if (!data.dialogue.Key.Contains($".{relationshipStatus}.")) {
                            continue;
                        }

                        if (data.requiredEvents is { } events && !events.All(requiredEvent => _eventCheckers.ContainsKey(requiredEvent) && _eventCheckers[requiredEvent]())) {
                            continue;
                        }

                        dialogueOptions.Add(data.dialogue.GetTranslation(Language.ActiveCulture), data.weight);
                    }

                    break;
            }

            return dialogueOptions.elements.Any() ? dialogueOptions : "Dialogue error! No dialogue found, report to devs!";
        }
    }
}