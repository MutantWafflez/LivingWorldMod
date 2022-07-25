using LivingWorldMod.Common.VanillaOverrides.NPCProfiles;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.GlobalNPCs {
    /// <summary>
    /// GlobalNPC that handles visual updates on town NPCs for various aesthetic purposes.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class TownProfileNPC : GlobalNPC {
        public override ITownNPCProfile ModifyTownNPCProfile(NPC npc) {
            //Rain profiles
            if (npc.type == NPCID.Guide) {
                return new GuideProfile();
            }

            return null;
        }
    }
}