using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using LivingWorldMod.Utilities;
using Terraria.GameContent;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.PersonalityTraits;

/// <summary>
/// Trait that handles various mood deficits due to property/sales tax. Higher of one or both of these values will increase the deficit.
/// </summary>
public class TaxesTrait : IPersonalityTrait {

    private const int ModeratePropertyTaxThreshold = Item.silver * 50;
    private const int HighPropertyTaxThreshold = Item.gold;
    private const int ExtremePropertyTaxThreshold = (int)(Item.gold * 1.75f);

    private const float MidSalesTaxThreshold = 0.1f;
    private const float HighSalesTaxThreshold = 0.2f;
    private const float ExtremeSalesTaxThreshold = 0.3f;
    
    public void ApplyTrait(PersonalityHelperInfo info, ShopHelper shopHelperInstance) {
        (int propertyTax, float salesTax) = TaxesSystem.Instance.GetTaxValuesOrDefault(info.NPC.type);

        TownNPCMoodModule moodModule = info.NPC.GetGlobalNPC<TownNPCMoodModule>();
        string npcTypeName = LWMUtils.GetNPCTypeNameOrIDName(info.NPC.type);

        // TODO: Finish trait
    }
}