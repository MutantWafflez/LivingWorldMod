using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.TownNPCRevitalization.AIStates;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;

namespace LivingWorldMod.Content.TownNPCRevitalization.Activities;

/// <summary>
/// Activity where Santa Claus gives a present to some other NPC.
/// </summary>
[Autoload(false)]
public class SantaGiftingActivity : TownNPCActivity {
    private NPC _receivingNPC;

    public override void DoState(TownGlobalNPC globalNPC, NPC npc) { }

    public override bool CanDoActivity(TownGlobalNPC globalNPC, NPC npc) {
        if (npc.type != NPCID.SantaClaus) {
            return false;
        }

        List<NPC> allTownNPCs = new(); /*LWMUtils.GetAllNPCs(otherNPC => otherNPC.TryGetGlobalNPC(out TownGlobalNPC _) && globalNPC.PathfinderModule.EntityWithinPathfinderZone(otherNPC));*/
        if (allTownNPCs.Count == 0) {
            return false;
        }

        NPC selectedNPC = allTownNPCs.FirstOrDefault(otherNPC => (int)otherNPC.ai[0] == GetStateInteger<DefaultAIState>());
        if (selectedNPC is null) {
            return false;
        }

        _receivingNPC = selectedNPC;
        return true;
    }
}