using System;
using LivingWorldMod.Common.GlobalNPCs;
using LivingWorldMod.Common.ModTypes;
using LivingWorldMod.Custom.Classes;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace LivingWorldMod.Content.TownNPCAIStates;

public sealed class GoHomeAIState : TownNPCAIState {
    public override int ReservedStateInteger => 27;

    public override void DoState(TownAIGlobalNPC globalNPC, NPC npc) {
        if (globalNPC.HousingModule.RestPos is not { } restPos) {
            TownAIGlobalNPC.RefreshToState<DefaultAIState>(npc);
            return;
        }

        TownNPCPathfinderModule pathfinderModule = globalNPC.PathfinderModule;
        if (npc.ai[1] == 0f) {
            if (--npc.ai[2] >= 0) {
                return;
            }

            bool hasPath = pathfinderModule.HasPath(restPos);
            if (!pathfinderModule.IsPathfinding && hasPath) {
                pathfinderModule.RequestPathfind(restPos, reachedDestination => {
                    if (reachedDestination && TownNPCHousingModule.ShouldSleep && globalNPC.HousingModule.RoomBoundingBox is not null) {
                        npc.ai[1] = 1f;
                        npc.ai[2] = 0f;
                    }
                    else {
                        npc.ai[2] = (int)(UnitUtils.RealLifeMinute / 2f);
                    }
                });
            }
            else if (pathfinderModule.IsPathfinding || !hasPath) {
                npc.ai[2] = (int)(UnitUtils.RealLifeMinute / 2f);
            }
        }
        else if (npc.ai[1] == 1f) {
            if ((npc.BottomLeft + new Vector2(0, -2f)).ToTileCoordinates() == restPos) {
                Tile restTile = Main.tile[restPos];
                if (TileID.Sets.CanBeSleptIn[restTile.TileType]) {
                    npc.friendlyRegen += 20;

                    PlayerSleepingHelper.GetSleepingTargetInfo(restPos.X, restPos.Y, out int targetDirection, out _, out _);
                    npc.direction = targetDirection;
                    npc.rotation = MathHelper.PiOver2 * -targetDirection;
                    Main.sleepingManager.AddNPC(npc.whoAmI, restPos);
                    globalNPC.SpriteModule.RequestBlink();
                }
                else if (TileID.Sets.CanBeSatOnForNPCs[restTile.TileType]) {
                    npc.friendlyRegen += 15;

                    npc.SitDown(restPos, out int direction, out _);
                    npc.direction = direction;
                    Main.sittingManager.AddNPC(npc.whoAmI, restPos);
                    globalNPC.SpriteModule.RequestBlink();
                }
                else {
                    npc.ai[1] = 0f;
                }
            }
            else {
                npc.ai[1] = 0f;
            }
        }
    }

    public override void FrameNPC(TownAIGlobalNPC globalNPC, NPC npc, int frameHeight) {
        // TODO: Likely redundant check, investigate if it can be removed
        if (globalNPC.HousingModule.RestPos is not { } restPos) {
            return;
        }

        Tile restTile = Main.tile[restPos];
        // Set NPC to sitting frame
        if (npc.ai[1] == 1f && TileID.Sets.CanBeSatOnForNPCs[restTile.TileType]) {
            npc.frame.Y = frameHeight * (Main.npcFrameCount[npc.type] - NPCID.Sets.AttackFrameCount[npc.type] - 3);
        }
    }

    public override void PostDrawNPC(TownAIGlobalNPC globalNPC, NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        Main.instance.LoadItem(ItemID.SleepingIcon);

        if (npc.ai[1] != 1f) {
            return;
        }

        Texture2D sleepingIconTexture = TextureAssets.Item[ItemID.SleepingIcon].Value;
        spriteBatch.Draw(
            sleepingIconTexture,
            npc.Top - screenPos + new Vector2(sleepingIconTexture.Width / -2f, -16 + MathF.Sin(Main.GlobalTimeWrappedHourly)),
            drawColor * 0.67f
        );
    }
}