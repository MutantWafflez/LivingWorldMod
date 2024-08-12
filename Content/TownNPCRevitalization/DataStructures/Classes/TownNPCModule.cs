using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes;

public abstract class TownNPCModule (NPC npc, TownGlobalNPC globalNPC) {
    protected readonly NPC npc = npc;

    protected readonly TownGlobalNPC globalNPC = globalNPC;

    public abstract void Update();
}