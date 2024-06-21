using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes;

public abstract class TownNPCModule {
    protected readonly NPC npc;

    protected TownNPCModule(NPC npc) {
        this.npc = npc;
    }

    protected TownGlobalNPC GlobalNPC => npc.GetGlobalNPC<TownGlobalNPC>();

    public virtual void Load() { }

    public abstract void Update();
}