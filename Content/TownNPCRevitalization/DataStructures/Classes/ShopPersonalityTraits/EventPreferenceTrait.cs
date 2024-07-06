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
public class EventPreferenceTrait(AffectionLevel affectionLevel, string eventName) : IShopPersonalityTrait {
    private delegate bool ActiveEvent(HelperInfo helperInfo);

    private static readonly Dictionary<string, ActiveEvent> ActiveEventFunctions = new() {
        { "Party", helperInfo => BirthdayParty.PartyIsUp && BirthdayParty.GenuineParty && BirthdayParty.CelebratingNPCs.Contains(helperInfo.npc.type) }, { "BloodMoon", _ => Main.bloodMoon }
    };

    public void ModifyShopPrice(HelperInfo info, ShopHelper shopHelperInstance) {
        if (ActiveEventFunctions[eventName](info)) {
            info.npc.GetGlobalNPC<TownGlobalNPC>()
                .MoodModule.ConvertReportTextToStaticModifier($"TownNPCMoodFlavorText.{info.npc.TypeName}".PrependModKey(), $"{affectionLevel}Event_{eventName}");
        }
    }
}