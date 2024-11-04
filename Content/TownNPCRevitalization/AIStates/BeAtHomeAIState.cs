using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria.GameContent;

namespace LivingWorldMod.Content.TownNPCRevitalization.AIStates;

public sealed class BeAtHomeAIState : TownNPCAIState {
    public override void DoState(TownGlobalNPC globalNPC, NPC npc) {
        if (!globalNPC.HousingModule.ShouldGoHome) {
            TownGlobalNPC.RefreshToState<DefaultAIState>(npc);
            return;
        }

        TownNPCPathfinderModule pathfinderModule = globalNPC.PathfinderModule;
        Point restPos = globalNPC.HousingModule.RestPos;
        if (!TownGlobalNPC.IsValidStandingPosition(npc, restPos)) {
            return;
        }

        npc.ai[1] = 0f;
        if (pathfinderModule.BottomLeftTileOfNPC != restPos) {
            pathfinderModule.RequestPathfind(restPos);
        }
        else {
            TownNPCSleepModule sleepModule = globalNPC.SleepModule;
            if (!sleepModule.ShouldSleep) {
                return;
            }

            npc.BottomLeft = restPos.ToWorldCoordinates(0f, 16f);

            Tile restTile = Main.tile[restPos];
            TownNPCSpriteModule spriteModule = globalNPC.SpriteModule;
            TownNPCChatModule chatModule = globalNPC.ChatModule;
            if (TileID.Sets.CanBeSleptIn[restTile.TileType]) {
                npc.friendlyRegen += 10;

                PlayerSleepingHelper.GetSleepingTargetInfo(restPos.X, restPos.Y, out int targetDirection, out _, out _);
                npc.direction = targetDirection;
                npc.rotation = MathHelper.PiOver2 * -targetDirection;
                npc.ai[1] = 1f;
                Main.sleepingManager.AddNPC(npc.whoAmI, restPos);

                sleepModule.isAsleep = true;
                sleepModule.awakeTicks -= 1.875f;

                spriteModule.CloseEyes();
                spriteModule.RequestDraw(TownNPCSleepModule.GetSleepSpriteDrawData);

                chatModule.DisableChatting(LWMUtils.RealLifeSecond);
                chatModule.DisableChatReception(LWMUtils.RealLifeSecond);
            }
            else if (TileID.Sets.CanBeSatOnForNPCs[restTile.TileType]) {
                npc.friendlyRegen += 5;

                npc.SitDown(restPos, out int direction, out _);
                npc.direction = direction;
                npc.ai[1] = 1f;
                Main.sittingManager.AddNPC(npc.whoAmI, restPos);

                sleepModule.isAsleep = true;
                sleepModule.awakeTicks -= 1.6f;

                spriteModule.CloseEyes();
                spriteModule.RequestDraw(TownNPCSleepModule.GetSleepSpriteDrawData);
                spriteModule.RequestFrameOverride((uint)(Main.npcFrameCount[npc.type] - NPCID.Sets.AttackFrameCount[npc.type] - 3));

                chatModule.DisableChatting(LWMUtils.RealLifeSecond);
                chatModule.DisableChatReception(LWMUtils.RealLifeSecond);
            }

            pathfinderModule.CancelPathfind();
        }
    }
}