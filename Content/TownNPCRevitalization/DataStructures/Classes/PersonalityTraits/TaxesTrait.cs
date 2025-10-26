using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;
using LivingWorldMod.DataStructures.Records;
using LivingWorldMod.DataStructures.Structs;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria.GameContent;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.PersonalityTraits;

/// <summary>
///     Trait that handles various mood deficits due to property/sales tax. Higher of one or both of these values will increase the deficit.
/// </summary>
public class TaxesTrait : IPersonalityTrait {
    private const int MidPropertyTaxThreshold = Item.silver * 50;
    private const int HighPropertyTaxThreshold = Item.gold;
    private const int ExtremePropertyTaxThreshold = (int)(Item.gold * 1.75f);

    private const float MidSalesTaxThreshold = 0.1f;
    private const float HighSalesTaxThreshold = 0.2f;
    private const float ExtremeSalesTaxThreshold = 0.3f;

    private static readonly Gradient<float> PropertyTaxGradient = new (
        MathHelper.Lerp,
        (MidPropertyTaxThreshold, -6f),
        (HighPropertyTaxThreshold, -12f),
        (ExtremePropertyTaxThreshold, -20f),
        (TaxesSystem.MaxPropertyTax, -35f)
    );

    private static readonly Gradient<float> SalesTaxGradient = new (
        MathHelper.Lerp,
        (0f, 0f),
        (MidSalesTaxThreshold, -8f),
        (HighSalesTaxThreshold, -18f),
        (ExtremeSalesTaxThreshold, -26f),
        (TaxesSystem.MaxSalesTax, -38f)
    );

    private static void CheckPropertyTax(int propertyTax, TownNPCMoodModule moodModule, string npcTypeName) {
        if (propertyTax < MidPropertyTaxThreshold) {
            return;
        }

        string propertyTaxKey = propertyTax switch {
            < HighPropertyTaxThreshold => "MidPropertyTax",
            < ExtremePropertyTaxThreshold => "HighPropertyTax",
            _ => "ExtremePropertyTax"
        };

        string flavorTextKey = $"TownNPCMoodFlavorText.{npcTypeName}.{propertyTaxKey}";
        string defaultFlavorTextKey = $"TownNPCMoodFlavorText.Default.{propertyTaxKey}";
        moodModule.AddModifier(
            new DynamicLocalizedText($"TownNPCMoodDescription.{propertyTaxKey}".Localized()),
            new DynamicLocalizedText(flavorTextKey.Localized(), FallbackText: defaultFlavorTextKey.Localized()),
            (int)PropertyTaxGradient.GetValue(propertyTax)
        );
    }

    private static void CheckSalesTax(float salesTax, TownNPCMoodModule moodModule, string npcTypeName) {
        if (salesTax <= 0f) {
            return;
        }

        string salesTaxKey = salesTax switch {
            < MidSalesTaxThreshold => "SalesTax",
            < HighSalesTaxThreshold => "MidSalesTax",
            < ExtremeSalesTaxThreshold => "HighSalesTax",
            _ => "ExtremeSalesTax"
        };

        string flavorTextKey = $"TownNPCMoodFlavorText.{npcTypeName}.{salesTaxKey}";
        string defaultFlavorTextKey = $"TownNPCMoodFlavorText.Default.{salesTaxKey}";
        moodModule.AddModifier(
            new DynamicLocalizedText($"TownNPCMoodDescription.{salesTaxKey}".Localized()),
            new DynamicLocalizedText(flavorTextKey.Localized(), FallbackText: defaultFlavorTextKey.Localized()),
            (int)SalesTaxGradient.GetValue(salesTax)
        );
    }

    public void ApplyTrait(PersonalityHelperInfo info, ShopHelper shopHelperInstance) {
        (int propertyTax, float salesTax) = TaxesSystem.Instance.GetTaxValuesOrDefault(info.NPC.type);

        TownNPCMoodModule moodModule = info.NPC.GetGlobalNPC<TownNPCMoodModule>();
        string npcTypeName = LWMUtils.GetNPCTypeNameOrIDName(info.NPC.type);

        CheckPropertyTax(propertyTax, moodModule, npcTypeName);
        CheckSalesTax(salesTax, moodModule, npcTypeName);
    }
}