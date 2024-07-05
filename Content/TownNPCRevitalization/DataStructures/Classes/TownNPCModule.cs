using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes;

public abstract class TownNPCModule (NPC npc) {
    protected readonly NPC npc = npc;

    protected TownGlobalNPC GlobalNPC => npc.GetGlobalNPC<TownGlobalNPC>();

    public abstract void Update();
}