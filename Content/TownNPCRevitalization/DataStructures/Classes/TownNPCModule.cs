using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes;

public abstract class TownNPCModule {
    protected TownGlobalNPC GlobalNPC => npc.GetGlobalNPC<TownGlobalNPC>();

    protected readonly NPC npc;

    protected TownNPCModule(NPC npc) {
        this.npc = npc;
    }

    public abstract void Update();
}