namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;

/// <summary>
///     Base type that allows for the freedom of <see cref="GlobalNPC" />, but with constraints on how it is updated when the NPC is active. The AI methods (PreAI, AI, and PostAI) are sealed and
///     should not be overriden in any way; use the <see cref="UpdateModule" /> method instead. This allows for module update priority, instead of updating in relation to load order.
/// </summary>
public abstract class TownNPCModule : GlobalNPC {
    /// <summary>
    ///     The priority of this module for updates when the applied NPC runs its AI. The lower the number, the "higher" the priority (or the earlier it is ran in NPC AI.) Defaults to 0.
    /// </summary>
    public virtual int UpdatePriority => 0;

    public sealed override bool InstancePerEntity => true;

    /// <summary>
    ///     Acts as the substitute for the <see cref="AI" /> method. Use this instead of trying to override the various AI methods.
    /// </summary>
    /// <param name="npc"> The NPC this module is applied to. </param>
    public virtual void UpdateModule(NPC npc) { }

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => TownGlobalNPC.EntityIsValidTownNPC(entity, lateInstantiation);

    public sealed override bool PreAI(NPC npc) => base.PreAI(npc);

    public sealed override void AI(NPC npc) {
        base.AI(npc);
    }

    public sealed override void PostAI(NPC npc) {
        base.PostAI(npc);
    }
}