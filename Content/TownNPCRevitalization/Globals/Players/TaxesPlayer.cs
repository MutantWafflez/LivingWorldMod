using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Players;

/// <summary>
///     Mod Player that handles player-specific tax interactions, namely for the sake of sales tax.
/// </summary>
public class TaxesPlayer : ModPlayer {
    public override void PostBuyItem(NPC vendor, Item[] shopInventory, Item item) {
        if (!TaxesSystem.IsNPCValidForTaxes(vendor, out _) || item.shopSpecialCurrency != CustomCurrencyID.None) {
            return;
        }

        Player.GetItemExpectedPrice(item, out _, out long buyPrice);

        Player.taxMoney = Utils.Clamp(Player.taxMoney + (int)((item.shopCustomPrice ?? buyPrice) * TaxesSystem.Instance.GetTaxValuesOrDefault(vendor.type).SalesTax), 0, TaxesSystem.TaxCap);
    }
}