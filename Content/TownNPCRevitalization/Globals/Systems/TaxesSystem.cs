using System.Collections.Generic;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems.UI;
using LivingWorldMod.Content.TownNPCRevitalization.UI.TaxSheet;
using LivingWorldMod.Globals.BaseTypes.Systems;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;

/// <summary>
///     System that handles the Tax Collector's overhauled Taxes in tandem with the new "Tax Sheet" UI. See <see cref="TaxSheetUISystem" /> and <see cref="TaxSheetUIState" />.
/// </summary>
public class TaxesSystem : BaseModSystem<TaxesSystem>  {
    private readonly record struct NPCTaxValues(long PropertyTax, float SalesTax);

    public const long MaxPropertyTax = Item.gold * 2 + Item.silver * 50;

    public const float MaxSalesTax = 0.4f;

    // In vanilla, each NPC gets taxed 50 copper per in-game hour - since this new system is day based, we multiply that value by 24 for an in-game day
    private static readonly NPCTaxValues DefaultTaxValues = new (Item.copper * 50 * 24, 0f);

    private readonly Dictionary<int, NPCTaxValues> _taxValues = [];
}