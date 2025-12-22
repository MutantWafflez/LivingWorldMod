using System;
using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.Villages.DataStructures.Enums;
using LivingWorldMod.Content.Villages.DataStructures.Records;
using LivingWorldMod.Content.Villages.Globals.BaseTypes.NPCs;
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

    /// <summary>
    ///     Selects a piece of localized dialogue for the specified villager type, based on the current relationship status,
    ///     any possible events, and weights of each dialogue.
    /// </summary>
    /// <param name="villagerType"> The villager type to get the dialogue line of. </param>
    /// <param name="relationshipStatus"> The current relationship status with the villager type in question. </param>
    /// <param name="dialogueType"> The type of dialogue wanted. </param>
    /// <returns></returns>
    public LocalizedText GetDialogue(VillagerType villagerType, VillagerRelationship relationshipStatus, DialogueType dialogueType) {
        List<DialogueData> allDialogue = Villager.VillagerProfiles[villagerType].Dialogues;
        WeightedRandom<LocalizedText> dialogueOptions = new();
        int priorityThreshold = allDialogue.Min(data => data.Priority);

        foreach (DialogueData data in dialogueType == DialogueType.Normal
            ? allDialogue.Where(dialogue => !dialogue.DialogueKey.StartsWith($"Mods.{nameof(LivingWorldMod)}.VillagerDialogue.{villagerType}.Shop"))
            : allDialogue.Where(dialogue => dialogue.DialogueKey.StartsWith($"Mods.{nameof(LivingWorldMod)}.VillagerDialogue.{villagerType}.Shop.{dialogueType.ToString().Replace("Shop", "")}"))) {
            //So many checks! Conditionals!
            if ((!data.DialogueKey.StartsWith($"Mods.{nameof(LivingWorldMod)}.VillagerDialogue.{villagerType}.Event")
                    && !data.DialogueKey.Contains($".{relationshipStatus}."))
                || !TestEvents(data.RequiredEvents)
                || data.Priority < priorityThreshold) {
                continue;
            }

            if (data.Priority > priorityThreshold) {
                priorityThreshold = data.Priority;
                dialogueOptions.Clear();
            }

            dialogueOptions.Add(Language.GetText(data.DialogueKey), data.Weight);
        }

        return dialogueOptions.elements.Count > 0 ? dialogueOptions : LocalizedText.Empty;
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