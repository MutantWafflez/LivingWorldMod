using LivingWorldMod.Common.GlobalNPCs;

namespace LivingWorldMod.Custom.Classes;

public abstract class TownNPCModule {
    protected TownGlobalNPC GlobalNPC => npc.GetGlobalNPC<TownGlobalNPC>();

    protected readonly NPC npc;

    protected TownNPCModule(NPC npc) {
        this.npc = npc;
    }

    public abstract void Update();
}