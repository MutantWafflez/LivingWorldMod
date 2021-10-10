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

            IL.Terraria.WorldGen.CheckSpecialTownNPCSpawningConditions += PreventNonVillagersFromTakingVillageHouses;
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
            orig(NPCUtils.IsTypeOfVillager(Type) ? NPCID.SkeletonMerchant : Type, nextNPC);
        }

        private void PreventNonVillagersFromTakingVillageHouses(ILContext il) {
            ILCursor c = new ILCursor(il);

            //We do not want non-villagers spawning in villager homes, which is what this patch is for
            //The method runs as normally, but right before it returns, we do a few checks
            //In this method, a return of false will mean that the specific NPC cannot spawn in this house, and true means the opposite
            //We check to see if this NPC already CANNOT spawn in said house for whatever reason, and that acts as normal if true
            //If the NPC CAN spawn here by normal means, we check to see if the room is within a village and if the NPC is a type of villager, and if both are true, prevent the NPC from taking that house
            if (c.TryGotoNext(i => i.MatchRet())) {
                c.Emit(Mono.Cecil.Cil.OpCodes.Pop);

                c.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_0);
                c.Emit(Mono.Cecil.Cil.OpCodes.Ldloc_1);

                c.EmitDelegate<Func<int, bool, bool>>((type, result) => {
                    //If the Town NPC already isn't allowed to spawn here for whatever reason, no need to do any additional fancy stuff
                    if (!result) {
                        return false;
                    }

                    Rectangle roomInQuestion = new Rectangle(WorldGen.roomX1, WorldGen.roomY1, WorldGen.roomX2 - WorldGen.roomX1, WorldGen.roomY2 - WorldGen.roomY1);

                    //HOWEVER, if the Town NPC can spawn here, we need to do additional checks to make sure it's not a non-villager spawning in a villager home
                    if (ModContent.GetModNPC(type) is not Villager && ModContent.GetInstance<WorldCreationSystem>().villageZones.Any(zone => zone.Contains(roomInQuestion))) {
                        return false;
                    }

                    return true;
                });

                //IL in question (Local variable 1 is the return value calculated beforehand, this is literally just a return statement):
                // /* (1331,3)-(1331,4) tModLoader\src\tModLoader\Terraria\WorldGen.cs */
                // /* 0x0047715D 07           */ IL_01AD: ldloc.1
                // /* 0x0047715E 2A           */ IL_01AE: ret
            }
        }
    }
}