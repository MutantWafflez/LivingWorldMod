using LivingWorldMod.Common.GlobalNPCs;

namespace LivingWorldMod.Common.ModTypes;

/// <summary>
/// More advanced version of a <see cref="TownNPCAIState"/> that requires
/// checking before it can performed.
/// </summary>
public abstract class TownNPCActivity : TownNPCAIState {
    /// <summary>
    /// Initializes the NPC to begin this activity. By default, sets
    /// all AI values to 0 except for index 0, which is set to
    /// <see cref="TownNPCAIState.ReservedStateInteger"/> .
    /// </summary>
    /// <param name="npc"></param>
    public virtual void InitializeActivity(NPC npc) {
        npc.ai[0] = ReservedStateInteger;

        for (int i = 1; i < NPC.maxAI; i++) {
            npc.ai[i] = 0;
        }
    }

    /// <summary>
    /// Returns whether or not the passed in NPC can do this activity. Determines
    /// whether or not this activity will be performed.
    /// </summary>
    public abstract bool CanDoActivity(TownGlobalNPC globalNPC, NPC npc);
}