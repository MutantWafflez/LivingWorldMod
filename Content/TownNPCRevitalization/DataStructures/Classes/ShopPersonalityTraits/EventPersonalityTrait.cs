using System;
using System.Collections;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.ShopPersonalityTraits;

/// <summary>
/// Similar to biome/NPC preferences for Town NPCs happiness, except for "events" which are denoted active by some arbitrary function returning a bool. 
/// </summary>
public class EventPersonalityTrait(params EventPersonalityTrait.EventPreference[] preferences) : IShopPersonalityTrait, IEnumerable<EventPersonalityTrait.EventPreference> {
    public record struct EventPreference(AffectionLevel AffectionLevel, Func<bool> EventActiveFunction, string EventName);

    private readonly List<EventPreference> _preferences = [..preferences];

    public void ModifyShopPrice(HelperInfo info, ShopHelper shopHelperInstance) {
        foreach ((AffectionLevel affectionLevel, Func<bool> eventActiveFunction, string eventName) in _preferences) {
            if (eventActiveFunction()) {
                shopHelperInstance.AddHappinessReportText($"{affectionLevel}Event_{eventName}");
            }
        }
    }

    public IEnumerator<EventPreference> GetEnumerator() => _preferences.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}