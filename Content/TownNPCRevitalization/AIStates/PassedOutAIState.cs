using LivingWorldMod.Content.TownNPCRevitalization.Globals.Hooks;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs.TownNPCModules;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.TownNPCRevitalization.AIStates;

/// <summary>
///     Town NPC AI state that occurs when an NPC has reached the maximum amount of ticks awake, and "passes out" due to exhaustion.
/// </summary>
public class PassedOutAIState : TownNPCAIState {
    public override void DoState( NPC npc) {
        TownNPCSleepModule sleepModule = npc.GetGlobalNPC<TownNPCSleepModule>();
        npc.direction = 1;
        npc.rotation = MathHelper.PiOver2;

        IUpdateSleep.Invoke(npc, new Vector2(0, npc.width), null, true);

        if ((sleepModule.awakeTicks -= 2f * (float)Main.dayRate) <= 0) {
            TownNPCStateModule.RefreshToState<DefaultAIState>(npc);
        }
    }
}