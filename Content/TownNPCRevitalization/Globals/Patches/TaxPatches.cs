using System;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using LivingWorldMod.DataStructures.Classes;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;

/// <summary>
///     Patches for specifically the Tax Overhaul.
/// </summary>
public class TaxPatches : LoadablePatch {
    public override void LoadPatches() {
        IL_Player.CollectTaxes += CollectTaxesPatch;
    }

    private void CollectTaxesPatch(ILContext il) {
        currentContext = il;

        // Edit that overrides the tax collection system to use our brand new tax overhaul values 
        ILCursor c = new (il);

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Action<Player>>(
            player => {
                int taxCap = TaxesSystem.TaxCap;
                if (Main.tenthAnniversaryWorld) {
                    taxCap *= 2;
                }

                if (!NPC.taxCollector || player.taxMoney >= taxCap) {
                    return;
                }

                foreach (NPC npc in Main.ActiveNPCs) {
                    if (!TaxesSystem.IsNPCValidForTaxes(npc, out _)) {
                        continue;
                    }

                    player.taxMoney += TaxesSystem.Instance.GetTaxValuesOrDefault(npc.type).PropertyTax;
                    if (player.taxMoney <= taxCap) {
                        continue;
                    }

                    player.taxMoney = taxCap;
                    return;
                }
            }
        );
        c.Emit(OpCodes.Ret);
    }
}