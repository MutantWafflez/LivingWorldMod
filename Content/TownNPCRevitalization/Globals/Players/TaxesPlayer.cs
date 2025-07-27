using LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Players;

/// <summary>
///     Mod Player that handles player-specific tax interactions, namely for the sake of sales tax.
/// </summary>
public class TaxesPlayer : ModPlayer {
    private const string DirectDepositKey = "DirectDeposit";

    /// <summary>
    ///     Whether or not, at each tax collection interval (see <see cref="TaxesPatches" />), the money will go immediately into this player's
    ///     bank, at a cut.
    /// </summary>
    public bool directDeposit;

    public override void PostBuyItem(NPC vendor, Item[] shopInventory, Item item) {
        if (!TaxesSystem.IsNPCValidForTaxes(vendor, out _) || item.shopSpecialCurrency != CustomCurrencyID.None) {
            return;
        }

        Player.GetItemExpectedPrice(item, out _, out long buyPrice);

        Player.taxMoney = Utils.Clamp(Player.taxMoney + (int)((item.shopCustomPrice ?? buyPrice) * TaxesSystem.Instance.GetTaxValuesOrDefault(vendor.type).SalesTax), 0, TaxesSystem.TaxCap);
    }

    public override void SaveData(TagCompound tag) {
        tag[DirectDepositKey] = directDeposit;
    }

    public override void LoadData(TagCompound tag) {
        tag.TryGet(DirectDepositKey, out bool hasDirectDeposit);

        directDeposit = hasDirectDeposit;
    }
}