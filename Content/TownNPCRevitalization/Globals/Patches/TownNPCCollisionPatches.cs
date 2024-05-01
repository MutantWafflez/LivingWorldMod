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
    public override void LoadPatches() {
        IL_NPC.UpdateCollision += NPCUpdateCollisionUpdate;
    }

    private void NPCUpdateCollisionUpdate(ILContext il) {
        // Hijacks collision for Town NPCs. Gives us full control.
        currentContext = il;

        ILCursor c = new(il);
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<NPC, bool>>(npc => {
            if (!npc.TryGetGlobalNPC<TownGlobalNPC>(out _)) {
                return false;
            }

            DoTownNPCCollision(npc);
            return true;
        });
        c.Emit(OpCodes.Brfalse_S, c.DefineLabel());
        c.Emit(OpCodes.Ret);

        ILLabel normalCollisionLabel = c.MarkLabel();
        c.GotoPrev(i => i.MatchBrfalse(out _));
        c.Next!.Operand = normalCollisionLabel;
    }

    private static void DoTownNPCCollision(NPC npc) {
        npc.Collision_WalkDownSlopes();
        bool lava = npc.Collision_LavaCollision();
        npc.Collision_WaterCollision(lava);
        if (!npc.wet) {
            npc.lavaWet = false;
            npc.honeyWet = false;
            npc.shimmerWet = false;
        }

        if (npc.wetCount > 0) {
            npc.wetCount--;
        }

        bool fall = npc.Collision_DecideFallThroughPlatforms();
        npc.oldVelocity = npc.velocity;
        npc.collideX = false;
        npc.collideY = false;
        npc.GetTileCollisionParameters(out Vector2 cPosition, out int cWidth, out int cHeight);
        Vector2 oldVelocity = npc.velocity;
        npc.velocity = Collision.TileCollision(cPosition, npc.velocity, cWidth, cHeight, fall, fall);
        float liquidVelocityModifier = 1f;
        if (npc.wet) {
            if (npc.shimmerWet) {
                liquidVelocityModifier = npc.shimmerMovementSpeed;
            }
            else if (npc.honeyWet) {
                liquidVelocityModifier = npc.honeyMovementSpeed;
            }
            else if (npc.lavaWet) {
                liquidVelocityModifier = npc.lavaMovementSpeed;
            }
            else {
                liquidVelocityModifier = npc.waterMovementSpeed;
            }
        }
        ApplyTownNPCVelocity(npc, oldVelocity, liquidVelocityModifier);

        npc.Collision_MoveSlopesAndStairFall(fall);
        Collision.StepConveyorBelt(npc, 1f);
    }

    private static void ApplyTownNPCVelocity(NPC npc, Vector2 oldVelocity, float velocityModifier) {
        if (Collision.up) {
            npc.velocity.Y = 0.01f;
        }

        Vector2 modifiedVelocity = npc.velocity * velocityModifier;
        if (npc.velocity.X != oldVelocity.X) {
            modifiedVelocity.X = npc.velocity.X;
            npc.collideX = true;
        }

        if (npc.velocity.Y != oldVelocity.Y) {
            modifiedVelocity.Y = npc.velocity.Y;
            npc.collideY = true;
        }

        npc.oldPosition = npc.position;
        npc.oldDirection = npc.direction;
        npc.position += modifiedVelocity;
    }
}