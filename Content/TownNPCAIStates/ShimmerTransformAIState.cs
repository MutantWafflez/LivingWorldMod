using System;
using LivingWorldMod.Common.GlobalNPCs;
using LivingWorldMod.Common.ModTypes;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;

namespace LivingWorldMod.Content.TownNPCAIStates;

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
            Point point = npc.Top.ToTileCoordinates();
            int maxYOffset = 30;
            Vector2? safePosition = null;
            bool isHomeless = npc.homeless && (npc.homeTileX == -1 || npc.homeTileY == -1);

            for (int i = 1; i < maxYOffset; i += 2) {
                Vector2? safePos = ShimmerHelper.FindSpotWithoutShimmer(npc, point.X, point.Y, i, isHomeless);

                if (!safePos.HasValue) {
                    continue;
                }

                safePosition = safePos.Value;
                break;
            }

            if (!safePosition.HasValue && npc.homeTileX != -1 && npc.homeTileY != -1) {
                for (int i = 1; i < maxYOffset; i += 2) {
                    Vector2? safePos = ShimmerHelper.FindSpotWithoutShimmer(npc, npc.homeTileX, npc.homeTileY, i, isHomeless);

                    if (!safePos.HasValue) {
                        continue;
                    }

                    safePosition = safePos.Value;
                    break;
                }
            }


            if (!safePosition.HasValue) {
                int startingYOffset = isHomeless ? 30 : 0;
                maxYOffset = 60;

                for (int i = startingYOffset; i < maxYOffset; i += 2) {
                    Vector2? safePos = ShimmerHelper.FindSpotWithoutShimmer(npc, point.X, point.Y, i, true);

                    if (!safePos.HasValue) {
                        continue;
                    }

                    safePosition = safePos.Value;
                    break;
                }
            }

            if (!safePosition.HasValue && npc.homeTileX != -1 && npc.homeTileY != -1) {
                maxYOffset = 60;
                for (int l = 30; l < maxYOffset; l += 2) {
                    Vector2? vector4 = ShimmerHelper.FindSpotWithoutShimmer(npc, npc.homeTileX, npc.homeTileY, l, true);

                    if (!vector4.HasValue) {
                        continue;
                    }

                    safePosition = vector4.Value;
                    break;
                }
            }

            if (safePosition.HasValue) {
                Vector2 oldPos = npc.position;
                npc.position = safePosition.Value;

                Vector2 positionChangeVector = npc.position - oldPos;

                if (positionChangeVector.Length() >= 560) {
                    npc.ai[2] = 30f;
                    ParticleOrchestrator.BroadcastParticleSpawn(ParticleOrchestraType.ShimmerTownNPCSend, new ParticleOrchestraSettings {
                        PositionInWorld = oldPos + npc.Size / 2f,
                        MovementVector = positionChangeVector
                    });
                }

                npc.netUpdate = true;
            }
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
            Dust.NewDustPerfect(Main.rand.NextVector2FromRectangle(hitbox) + Main.rand.NextVector2Circular(8f, 0f) + new Vector2(0f, 4f), 309,
                new Vector2(0f, -2f).RotatedBy(randomDustDirection * ((float)Math.PI * 2f) * 0.11f), 0, default(Color), 1.7f - Math.Abs(randomDustDirection) * 1.3f);
        }

        if (npc.ai[1] > 60f && Main.rand.NextBool(15)) {
            for (int i = 0; i < 3; i++) {
                Vector2 vector = Main.rand.NextVector2FromRectangle(npc.Hitbox);
                ParticleOrchestrator.RequestParticleSpawn(true, ParticleOrchestraType.ShimmerBlock, new ParticleOrchestraSettings {
                    PositionInWorld = vector,
                    MovementVector = npc.DirectionTo(vector).RotatedBy((float)Math.PI * 9f / 20f * (Main.rand.Next(2) * 2 - 1)) * Main.rand.NextFloat()
                });
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

        ParticleOrchestrator.BroadcastParticleSpawn(ParticleOrchestraType.ShimmerTownNPC, new ParticleOrchestraSettings {
            PositionInWorld = npc.Center
        });
    }
}