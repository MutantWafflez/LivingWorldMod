using System;
using System.Reflection;
using LivingWorldMod.DataStructures.Classes;
using LivingWorldMod.DataStructures.Interfaces;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI.Elements;
using BestiaryInfoCategory = Terraria.GameContent.UI.Elements.UIBestiaryEntryInfoPage.BestiaryInfoCategory;

namespace LivingWorldMod.Globals.Patches;

/// <summary>
///     Patches that deal broadly with the bestiary, whether it be UI or other internal mechanisms.
/// </summary>
public class BestiaryPatches : LoadablePatch {
    private static BestiaryInfoCategory _returnedCategory;

    private static void BestiaryCategoryModificationPatch(ILContext il) {
        // Edit that allows usage of IBestiaryCategorizedElement and go around vanilla's hard-coding with the BestiaryInfoCategory enum
        currentContext = il;

        ILCursor c = new (il);
        c.Emit(OpCodes.Ldarg_1);
        c.EmitDelegate<Func<IBestiaryInfoElement, bool>>(
            element => {
                if (element is not IBestiaryCategorizedElement categorizedElement) {
                    return false;
                }

                _returnedCategory = categorizedElement.InfoCategory;
                return true;
            }
        );
        c.Emit(OpCodes.Brfalse_S, c.DefineLabel());

        Instruction falseBranchInstr = c.Prev;
        c.Emit(OpCodes.Ldsfld, typeof(BestiaryPatches).GetField(nameof(_returnedCategory), BindingFlags.Static | BindingFlags.NonPublic)!);
        c.Emit(OpCodes.Ret);

        falseBranchInstr.Operand = c.MarkLabel();
    }

    public override void LoadPatches() {
        IL_UIBestiaryEntryInfoPage.GetBestiaryInfoCategory += BestiaryCategoryModificationPatch;
    }
}