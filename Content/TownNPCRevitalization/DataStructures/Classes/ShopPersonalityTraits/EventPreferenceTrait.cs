using System.Collections.Generic;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Utilities;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.GameContent.Personalities;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.ShopPersonalityTraits;

/// <summary>
///     Similar to biome/NPC preferences for Town NPCs happiness, except for "events" which are denoted active by some
///     arbitrary function returning a bool.
/// </summary>
public class EventPreferenceTrait(int moodOffset, string eventName) : IShopPersonalityTrait {
    private delegate bool ActiveEvent(HelperInfo helperInfo);

    private static readonly Dictionary<string, ActiveEvent> ActiveEventFunctions = new() {
        { "Party", _ => BirthdayParty.PartyIsUp && BirthdayParty.GenuineParty },
        { "BloodMoon", _ => Main.bloodMoon },
        { "Rain", _ => Main.raining && !Main.IsItStorming },
        { "Eclipse", _ => Main.eclipse },
        { "Storm", _ => Main.IsItStorming },
        { "GoblinArmy", _ => Main.invasionType == InvasionID.GoblinArmy },
        { "Pirates", _ => Main.invasionType == InvasionID.PirateInvasion }
    };

    public void ModifyShopPrice(HelperInfo info, ShopHelper shopHelperInstance) {
        if (!ActiveEventFunctions[eventName](info)) {
            return;
        }

        info.npc.GetGlobalNPC<TownGlobalNPC>()
            .MoodModule
            .AddModifier($"TownNPCMoodDescription.Event_{eventName}".Localized(), $"TownNPCMoodFlavorText.{info.npc.TypeName}.Event_{eventName}".Localized(), moodOffset, 0);
    }
}