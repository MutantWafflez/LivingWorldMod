using System;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace LivingWorldMod.Content.TownNPCRevitalization.AIStates;

public sealed class BeAtHomeAIState : TownNPCAIState {
    public override void DoState(TownGlobalNPC globalNPC, NPC npc) {
        if (!TownNPCHousingModule.ShouldGoHome) {
            TownGlobalNPC.RefreshToState<DefaultAIState>(npc);
            return;
        }

        TownNPCPathfinderModule pathfinderModule = globalNPC.PathfinderModule;
        Point restPos = globalNPC.HousingModule.RestPos;
        if (!TownGlobalNPC.IsValidStandingPosition(npc, restPos)) {
            return;
        }

        if (pathfinderModule.BottomLeftTileOfNPC != restPos) {
            npc.ai[1] = 0f;
            pathfinderModule.RequestPathfind(restPos);
        }
        else if (TownNPCHousingModule.ShouldSleep) {
            Tile restTile = Main.tile[restPos];
            npc.BottomLeft = restPos.ToWorldCoordinates(0f, 16f);
            if (TileID.Sets.CanBeSleptIn[restTile.TileType]) {
                npc.friendlyRegen += 20;

                PlayerSleepingHelper.GetSleepingTargetInfo(restPos.X, restPos.Y, out int targetDirection, out _, out _);
                npc.direction = targetDirection;
                npc.rotation = MathHelper.PiOver2 * -targetDirection;
                Main.sleepingManager.AddNPC(npc.whoAmI, restPos);
                globalNPC.SpriteModule.RequestBlink();

                npc.ai[1] = 1f;
                // TODO: Add mood boost for good rest
                npc.ai[2] += 2f;
            }
            else if (TileID.Sets.CanBeSatOnForNPCs[restTile.TileType]) {
                npc.friendlyRegen += 15;

                npc.SitDown(restPos, out int direction, out _);
                npc.direction = direction;
                Main.sittingManager.AddNPC(npc.whoAmI, restPos);
                globalNPC.SpriteModule.RequestBlink();

                npc.ai[1] = 1f;
                npc.ai[2] += 1f;
            }
            pathfinderModule.CancelPathfind();
        }
    }

    public override void FrameNPC(TownGlobalNPC globalNPC, NPC npc, int frameHeight) {
        Tile restTile = Main.tile[globalNPC.HousingModule.RestPos];
        // Set NPC to sitting frame
        if (npc.ai[1] == 1f && TileID.Sets.CanBeSatOnForNPCs[restTile.TileType]) {
            npc.frame.Y = frameHeight * (Main.npcFrameCount[npc.type] - NPCID.Sets.AttackFrameCount[npc.type] - 3);
        }
    }

    public override void PostDrawNPC(TownGlobalNPC globalNPC, NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (npc.ai[1] != 1f) {
            return;
        }

        Main.instance.LoadItem(ItemID.SleepingIcon);
        Texture2D sleepingIconTexture = TextureAssets.Item[ItemID.SleepingIcon].Value;
        spriteBatch.Draw(
            sleepingIconTexture,
            npc.Top - screenPos + new Vector2(sleepingIconTexture.Width / -2f, -16 + MathF.Sin(Main.GlobalTimeWrappedHourly)),
            drawColor * 0.67f
        );
    }
}