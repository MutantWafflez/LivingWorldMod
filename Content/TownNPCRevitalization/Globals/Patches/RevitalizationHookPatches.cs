using System.Reflection;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Hooks;
using LivingWorldMod.DataStructures.Classes;
using LivingWorldMod.Utilities;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;

/// <summary>
///     Patches for the Town NPC Revitalization that inject custom hooks into various vanilla methods.
/// </summary>
public class RevitalizationHookPatches : LoadablePatch {
    public override void LoadPatches() {
        IL_Main.GUIChatDrawInner += SetButtonTextsHookImplementationPatch;
    }

    private void SetButtonTextsHookImplementationPatch(ILContext il) {
        // Super simple edit: Add our call to our custom GlobalNPC hook, so we can freely modify NPC chat buttons
        currentContext = il;

        ILCursor c = new(il);

        int focusTextOne = 0;
        int focusTextTwo = 0;
        c.ErrorOnFailedGotoNext(
            MoveType.After,
            i => i.MatchLdloca(out focusTextOne),
            i => i.MatchLdloca(out focusTextTwo),
            i => i.MatchCall(typeof(NPCLoader), nameof(NPCLoader.SetChatButtons))
        );

        c.Emit(OpCodes.Ldloca, focusTextOne);
        c.Emit(OpCodes.Ldloca, focusTextTwo);
        c.Emit(OpCodes.Call, typeof(ISetButtonTexts).GetMethod(nameof(ISetButtonTexts.Invoke), BindingFlags.Public | BindingFlags.Static)!);
    }
}