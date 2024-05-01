using System;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.DataStructures.Classes;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;

/// <summary>
/// Patches that deal with special collision with Town NPCs.
/// </summary>
public class TownNPCCollisionPatches : LoadablePatch {
    public override void LoadPatches() {
        IL_NPC.UpdateCollision += NPCUpdateCollisionUpdate;
    }

    private void NPCUpdateCollisionUpdate(ILContext il) {
        // Hijacks collision for Town NPCs. Gives us full control.
        currentContext = il;

        ILCursor c = new(il);
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<NPC, bool>>(npc => {
            if (!npc.TryGetGlobalNPC(out TownGlobalNPC globalNPC)) {
                return false;
            }

            globalNPC.CollisionModule.Update();
            return true;
        });
        c.Emit(OpCodes.Brfalse_S, c.DefineLabel());
        c.Emit(OpCodes.Ret);

        ILLabel normalCollisionLabel = c.MarkLabel();
        c.GotoPrev(i => i.MatchBrfalse(out _));
        c.Next!.Operand = normalCollisionLabel;
    }
}