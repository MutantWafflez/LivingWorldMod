using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Enums;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Hooks;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.TownNPCRevitalization.AIStates;

public sealed class BeAtHomeAIState : TownNPCAIState {
    /// <summary>
    ///     NPC.ai[1] will be set to this value when the NPC is sleeping.
    /// </summary>
    public const float IsSleepingStateFlag = 2f;

    private const float StartSleepAnimationState = 0f;
    private const float EndSleepAnimationState = 1f;

    private const float RetryPathfindToHomeCooldown = LWMUtils.RealLifeSecond * 5;

    private static bool IsSittingAnimationFinished(in NPC npc) => (int)npc.ai[1] != IsSleepingStateFlag;

    public override void DoState(NPC npc) {
        if (!npc.GetGlobalNPC<TownNPCHousingModule>().WillGoHome) {
            TownNPCStateModule.RefreshToState<DefaultAIState>(npc);
            return;
        }

        (Point pathfindPos, Point restTilePos, NPCRestType npcRestType, BedInfo? bedInfo) = npc.GetGlobalNPC<TownNPCHousingModule>().RestInfo;
        TownNPCPathfinderModule pathfinderModule = npc.GetGlobalNPC<TownNPCPathfinderModule>();
        if (!TownGlobalNPC.IsValidStandingPosition(npc, pathfindPos)) {
            return;
        }

        if (pathfinderModule.BottomLeftTileOfNPC != pathfindPos) {
            npc.ai[1] = 0;
            if (--npc.ai[2] > 0) {
                return;
            }

            npc.ai[2] = RetryPathfindToHomeCooldown;
            pathfinderModule.RequestPathfind(pathfindPos);

            return;
        }

        if (!npc.TryGetGlobalNPC(out TownNPCSleepModule sleepModule) || !sleepModule.CanSleep) {
            npc.ai[1] = StartSleepAnimationState;
            return;
        }

        npc.ai[2] = 0f;
        switch (npc.ai[1]) {
            case StartSleepAnimationState:
                npc.GetGlobalNPC<TownNPCAnimationModule>().RequestAnimation(TownNPCAnimationModule.GetGiveItemAnimation(in npc));

                npc.ai[3] = 0f;
                npc.ai[1] = EndSleepAnimationState;
                return;
            case EndSleepAnimationState:
                if (++npc.ai[3] <= TownNPCAnimationModule.GiveItemAnimationDuration) {
                    return;
                }

                npc.ai[1] = IsSleepingStateFlag;
                npc.ai[3] = 0f;
                break;
        }

        npc.BottomLeft = pathfindPos.ToWorldCoordinates(8f, 16f);

        float currentSleepQualityModifier = (float)Main.dayRate * sleepModule.SleepQualityModifier;
        Vector2? drawOffset;
        uint? frameOverride = null;
        switch (npcRestType) {
            case NPCRestType.Bed:
                // If BedInfo is broken for some reason, jump to sleeping on the floor at the foot of the bed.
                if (bedInfo is not var (_, _, sleepDirection, anchorPosition, sleepDrawOffset)) {
                    goto case NPCRestType.Floor;
                }

                npc.friendlyRegen += 10;
                npc.direction = sleepDirection;
                npc.rotation = MathHelper.PiOver2 * -sleepDirection;

                Main.sleepingManager.AddNPC(npc.whoAmI, restTilePos);

                sleepModule.awakeTicks -= 1.875f * currentSleepQualityModifier;

                Vector2 visualOffset = anchorPosition - npc.Bottom + sleepDrawOffset;
                drawOffset = sleepDirection == 1 ? new Vector2(npc.width / 2f, visualOffset.Y) : new Vector2(npc.width, visualOffset.Y);
                break;
            case NPCRestType.Chair:
                npc.friendlyRegen += 5;

                npc.SitDown(restTilePos, out int direction, out Vector2 newBottom);
                npc.direction = direction;
                Main.sittingManager.AddNPC(npc.whoAmI, restTilePos);

                sleepModule.awakeTicks -= 1.6f * currentSleepQualityModifier;

                drawOffset = newBottom - npc.Bottom;

                npc.GetGlobalNPC<TownNPCAnimationModule>().RequestAnimation(TownNPCAnimationModule.GetSittingAnimation(npc, IsSittingAnimationFinished));
                break;
            default:
            case NPCRestType.Floor:
                npc.friendlyRegen += 2;

                npc.direction = -1;
                npc.rotation = MathHelper.PiOver2;

                sleepModule.awakeTicks -= 1.2f * currentSleepQualityModifier;

                drawOffset = new Vector2(0, npc.width);
                break;
        }

        npc.ai[1] = IsSleepingStateFlag;
        IUpdateSleep.Invoke(npc, drawOffset, npcRestType, bedInfo);

        pathfinderModule.CancelPathfind();
    }
}