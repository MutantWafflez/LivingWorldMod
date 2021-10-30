using System;
using System.Linq;
using LivingWorldMod.Common.Systems;
using LivingWorldMod.Content.NPCs.Villagers;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
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
            bool isVillager = self.ModNPC is Villager;

            if (isVillager) {
                self.townNPC = true;
            }

            orig(self);

            if (isVillager) {
                self.townNPC = false;
            }
        }

        private void TownNPCFindFrame(On.Terraria.NPC.orig_VanillaFindFrame orig, NPC self, int num) {
            bool isVillager = self.ModNPC is Villager;

            if (isVillager) {
                self.townNPC = true;
            }

            orig(self, num);

            if (isVillager) {
                self.townNPC = false;
            }
        }

        private bool ShopHelperTownNPCStatus(On.Terraria.GameContent.ShopHelper.orig_IsNotReallyTownNPC orig, Terraria.GameContent.ShopHelper self, NPC npc) {
            return npc.ModNPC is Villager || orig(self, npc);
        }

        private void TownNPCNames(On.Terraria.NPC.orig_GiveTownUniqueDataToNPCsThatNeedIt orig, int Type, int nextNPC) {
            orig(NPCUtils.IsTypeOfVillager(Type) ? NPCID.SkeletonMerchant : Type, nextNPC);
        }
    }
}