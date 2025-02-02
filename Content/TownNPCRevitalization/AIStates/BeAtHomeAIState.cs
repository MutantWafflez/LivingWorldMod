using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Enums;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs.TownNPCModules;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria.GameContent;

namespace LivingWorldMod.Content.TownNPCRevitalization.AIStates;

public sealed class BeAtHomeAIState : TownNPCAIState {
    /// <summary>
    ///     NPC.ai[1] will be set to this value when the NPC is sleeping.
    /// </summary>
    public const float IsSleepingStateFlag = 1f;

    private const float RetryPathfindToHomeCooldown = LWMUtils.RealLifeSecond * 5;

    public override void DoState(NPC npc) {
        if (!npc.GetGlobalNPC<TownNPCHousingModule>().ShouldGoHome) {
            TownNPCStateModule.RefreshToState<DefaultAIState>(npc);
            return;
        }

        (Point pathfindPos, Point restTilePos, NPCRestType npcRestType) = npc.GetGlobalNPC<TownNPCHousingModule>().RestInfo;
        TownNPCPathfinderModule pathfinderModule = npc.GetGlobalNPC<TownNPCPathfinderModule>();
        if (!TownGlobalNPC.IsValidStandingPosition(npc, pathfindPos)) {
            return;
        }

        npc.ai[1] = 0f;
        if (pathfinderModule.BottomLeftTileOfNPC != pathfindPos) {
            if (--npc.ai[2] > 0) {
                return;
            }

            npc.ai[2] = RetryPathfindToHomeCooldown;
            pathfinderModule.RequestPathfind(pathfindPos);
        }
        else {
            npc.ai[2] = 0;
            TownNPCSleepModule sleepModule = npc.GetGlobalNPC<TownNPCSleepModule>();
            if (!sleepModule.CanSleep) {
                return;
            }

            npc.BottomLeft = pathfindPos.ToWorldCoordinates(8f, 16f);

            TownNPCSpriteModule spriteModule = npc.GetGlobalNPC<TownNPCSpriteModule>();
            TownNPCChatModule chatModule = npc.GetGlobalNPC<TownNPCChatModule>();
            float currentSleepQualityModifier = (float)Main.dayRate * sleepModule.SleepQualityModifier;
            switch (npcRestType) {
                case NPCRestType.Bed:
                    npc.friendlyRegen += 10;

                    PlayerSleepingHelper.GetSleepingTargetInfo(restTilePos.X, restTilePos.Y, out int targetDirection, out _, out Vector2 visualOffset);
                    npc.direction = targetDirection;
                    npc.rotation = MathHelper.PiOver2 * -targetDirection;
                    Main.sleepingManager.AddNPC(npc.whoAmI, restTilePos);

                    sleepModule.awakeTicks -= 1.875f * currentSleepQualityModifier;

                    spriteModule.OffsetDrawPosition(targetDirection == 1 ? new Vector2(npc.width / 2f, visualOffset.Y) : new Vector2(npc.width, visualOffset.Y));
                    break;
                case NPCRestType.Chair:
                    npc.friendlyRegen += 5;

                    npc.SitDown(restTilePos, out int direction, out Vector2 newBottom);
                    npc.direction = direction;
                    Main.sittingManager.AddNPC(npc.whoAmI, restTilePos);

                    sleepModule.awakeTicks -= 1.6f * currentSleepQualityModifier;

                    spriteModule.RequestFrameOverride((uint)(Main.npcFrameCount[npc.type] - NPCID.Sets.AttackFrameCount[npc.type] - 3));
                    spriteModule.OffsetDrawPosition(newBottom - npc.Bottom);
                    break;
                default:
                case NPCRestType.None:
                    npc.friendlyRegen += 2;

                    npc.direction = -1;
                    npc.rotation = MathHelper.PiOver2;

                    sleepModule.awakeTicks -= 1.2f * currentSleepQualityModifier;

                    spriteModule.OffsetDrawPosition(new Vector2(0, npc.width));
                    break;
            }

            npc.ai[1] = IsSleepingStateFlag;
            spriteModule.RequestDraw(sleepModule.GetSleepSpriteDrawData);
            spriteModule.CloseEyes();

            chatModule.DisableChatting(LWMUtils.RealLifeSecond);
            chatModule.DisableChatReception(LWMUtils.RealLifeSecond);

            pathfinderModule.CancelPathfind();
        }
    }
}