using LivingWorldMod.Custom.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Patches {

    /// <summary>
    /// Class that adds/loads all of the patches for TownNPC-specific methods.
    /// </summary>
    public class TownNPCPatches : ILoadable {

        public void Load(Mod mod) {
            On.Terraria.NPC.AI_007_TownEntities += TownNPCAI;

            On.Terraria.NPC.VanillaFindFrame += TownNPCFindFrame;

            On.Terraria.GameContent.ShopHelper.IsNotReallyTownNPC += ShopHelperTownNPCStatus;

            On.Terraria.NPC.GiveTownUniqueDataToNPCsThatNeedIt += TownNPCNames;
        }

        public void Unload() { }

        private void TownNPCAI(On.Terraria.NPC.orig_AI_007_TownEntities orig, NPC self) {
            bool isVillager = self.IsTypeOfVillager();

            if (isVillager) {
                self.townNPC = true;
            }

            orig(self);

            if (isVillager) {
                self.townNPC = false;
            }
        }

        private void TownNPCFindFrame(On.Terraria.NPC.orig_VanillaFindFrame orig, NPC self, int num) {
            bool isVillager = self.IsTypeOfVillager();

            if (isVillager) {
                self.townNPC = true;
            }

            orig(self, num);

            if (isVillager) {
                self.townNPC = false;
            }
        }

        private bool ShopHelperTownNPCStatus(On.Terraria.GameContent.ShopHelper.orig_IsNotReallyTownNPC orig, Terraria.GameContent.ShopHelper self, NPC npc) {
            return npc.IsTypeOfVillager() || orig(self, npc);
        }

        private void TownNPCNames(On.Terraria.NPC.orig_GiveTownUniqueDataToNPCsThatNeedIt orig, int Type, int nextNPC) {
            orig(NPCUtilities.IsTypeOfVillager(Type) ? NPCID.SkeletonMerchant : Type, nextNPC);
        }
    }
}