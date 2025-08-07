using System;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.BaseTypes.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;

/// <summary>
///     Town NPC module that handles the special collision of Town NPCs introduced by this mod.
/// </summary>
public class TownNPCCollisionModule : TownNPCModule {
    public bool fallThroughPlatforms;
    public bool fallThroughStairs;
    public bool walkThroughStairs;
    public bool ignoreLiquidVelocityModifications;

    /// <summary>
    ///     This method is called in <seealso cref="RevitalizationNPCPatches" />.
    /// </summary>
    public void UpdateCollision() {
        NPC.Collision_WalkDownSlopes();
        bool lavaCollision = NPC.Collision_LavaCollision();

        // The water collision function has a potential velocity side effect, so to ensure that ignore flag is honored, we must wrap the function call
        if (ignoreLiquidVelocityModifications) {
            Vector2 dryVelocity = NPC.velocity;
            NPC.Collision_WaterCollision(lavaCollision);
            NPC.velocity = dryVelocity;
        }
        else {
            NPC.Collision_WaterCollision(lavaCollision);
        }

        if (!NPC.wet) {
            NPC.lavaWet = NPC.honeyWet = NPC.shimmerWet = false;
        }

        if (NPC.wetCount > 0) {
            NPC.wetCount--;
        }

        NPC.oldVelocity = NPC.velocity;
        NPC.collideX = NPC.collideY = false;
        NPC.GetTileCollisionParameters(out Vector2 cPosition, out int cWidth, out int cHeight);
        Vector2 oldVelocity = NPC.velocity;
        NPC.velocity = Collision.TileCollision(cPosition, NPC.velocity, cWidth, cHeight, fallThroughPlatforms);
        float liquidVelocityModifier = 1f;
        if (NPC.wet && !ignoreLiquidVelocityModifications) {
            if (NPC.shimmerWet) {
                liquidVelocityModifier = NPC.shimmerMovementSpeed;
            }
            else if (NPC.honeyWet) {
                liquidVelocityModifier = NPC.honeyMovementSpeed;
            }
            else if (NPC.lavaWet) {
                liquidVelocityModifier = NPC.lavaMovementSpeed;
            }
            else {
                liquidVelocityModifier = NPC.waterMovementSpeed;
            }
        }

        ApplyNPCVelocity(oldVelocity, liquidVelocityModifier);

        AttemptSlopeCollision();
        Collision.StepConveyorBelt(NPC, 1f);
    }

    private void ApplyNPCVelocity(Vector2 oldVelocity, float velocityModifier) {
        if (Collision.up) {
            NPC.velocity.Y = 0.01f;
        }

        Vector2 modifiedVelocity = NPC.velocity * velocityModifier;
        if (NPC.velocity.X != oldVelocity.X) {
            modifiedVelocity.X = NPC.velocity.X;
            NPC.collideX = true;
        }

        if (NPC.velocity.Y != oldVelocity.Y) {
            modifiedVelocity.Y = NPC.velocity.Y;
            NPC.collideY = true;
        }

        NPC.oldPosition = NPC.position;
        NPC.oldDirection = NPC.direction;
        NPC.position += modifiedVelocity;
    }

    private void AttemptSlopeCollision() {
        NPC.stairFall = fallThroughStairs;
        NPC.GetTileCollisionParameters(out Vector2 cPosition, out int cWidth, out int cHeight);
        Vector2 endPosOffset = NPC.position - cPosition;
        Vector4 newPosAndVelocity = Collision.SlopeCollision(cPosition, NPC.velocity, cWidth, cHeight, NPC.gravity, fallThroughStairs);
        Vector2 newPos = newPosAndVelocity.XY(), newVelocity = newPosAndVelocity.ZW();

        if (NPC.velocity.Y <= 0 && Collision.stair && walkThroughStairs) {
            newPos = NPC.position;
            newVelocity.Y = 0f;
        }
        else if (Collision.stair && Math.Abs(newPos.Y - NPC.position.Y) > 8f) {
            NPC.gfxOffY -= newPos.Y - NPC.position.Y;
            NPC.stepSpeed = 2f;
        }

        NPC.position = newPos;
        NPC.velocity = newVelocity;
        NPC.position += endPosOffset;
    }
}