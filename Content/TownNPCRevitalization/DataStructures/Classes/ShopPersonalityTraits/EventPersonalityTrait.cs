using System;
using System.Collections;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.ShopPersonalityTraits;

public class EventPersonalityTrait(params EventPersonalityTrait.EventPreference[] preferences) : IShopPersonalityTrait, IEnumerable<EventPersonalityTrait.EventPreference> {
    public record struct EventPreference(AffectionLevel AffectionLevel, Func<bool> EventActiveFunction);

    private readonly List<EventPreference> _preferences = [..preferences];

    public void ModifyShopPrice(HelperInfo info, ShopHelper shopHelperInstance) {
        foreach ((AffectionLevel affectionLevel, Func<bool> eventActiveFunction) in _preferences) {
            
        }
    }

    public IEnumerator<EventPreference> GetEnumerator() => throw new NotImplementedException();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}