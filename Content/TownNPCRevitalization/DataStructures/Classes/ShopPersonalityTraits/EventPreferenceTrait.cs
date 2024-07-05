using System.Collections;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.GameContent.Personalities;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.ShopPersonalityTraits;

/// <summary>
///     Similar to biome/NPC preferences for Town NPCs happiness, except for "events" which are denoted active by some
///     arbitrary function returning a bool.
/// </summary>
public class EventPreferenceTrait(params EventPreferenceTrait.EventPreference[] preferences) : IShopPersonalityTrait, IEnumerable<EventPreferenceTrait.EventPreference> {
    public delegate bool ActiveEvent(HelperInfo helperInfo);

    public record struct EventPreference(AffectionLevel AffectionLevel, string EventName);

    private static readonly Dictionary<string, ActiveEvent> ActiveEventFunctions = new() {
        { "Party", helperInfo => BirthdayParty.PartyIsUp && BirthdayParty.GenuineParty && BirthdayParty.CelebratingNPCs.Contains(helperInfo.npc.type) },
        { "BloodMoon", _ => Main.bloodMoon }
    };

    private readonly List<EventPreference> _preferences = [..preferences];

    public void ModifyShopPrice(HelperInfo info, ShopHelper shopHelperInstance) {
        foreach ((AffectionLevel affectionLevel, string eventName) in _preferences) {
            if (ActiveEventFunctions[eventName](info)) {
                shopHelperInstance.AddHappinessReportText($"{affectionLevel}Event_{eventName}");
            }
        }
    }

    public IEnumerator<EventPreference> GetEnumerator() => _preferences.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}