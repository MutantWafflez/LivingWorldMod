using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;

namespace LivingWorldMod.Content.TownNPCRevitalization.AIStates;

/// <summary>
/// Town NPC AI state that occurs when an NPC has reached the maximum amount of ticks awake, and "passes out" due to exhaustion.
/// </summary>
public class PassedOutAIState : TownNPCAIState {
    public override void DoState(TownGlobalNPC globalNPC, NPC npc) {
        throw new System.NotImplementedException();
    }
}