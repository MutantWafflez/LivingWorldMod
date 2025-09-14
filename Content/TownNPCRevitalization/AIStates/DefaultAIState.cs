using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;
using LivingWorldMod.Globals.Configs;
using LivingWorldMod.Utilities;

namespace LivingWorldMod.Content.TownNPCRevitalization.AIStates;

/// <summary>
///     The state where the NPC is standing still and simply occasionally
///     looking left and right. This state is the one all Town NPCs will be
///     in if they have absolutely nothing to do.
/// </summary>
public class DefaultAIState : TownNPCAIState {
    public override int ReservedStateInteger => 0;

    public override void DoState(NPC npc) {
        if (npc.velocity.Y == 0) {
            npc.velocity *= 0.75f;
        }

        if (Main.netMode == NetmodeID.MultiplayerClient) {
            return;
        }

        if (npc.breath == 0) {
            TownNPCStateModule.RefreshToState<WalkToRandomPosState>(npc);
            return;
        }

        if (--npc.ai[1] > 0) {
            return;
        }

        if ((LWM.IsDebug && ModContent.GetInstance<DebugConfig>().guaranteedWanderOffCooldown) || Main.rand.NextBool(4, 10)) {
            TownNPCStateModule.RefreshToState<WalkToRandomPosState>(npc);
            return;
        }

        npc.ai[1] = Main.rand.Next(LWMUtils.RealLifeSecond * 5, LWMUtils.RealLifeSecond * 8);
        npc.direction = -npc.direction;
        npc.netUpdate = true;
    }
}