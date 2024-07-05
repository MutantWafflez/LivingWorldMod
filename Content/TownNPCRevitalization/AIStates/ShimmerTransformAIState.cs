using System;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Drawing;

namespace LivingWorldMod.Content.TownNPCRevitalization.AIStates;

public class ShimmerTransformAIState : TownNPCAIState {
    public override int ReservedStateInteger => 25;

    public override void DoState(TownGlobalNPC globalNPC, NPC npc) {
        // Adapted vanilla code
        npc.dontTakeDamage = true;
        if (npc.ai[1] == 0f) {
            npc.velocity.X = 0f;
        }

        npc.shimmerWet = false;
        npc.wet = false;
        npc.lavaWet = false;
        npc.honeyWet = false;

        if (npc.ai[1] == 0f && Main.netMode == NetmodeID.MultiplayerClient) {
            return;
        }

        if (npc.ai[1] == 0f && npc.ai[2] < 1f) {
            npc.AI_007_TownEntities_Shimmer_TeleportToLandingSpot();
        }

        if (npc.ai[2] > 0f) {
            npc.ai[2] -= 1f;
            if (npc.ai[2] <= 0f) {
                npc.ai[1] = 1f;
            }

            return;
        }

        npc.ai[1] += 1f;
        if (npc.ai[1] >= 30f) {
            if (!Collision.WetCollision(npc.position, npc.width, npc.height)) {
                npc.shimmerTransparency = MathHelper.Clamp(npc.shimmerTransparency - 1f / 60f, 0f, 1f);
            }
            else {
                npc.ai[1] = 30f;
            }

            npc.velocity = new Vector2(0f, -4f * npc.shimmerTransparency);
        }

        Rectangle hitbox = npc.Hitbox;
        hitbox.Y += 20;
        hitbox.Height -= 20;
        float randomDustDirection = Main.rand.NextFloatDirection();
        Lighting.AddLight(npc.Center, Main.hslToRgb((float)Main.timeForVisualEffects / 360f % 1f, 0.6f, 0.65f).ToVector3() * Utils.Remap(npc.ai[1], 30f, 90f, 0f, 0.7f));
        if (Main.rand.NextFloat() > Utils.Remap(npc.ai[1], 30f, 60f, 1f, 0.5f)) {
            Dust.NewDustPerfect(
                Main.rand.NextVector2FromRectangle(hitbox) + Main.rand.NextVector2Circular(8f, 0f) + new Vector2(0f, 4f),
                309,
                new Vector2(0f, -2f).RotatedBy(randomDustDirection * ((float)Math.PI * 2f) * 0.11f),
                0,
                default(Color),
                1.7f - Math.Abs(randomDustDirection) * 1.3f
            );
        }

        if (npc.ai[1] > 60f && Main.rand.NextBool(15)) {
            for (int i = 0; i < 3; i++) {
                Vector2 vector = Main.rand.NextVector2FromRectangle(npc.Hitbox);
                ParticleOrchestrator.RequestParticleSpawn(
                    true,
                    ParticleOrchestraType.ShimmerBlock,
                    new ParticleOrchestraSettings {
                        PositionInWorld = vector, MovementVector = npc.DirectionTo(vector).RotatedBy((float)Math.PI * 9f / 20f * (Main.rand.Next(2) * 2 - 1)) * Main.rand.NextFloat()
                    }
                );
            }
        }

        npc.TargetClosest();
        if (!(npc.ai[1] >= 75f) || !(npc.shimmerTransparency <= 0f) || Main.netMode == NetmodeID.MultiplayerClient) {
            return;
        }

        for (int i = 0; i < NPC.maxAI; i++) {
            npc.ai[i] = npc.localAI[i] = 0f;
        }

        npc.velocity = new Vector2(0f, -4f);
        npc.netUpdate = true;

        npc.townNpcVariationIndex = npc.townNpcVariationIndex != 1 ? 1 : 0;

        NetMessage.SendData(MessageID.UniqueTownNPCInfoSyncRequest, -1, -1, null, npc.whoAmI);
        npc.Teleport(npc.position, 12);

        ParticleOrchestrator.BroadcastParticleSpawn(ParticleOrchestraType.ShimmerTownNPC, new ParticleOrchestraSettings { PositionInWorld = npc.Center });
    }
}