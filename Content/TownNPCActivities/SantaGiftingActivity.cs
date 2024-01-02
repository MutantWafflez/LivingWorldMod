using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Common.GlobalNPCs;
using LivingWorldMod.Common.ModTypes;
using LivingWorldMod.Content.TownNPCAIStates;
using LivingWorldMod.Custom.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.TownNPCActivities {
    /// <summary>
    /// Activity where Santa Claus gives a present to some other NPC.
    /// </summary>
    [Autoload(false)]
    public class SantaGiftingActivity : TownNPCActivity {
        public override int ReservedStateInteger => 27;

        private NPC _receivingNPC;

        public override void DoState(TownAIGlobalNPC globalNPC, NPC npc) { }

        public override bool CanDoActivity(TownAIGlobalNPC globalNPC, NPC npc) {
            if (npc.type != NPCID.SantaClaus) {
                return false;
            }

            List<NPC> allTownNPCs = NPCUtils.GetAllNPCs(otherNPC => otherNPC.TryGetGlobalNPC(out TownAIGlobalNPC _) && globalNPC.PathfinderModule.EntityWithinPathfinderZone(otherNPC));
            if (!allTownNPCs.Any()) {
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
}