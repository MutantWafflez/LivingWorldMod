﻿using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Enums;
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

        (Point pathfindPos, Point restTilePos, NPCRestType npcRestType) = globalNPC.HousingModule.RestInfo;
        TownNPCPathfinderModule pathfinderModule = globalNPC.PathfinderModule;
        if (!TownGlobalNPC.IsValidStandingPosition(npc, pathfindPos)) {
            return;
        }

        npc.ai[1] = 0f;
        if (pathfinderModule.BottomLeftTileOfNPC != pathfindPos) {
            pathfinderModule.RequestPathfind(pathfindPos);
        }
        else {
            TownNPCSleepModule sleepModule = globalNPC.SleepModule;
            if (!sleepModule.ShouldSleep) {
                return;
            }

            npc.BottomLeft = pathfindPos.ToWorldCoordinates(8f, 16f);

            TownNPCSpriteModule spriteModule = globalNPC.SpriteModule;
            TownNPCChatModule chatModule = globalNPC.ChatModule;
            float currentSleepQualityModifier = (float)Main.dayRate * sleepModule.SleepQualityModifier;
            switch (npcRestType) {
                case NPCRestType.Bed:
                    npc.friendlyRegen += 10;

                    PlayerSleepingHelper.GetSleepingTargetInfo(restTilePos.X, restTilePos.Y, out int targetDirection, out _, out Vector2 visualOffset);
                    npc.direction = targetDirection;
                    npc.rotation = MathHelper.PiOver2 * -targetDirection;
                    npc.ai[1] = 1f;
                    Main.sleepingManager.AddNPC(npc.whoAmI, restTilePos);

                    sleepModule.isAsleep = true;
                    sleepModule.awakeTicks -= 1.875f * currentSleepQualityModifier;

                    spriteModule.CloseEyes();
                    spriteModule.RequestDraw(sleepModule.GetSleepSpriteDrawData);
                    spriteModule.OffsetDrawPosition(targetDirection == 1 ? new Vector2(npc.width / 2f, visualOffset.Y) : new Vector2(npc.width, visualOffset.Y));

                    chatModule.DisableChatting(LWMUtils.RealLifeSecond);
                    chatModule.DisableChatReception(LWMUtils.RealLifeSecond);
                    break;
                case NPCRestType.Chair:
                    npc.friendlyRegen += 5;

                    npc.SitDown(restTilePos, out int direction, out Vector2 newBottom);
                    npc.direction = direction;
                    npc.ai[1] = 1f;
                    Main.sittingManager.AddNPC(npc.whoAmI, restTilePos);

                    sleepModule.isAsleep = true;
                    sleepModule.awakeTicks -= 1.6f * currentSleepQualityModifier;

                    spriteModule.CloseEyes();
                    spriteModule.RequestDraw(sleepModule.GetSleepSpriteDrawData);
                    spriteModule.RequestFrameOverride((uint)(Main.npcFrameCount[npc.type] - NPCID.Sets.AttackFrameCount[npc.type] - 3));
                    spriteModule.OffsetDrawPosition(newBottom - npc.Bottom);

                    chatModule.DisableChatting(LWMUtils.RealLifeSecond);
                    chatModule.DisableChatReception(LWMUtils.RealLifeSecond);
                    break;
            }

            pathfinderModule.CancelPathfind();
        }
    }
}