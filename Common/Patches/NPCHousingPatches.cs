using System;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using LivingWorldMod.Common.Systems;
using LivingWorldMod.Common.Systems.UI;
using LivingWorldMod.Content.NPCs.Villagers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Patches {

    /// <summary>
    /// Class that contains IL/On patches for NPC housing-related manners.
    /// </summary>
    public class NPCHousingPatches : ILoadable {

        public void Load(Mod mod) {
            IL.Terraria.WorldGen.CheckSpecialTownNPCSpawningConditions += PreventNonVillagersFromTakingVillageHouses;

            IL.Terraria.Main.DrawInterface_38_MouseCarriedObject += DrawSelectedVillagerOnMouse;

            IL.Terraria.Main.DrawInterface_7_TownNPCHouseBanners += ShowBannersInVillagerHousingMenu;

            IL.Terraria.Main.DrawNPCHousesInWorld += DrawVillagerBannerInHouses;
        }

        public void Unload() { }

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

        private void DrawSelectedVillagerOnMouse(ILContext il) {
            //Run-down of this edit:
            //We want to draw the selected villager near the mouse, similar to how vanilla draws NPC heads on the mouse when the player is changing around houses
            //So, we want to avoid the head being drawn since there is no head sprite for villagers; so we will get an "exit" instruction that is after the head
            // drawing code using a label. The head will draw as normal if it the NPC in question is a normal NPC, otherwise, our special draw code takes over.

            ILCursor c = new ILCursor(il);

            bool foundExitInstruction = false;
            ILLabel exitLabel = c.DefineLabel();

            //Get label of instruction we will be transforming to. This IL edit will not apply if this exit label is not created
            if (c.TryGotoNext(i => i.MatchLdloc(15))) {
                foundExitInstruction = true;

                exitLabel = c.MarkLabel();
            }

            c.Index = 0;

            //If the target instruction is found and we found the exit instruction, draw the villager if applicable
            if (c.TryGotoNext(i => i.MatchLdloc(9)) && foundExitInstruction) {
                //What we return here will determine whether or not we skip past the drawing head step in the vanilla function.
                c.EmitDelegate<Func<bool>>(() => {
                    if (Main.instance.mouseNPCIndex > -1 && Main.npc[Main.instance.mouseNPCIndex].ModNPC is Villager villager) {
                        float drawScale = 0.67f;

                        Texture2D bodyTexture = villager.bodyAssets[villager.bodySpriteType].Value;
                        Texture2D headTexture = villager.headAssets[villager.headSpriteType].Value;

                        Vector2 drawPos = new Vector2(Main.mouseX, Main.mouseY);
                        Rectangle textureDrawRegion = new Rectangle(0, 0, bodyTexture.Width, bodyTexture.Height / Main.npcFrameCount[villager.Type]);

                        Main.spriteBatch.Draw(bodyTexture, drawPos, textureDrawRegion, Color.White, 0f, Vector2.Zero, Main.cursorScale * drawScale, SpriteEffects.None, 0f);
                        Main.spriteBatch.Draw(headTexture, drawPos, textureDrawRegion, Color.White, 0f, Vector2.Zero, Main.cursorScale * drawScale, SpriteEffects.None, 0f);

                        //If a type of villager, we do not want the head drawing function, so skip it by returning true
                        return true;
                    }

                    //If not a type of villager or otherwise an invalid index (or just the above if statement failing in general), then return false and have the head draw as normal.
                    return false;
                });

                //Actual instruction that causes the "skipping." This instruction is why the exit label is necessary, since without it, the IL literally won't function and the head will draw.
                c.Emit(Mono.Cecil.Cil.OpCodes.Brtrue_S, exitLabel);
            }
        }

        private void ShowBannersInVillagerHousingMenu(ILContext il) {
            //Edit rundown:
            //Very simple this time. We need to simply allow for the banners to be drawn while in the villager housing menu along with the normal
            // vanilla housing menu.

            ILCursor c = new ILCursor(il);

            //Navigate to EquipPage check
            if (c.TryGotoNext(i => i.MatchLdsfld<Main>(nameof(Main.EquipPage)))) {
                //If we correctly navigated to the EquipPage, we need to get the label of the beq_s that is associated with it (see IL block below)
                if (c.Instrs[c.Index + 2].OpCode == Mono.Cecil.Cil.OpCodes.Beq_S) {
                    //Steal the label from the beq_s
                    ILLabel stolenLabel = (ILLabel)c.Instrs[c.Index + 2].Operand;

                    //Remove all the instructions in this IL block so we can re-add it and slip in our own check
                    for (int i = 0; i < 3; i++) {
                        c.Remove();
                    }

                    //Re-add our check, making sure it's inverted compared to the IL, since in IL it determines if the code should run if these values are FALSE,
                    // but since we took control of the instructions, we can test based on if it's true or not for easy understanding
                    c.EmitDelegate<Func<bool>>(() => Main.EquipPage == 1 || ModContent.GetInstance<VillagerHousingUISystem>().housingState.isMenuVisible);

                    c.Emit(Mono.Cecil.Cil.OpCodes.Brtrue_S, stolenLabel);
                }
                else {
                    //Throw error is the code does not match, since this IL edit is kind of a necessary functionality of the mod
                    throw new ArgumentNullException(c.Instrs[c.Index + 2].OpCode.ToString(), "Expected Beq_S op-code, got wrong value.");
                }
            }

            //IL Block in question ^:
            // /* 0x00112F7D 7E2F030004   */ IL_0001: ldsfld    int32 Terraria.Main::EquipPage
            // /* 0x00112F82 17           */ IL_0006: ldc.i4.1
            // /* 0x00112F83 2E14         */ IL_0007: beq.s     IL_001D
        }

        private void DrawVillagerBannerInHouses(ILContext il) {
            //Edit rundown:
            //We need to have the villagers who are living in villager homes be displayed on their banners, like they are for normal town NPCs
            //We will do this by hijacking the if statement that tests if the NPC is a town NPC and also test whether or not they are a villager
            //If they are a villager, move to our code for drawing, otherwise, do normal vanilla code

            ILCursor c = new ILCursor(il);

            //Navigate to first TownNPC check
            if (c.TryGotoNext(i => i.MatchLdfld<NPC>(nameof(NPC.townNPC)))) {
                //Remove townNPC check in order to add our villager check
                c.Remove();

                c.EmitDelegate<Func<NPC, bool>>(npc => npc.ModNPC is Villager || npc.townNPC);
            }

            //First IL block in question for the statement above ^:
            // /* 0x00101758 7ED8040004   */ IL_0020: ldsfld    class Terraria.NPC[] Terraria.Main::npc
            // /* 0x0010175D 06           */ IL_0025: ldloc.0
            // /* 0x0010175E 9A           */ IL_0026: ldelem.ref
            // /* 0x0010175F 7BE3070004   */ IL_0027: ldfld     bool Terraria.NPC::townNPC
            // /* 0x00101764 2C41         */ IL_002C: brfalse.s IL_006F

            //Navigate to second TownNPC check
            if (c.TryGotoNext(i => i.MatchLdfld<NPC>(nameof(NPC.townNPC)))) {
                //Remove townNPC check in order to add our villager check (again)
                c.Remove();

                //In this context, the game is checking if the npc is NOT a villager/townNPC
                c.EmitDelegate<Func<NPC, bool>>(npc => npc.ModNPC is not Villager || !npc.townNPC);
            }

            //Second IL block in question for the statement above ^:
            // /* 0x0010180E 1105         */ IL_00D6: ldloc.s   nPC
            // /* 0x00101810 7BE3070004   */ IL_00D8: ldfld     bool Terraria.NPC::townNPC
            // /* 0x00101815 2C2A         */ IL_00DD: brfalse.s IL_0109
        }
    }
}