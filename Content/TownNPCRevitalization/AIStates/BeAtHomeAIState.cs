using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
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
            if (sleepModule.ShouldSleep) {
                Tile restTile = Main.tile[restPos];
                npc.BottomLeft = restPos.ToWorldCoordinates(0f, 16f);
                TownNPCSpriteModule spriteModule = globalNPC.SpriteModule;
                if (TileID.Sets.CanBeSleptIn[restTile.TileType]) {
                    npc.friendlyRegen += 10;

                    PlayerSleepingHelper.GetSleepingTargetInfo(restPos.X, restPos.Y, out int targetDirection, out _, out _);
                    npc.direction = targetDirection;
                    npc.rotation = MathHelper.PiOver2 * -targetDirection;
                    Main.sleepingManager.AddNPC(npc.whoAmI, restPos);
                    spriteModule.CloseEyes();

                    npc.ai[1] = 1f;
                    sleepModule.awakeTicks -= 1.875f;
                    spriteModule.RequestDraw(TownNPCSleepModule.GetSleepSpriteDrawData);
                }
                else if (TileID.Sets.CanBeSatOnForNPCs[restTile.TileType]) {
                    npc.friendlyRegen += 5;

                    npc.SitDown(restPos, out int direction, out _);
                    npc.direction = direction;
                    Main.sittingManager.AddNPC(npc.whoAmI, restPos);
                    spriteModule.CloseEyes();

                    npc.ai[1] = 1f;
                    sleepModule.awakeTicks -= 1.6f;
                    spriteModule.RequestDraw(TownNPCSleepModule.GetSleepSpriteDrawData);
                    spriteModule.RequestFrameOverride((uint)(Main.npcFrameCount[npc.type] - NPCID.Sets.AttackFrameCount[npc.type] - 3));
                }

                pathfinderModule.CancelPathfind();
            }
        }
    }
}