using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;

/// <summary>
///     Extremely similar interface to <see cref="IShopPersonalityTrait" />, created for more express control over the Town NPC Revitalization mood system in comparison to the vanilla version.
/// </summary>
public interface IPersonalityTrait {
    public void ApplyTrait(PersonalityHelperInfo info, ShopHelper shopHelperInstance);
}