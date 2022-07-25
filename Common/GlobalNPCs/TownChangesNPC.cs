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
    public class TownChangesNPC : GlobalNPC {
        private static RainProfile _rainProfile;

        public override void Load() {
            _rainProfile = new RainProfile();
        }

        public override void Unload() {
            _rainProfile = null;
        }

        public override ITownNPCProfile ModifyTownNPCProfile(NPC npc) {
            //Rain profiles
            if (npc.type == NPCID.Guide) {
                return _rainProfile;
            }

            return null;
        }
    }
}