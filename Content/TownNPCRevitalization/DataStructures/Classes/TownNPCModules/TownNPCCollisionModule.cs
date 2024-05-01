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
        bool lava = npc.Collision_LavaCollision();
        npc.Collision_WaterCollision(lava);
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
        npc.velocity = Collision.TileCollision(cPosition, npc.velocity, cWidth, cHeight, fallThroughPlatforms, fallThroughPlatforms);
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
        ApplyTownNPCVelocity(oldVelocity, liquidVelocityModifier);

        TownNPCSlopeCollision();
        Collision.StepConveyorBelt(npc, 1f);
    }

    private void ApplyTownNPCVelocity(Vector2 oldVelocity, float velocityModifier) {
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

    private void TownNPCSlopeCollision() {
        // if (fall) {
        //     npc.stairFall = true;
        // }
        npc.stairFall = ignoreStairs;

        npc.GetTileCollisionParameters(out Vector2 cPosition, out int cWidth, out int cHeight);
        Vector2 vector = npc.position - cPosition;
        Vector4 vector2 = Collision.SlopeCollision(cPosition, npc.velocity, cWidth, cHeight, npc.gravity, ignoreStairs);
        // if (Collision.stairFall) {
        //     npc.stairFall = true;
        // }
        // else if (!fall) {
        //     npc.stairFall = false;
        // }

        if (Collision.stair && Math.Abs(vector2.Y - npc.position.Y) > 8f) {
            npc.gfxOffY -= vector2.Y - npc.position.Y;
            npc.stepSpeed = 2f;
        }

        npc.position.X = vector2.X;
        npc.position.Y = vector2.Y;
        npc.velocity.X = vector2.Z;
        npc.velocity.Y = vector2.W;
        npc.position += vector;
    }
}