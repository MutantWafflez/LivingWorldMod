using System;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Players;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using LivingWorldMod.DataStructures.Classes;
using LivingWorldMod.Utilities;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.DataStructures;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;

/// <summary>
///     Patches for specifically the Taxes Overhaul.
/// </summary>
public class TaxesPatches : LoadablePatch {
    private static void CollectTaxesFromNPCs(Player player, int taxCap) {
        ShoppingSettings taxCollectorShopSettings = Main.ShopHelper.GetShoppingSettings(Main.LocalPlayer, LWMUtils.GetFirstNPC(npc => npc.type == NPCID.TaxCollector));

        foreach (NPC npc in Main.ActiveNPCs) {
            if (!TaxesSystem.IsNPCValidForTaxes(npc, out _)) {
                continue;
            }

            player.taxMoney += (int)(TaxesSystem.Instance.GetTaxValuesOrDefault(npc.type).PropertyTax / taxCollectorShopSettings.PriceAdjustment);
            if (player.taxMoney < taxCap) {
                continue;
            }

            player.taxMoney = taxCap;
            break;
        }
    }

    public override void LoadPatches() {
        IL_Player.CollectTaxes += CollectTaxesPatch;
        IL_Main.GUIChatDrawInner += TaxCollectorCollectButtonPatch;
    }

    private void TaxCollectorCollectButtonPatch(ILContext il) {
        currentContext = il;

        // Edit that patches out happiness affecting the Tax Collectors "Collect" button
        // Happiness is being moved to directly affect how much goes into the bank from each NPC
        ILCursor c = new(il);

        int taxMoneyLocalIndex = int.MaxValue;
        while (
            c.TryGotoNext(
                MoveType.After,
                i => i.MatchLdfld<ShoppingSettings>(nameof(ShoppingSettings.PriceAdjustment)),
                i => i.MatchDiv(),
                i => i.MatchConvI4(),
                i => i.MatchStloc(out taxMoneyLocalIndex)
            )
        ) {
            c.Index--;

            c.EmitPop();
            c.EmitLdloc(taxMoneyLocalIndex);
        }
    }

    private void CollectTaxesPatch(ILContext il) {
        currentContext = il;

        // Edit that overrides the tax collection system to use our brand new tax overhaul values 
        ILCursor c = new (il);

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Action<Player>>(player => {
                int taxCap = TaxesSystem.TaxCap;
                if (!NPC.taxCollector) {
                    return;
                }

                if (player.taxMoney < taxCap) {
                    CollectTaxesFromNPCs(player, taxCap);
                }

                if (!player.GetModPlayer<TaxesPlayer>().directDeposit) {
                    return;
                }

                int[] directDepositAmounts = Utils.CoinsSplit((int)(player.taxMoney * (1f - TaxesSystem.DirectDepositCut)));

                int currentCoinType = ItemID.CopperCoin;
                NPC taxCollector = LWMUtils.GetFirstNPC(npc => npc.type == NPCID.TaxCollector);
                foreach (int coinAmount in directDepositAmounts) {
                    if (coinAmount > 0) {
                        player.QuickSpawnItem(new EntitySource_Gift(taxCollector), currentCoinType, coinAmount);
                    }

                    currentCoinType++;
                }

                player.taxMoney = 0;
            }
        );
        c.Emit(OpCodes.Ret);
    }
}