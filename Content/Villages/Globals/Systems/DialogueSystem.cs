using System;
using System.Collections.Generic;
using System.Linq;
using Hjson;
using LivingWorldMod.Content.Villages.DataStructures.Enums;
using LivingWorldMod.Content.Villages.DataStructures.Structs;
using LivingWorldMod.Globals.BaseTypes.Systems;

using Terraria.GameContent.Events;
using Terraria.Localization;
using Terraria.Utilities;

namespace LivingWorldMod.Content.Villages.Globals.Systems;

/// <summary>
///     ModSystem that handles Dialogue for the various types of the villagers, including dialogue weights & event
///     requirements.
/// </summary>
[Autoload(Side = ModSide.Client)]
public class DialogueSystem : BaseModSystem<DialogueSystem> {
    private Dictionary<VillagerType, List<DialogueData>> _villagerDialogue;
    private Dictionary<string, Func<bool>> _eventCheckers;

    public override void Load() {
        _eventCheckers = new Dictionary<string, Func<bool>> {
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
        _villagerDialogue = [];

        Dictionary<string, LocalizedText> translationDict = LanguageManager.Instance._localizedTexts;
        Dictionary<string, LocalizedText> allDialogue = translationDict
            .Where(pair => pair.Value.Key.StartsWith($"Mods.{nameof(LivingWorldMod)}.VillagerDialogue"))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        JsonValue jsonReputationData = LWMUtils.GetJSONFromFile("Assets/JSONData/DialogueWeights.json");
        for (VillagerType type = 0; (int)type < LWMUtils.GetTotalVillagerTypeCount(); type++) {
            List<DialogueData> finalDialogueData = [];

            // TODO: Use something like Lang.CreateDialogueFilter instead of enumerating manually
            string keyStart = $"Mods.{nameof(LivingWorldMod)}.VillagerDialogue.{type}";
            Dictionary<string, LocalizedText> typeDialogue = allDialogue.Where(pair => pair.Key.StartsWith(keyStart)).ToDictionary(pair => pair.Key, pair => pair.Value);
            JsonObject villageSpecificData = jsonReputationData[type.ToString()].Qo();

            foreach ((string key, LocalizedText value) in typeDialogue) {
                double weight = 1;
                int priority = 0;
                string[] requiredEvents = null;

                string[] splitKey = key[(keyStart.Length + 1)..].Split('.');
                if (villageSpecificData.TryGetValue(splitKey[0], out JsonValue dialogueSubdivisionData) && dialogueSubdivisionData.Qo().TryGetValue(splitKey[1], out JsonValue specificDialogueValue)) {
                    JsonObject specificDialogueObject = specificDialogueValue.Qo();

                    if (specificDialogueObject.TryGetValue("Weight", out JsonValue weightValue)) {
                        weight = weightValue.Qd();
                    }

                    if (specificDialogueObject.TryGetValue("Priority", out JsonValue priorityValue)) {
                        priority = priorityValue.Qi();
                    }

                    if (specificDialogueObject.TryGetValue("Events", out JsonValue eventsValue)) {
                        requiredEvents = eventsValue.Qs().Split('|');
                    }
                }

                finalDialogueData.Add(new DialogueData(value, weight, priority, requiredEvents));
            }

            _villagerDialogue[type] = finalDialogueData;
        }
    }

    /// <summary>
    ///     Selects a piece of localized dialogue for the specified villager type, based on the current relationship status,
    ///     any possible events, and weights of each dialogue.
    /// </summary>
    /// <param name="villagerType"> The villager type to get the dialogue line of. </param>
    /// <param name="relationshipStatus"> The current relationship status with the villager type in question. </param>
    /// <param name="dialogueType"> The type of dialogue wanted. </param>
    /// <returns></returns>
    public string GetDialogue(VillagerType villagerType, VillagerRelationship relationshipStatus, DialogueType dialogueType) {
        List<DialogueData> allDialogue = _villagerDialogue[villagerType];
        WeightedRandom<string> dialogueOptions = new();
        int priorityThreshold = allDialogue.Min(data => data.priority);

        foreach (DialogueData data in dialogueType == DialogueType.Normal
            ? allDialogue.Where(dialogue => !dialogue.dialogue.Key.StartsWith($"Mods.{nameof(LivingWorldMod)}.VillagerDialogue.{villagerType}.Shop"))
            : allDialogue.Where(dialogue => dialogue.dialogue.Key.StartsWith($"Mods.{nameof(LivingWorldMod)}.VillagerDialogue.{villagerType}.Shop.{dialogueType.ToString().Replace("Shop", "")}"))) {
            //So many checks! Conditionals!
            if ((!data.dialogue.Key.StartsWith($"Mods.{nameof(LivingWorldMod)}.VillagerDialogue.{villagerType}.Event")
                    && !data.dialogue.Key.Contains($".{relationshipStatus}."))
                || !TestEvents(data.requiredEvents)
                || data.priority < priorityThreshold) {
                continue;
            }

            if (data.priority > priorityThreshold) {
                priorityThreshold = data.priority;
                dialogueOptions.Clear();
            }

            dialogueOptions.Add(data.dialogue.Value, data.weight);
        }

        return dialogueOptions.elements.Count != 0 ? dialogueOptions : "Dialogue error! No dialogue found, report to devs!";
    }

    /// <summary>
    ///     Takes the passed in events and checks to see if any pass. Returns true if all passed, or it is null, false otherwise.
    /// </summary>
    /// <param name="events"> The array of events to check. </param>
    private bool TestEvents(string[] events) {
        if (events is null) {
            return true;
        }

        foreach (string eventToCheck in events) {
            //Negation functionality
            if (eventToCheck.StartsWith('!')) {
                string eventKey = eventToCheck.TrimStart('!');

                if (_eventCheckers.TryGetValue(eventKey, out Func<bool> value) && value()) {
                    return false;
                }
            }
            else {
                if (_eventCheckers.TryGetValue(eventToCheck, out Func<bool> value) && !value()) {
                    return false;
                }
            }
        }

        return true;
    }
}