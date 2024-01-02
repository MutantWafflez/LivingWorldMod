using LivingWorldMod.Common.ModTypes;
using LivingWorldMod.Content.TownNPCAIStates;

namespace LivingWorldMod.Common.GlobalNPCs;

// Temporary class to "wake up" NPCs in worlds where they were previously sleeping.
// Created to completely disable town NPC revitalization features.
public class FixSleepingNPC : GlobalNPC {
    public override void AI(NPC npc) {
        if (npc.ai[0] != TownNPCAIState.GetStateInteger<BeAtHomeAIState>()) {
            return;
        }

        npc.ai[0] = 0f;
        npc.netUpdate = true;
    }
}