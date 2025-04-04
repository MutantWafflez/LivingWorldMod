using System.Collections.Generic;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs.TownNPCModules;
using LivingWorldMod.Utilities;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using DesertBiome = Terraria.GameContent.Personalities.DesertBiome;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.PersonalityTraits;

/// <summary>
///     Similar to biome/NPC preferences for Town NPCs happiness, except for "events" which are denoted active by some
///     arbitrary function returning a bool.
/// </summary>
public class EventPreferencesTrait(EventPreferencesTrait.EventPreference[] preferences) : IPersonalityTrait {
    public delegate bool EventPredicate(PersonalityHelperInfo helperInfo);

    public record struct EventPreference(string EventName, int MoodOffset);

    public static readonly Dictionary<string, EventPredicate> EventPredicates = new() {
        { "Party", _ => BirthdayParty.PartyIsUp && BirthdayParty.GenuineParty },
        { "BloodMoon", _ => Main.bloodMoon },
        { "Rain", _ => Main.raining && !Main.IsItStorming },
        { "Eclipse", _ => Main.eclipse },
        { "Storm", _ => Main.IsItStorming },
        { "GoblinArmy", _ => Main.invasionType == InvasionID.GoblinArmy },
        { "Pirates", _ => Main.invasionType == InvasionID.PirateInvasion },
        { "Sandstorm", info => Sandstorm.Happening && ModContent.GetInstance<DesertBiome>().IsInBiome(info.Player) }
    };

    public void ApplyTrait(PersonalityHelperInfo info, ShopHelper shopHelperInstance) {
        foreach ((string eventName, int moodOffset) in preferences) {
            if (!EventPredicates[eventName](info)) {
                continue;
            }

            info.NPC.GetGlobalNPC<TownNPCMoodModule>()
                .AddModifier(
                    $"TownNPCMoodDescription.Event_{eventName}".Localized(),
                    $"TownNPCMoodFlavorText.{LWMUtils.GetNPCTypeNameOrIDName(info.NPC.type)}.Event_{eventName}".Localized(),
                    moodOffset
                );
        }
    }
}