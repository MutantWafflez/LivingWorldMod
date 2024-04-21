using System;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.DataStructures.Classes;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;

/// <summary>
/// Patches that deal with special collision with Town NPCs.
/// </summary>
public class TownNPCCollisionPatches : LoadablePatch {
    private delegate bool ApplyTileCollision(NPC npc, bool fall, Vector2 cPosition, int cWidth, int cHeight);

    public override void LoadPatches() {
        IL_NPC.ApplyTileCollision += ApplyTownNPCCollision;
    }

    private void ApplyTownNPCCollision(ILContext il) {
        // Small edit that lets us do our own collision for Town NPCs instead of vanilla's.
        currentContext = il;

        ILCursor c = new(il);

        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldarg_1);
        c.Emit(OpCodes.Ldarg_2);
        c.Emit(OpCodes.Ldarg_3);
        c.Emit(OpCodes.Ldarg_S, (byte)4);
        c.EmitDelegate<ApplyTileCollision>((npc, fall, cPosition, cWidth, cHeight) => {
            if (!npc.TryGetGlobalNPC<TownGlobalNPC>(out _)) {
                return false;
            }

            npc.velocity = TownNPCCollisionMovement(npc, fall, cPosition, cWidth, cHeight);
            return true;
        });
        c.Emit(OpCodes.Brfalse_S, c.DefineLabel());
        c.Emit(OpCodes.Ret);

        ILLabel failureLabel = c.MarkLabel();
        if (c.TryGotoPrev(MoveType.Before, i => i.Match(OpCodes.Brfalse_S))) {
            c.Next!.Operand = failureLabel;
        }
        else {
            throw new Exception("Previously emitted brfalse_s instruction not found.");
        }
    }

    private static Vector2 TownNPCCollisionMovement(NPC npc, bool fall, Vector2 cPosition, int cWidth, int cHeight) => Collision.TileCollision(cPosition, npc.velocity, cWidth, cHeight, fall, fall);
}