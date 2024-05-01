using System;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;

/// <summary>
/// Town NPC module that handles the special collision of Town NPCs introduced by this mod.
/// </summary>
public class TownNPCCollisionModule(NPC npc) : TownNPCModule(npc) {
    public bool fallThroughPlatforms;
    public bool ignoreStairs;

    /// <summary>
    /// This method is called in <seealso cref="TownNPCCollisionPatches"/>.
    /// </summary>
    public override void Update() {
        npc.Collision_WalkDownSlopes();
        npc.Collision_WaterCollision(npc.Collision_LavaCollision());
        if (!npc.wet) {
            npc.lavaWet = npc.honeyWet = npc.shimmerWet = false;
        }

        if (npc.wetCount > 0) {
            npc.wetCount--;
        }

        npc.oldVelocity = npc.velocity;
        npc.collideX = npc.collideY = false;
        npc.GetTileCollisionParameters(out Vector2 cPosition, out int cWidth, out int cHeight);
        Vector2 oldVelocity = npc.velocity;
        npc.velocity = Collision.TileCollision(cPosition, npc.velocity, cWidth, cHeight, fallThroughPlatforms);
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
        ApplyNPCVelocity(oldVelocity, liquidVelocityModifier);

        AttemptSlopeCollision();
        Collision.StepConveyorBelt(npc, 1f);
    }

    private void ApplyNPCVelocity(Vector2 oldVelocity, float velocityModifier) {
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

    private void AttemptSlopeCollision() {
        npc.stairFall = ignoreStairs;
        npc.GetTileCollisionParameters(out Vector2 cPosition, out int cWidth, out int cHeight);
        Vector2 endPosOffset = npc.position - cPosition;
        Vector4 newPosAndVelocity = Collision.SlopeCollision(cPosition, npc.velocity, cWidth, cHeight, npc.gravity, fallThroughPlatforms);
        Vector2 newPos = newPosAndVelocity.XY(), newVelocity = newPosAndVelocity.ZW();

        if (npc.velocity.Y <= 0 && Collision.stair && ignoreStairs) {
            newPos = npc.position;
            newVelocity.Y = 0f;
        }
        else if (Collision.stair && Math.Abs(newPos.Y - npc.position.Y) > 8f) {
            npc.gfxOffY -= newPos.Y - npc.position.Y;
            npc.stepSpeed = 2f;
        }

        npc.position = newPos;
        npc.velocity = newVelocity;
        npc.position += endPosOffset;
    }
}