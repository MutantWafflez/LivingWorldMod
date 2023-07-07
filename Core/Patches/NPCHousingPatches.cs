using LivingWorldMod.Common.Systems.UI;
using LivingWorldMod.Content.NPCs.Villagers;
using LivingWorldMod.Content.TileEntities.Interactables;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LivingWorldMod.Common.GlobalNPCs;
using LivingWorldMod.Custom.Classes;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Achievements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Core.Patches {
    /// <summary>
    /// Class that contains IL/On patches for NPC housing-related manners.
    /// </summary>
    public class NPCHousingPatches : ILoadable {
        public static Point HouseBedPosition = Point.Zero;

        public void Load(Mod mod) {
            Terraria.IL_WorldGen.CheckSpecialTownNPCSpawningConditions += TestForVillageHouse;

            Terraria.IL_Main.DrawInterface_38_MouseCarriedObject += DrawSelectedVillagerOnMouse;

            Terraria.IL_Main.DrawInterface_7_TownNPCHouseBanners += BannersVisibleWhileInVillagerHousingMenu;

            Terraria.IL_Main.DrawNPCHousesInWorld += DrawVillagerBannerInHouses;

            Terraria.IL_WorldGen.ScoreRoom_IsThisRoomOccupiedBySomeone += RoomOccupancyCheck;

            Terraria.IL_WorldGen.ScoreRoom += IgnoreRoomOccupancy;

            //TODO: Finish NPC Sleeping Tests
            /*
            IL.Terraria.WorldGen.ScoreRoom += FindRoomBed;

            IL.Terraria.WorldGen.QuickFindHome += AssignBedToNPC;

            IL.Terraria.WorldGen.SpawnTownNPC += AssignBedToNPC; // I love when one edit can be used in two methods
            */
        }

        public void Unload() { }

        private void TestForVillageHouse(ILContext il) {
            ILCursor c = new ILCursor(il);

            //We do not want non-villagers spawning in villager homes, which is what this patch is for
            //In this method, a return of false will mean that the specific NPC cannot spawn in this house, and true means the opposite
            //We check to see if this NPC already CANNOT spawn in said house for whatever reason at the beginning, and that acts as normal if true
            //If the NPC CAN spawn here by normal means, we check to see if the room is within a village and if the NPC is a type of villager, and if both are true, prevent the NPC from taking that house

            //Navigate to first false check
            c.ErrorOnFailedGotoNext(i => i.MatchCall(typeof(NPCLoader).FullName, nameof(NPCLoader.CheckConditions)));

            //Move to the beginning of the return instruction block and set a label there
            c.Index += 2;
            ILLabel falseReturnLabel = c.MarkLabel();
            c.Index = 0;
            //Get type from passed in parameter
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<int, bool>>(type => {
                Rectangle roomInQuestion = new Rectangle(WorldGen.roomX1, WorldGen.roomY1, WorldGen.roomX2 - WorldGen.roomX1, WorldGen.roomY2 - WorldGen.roomY1);

                ModNPC modNPC = ModContent.GetModNPC(type);
                List<VillageShrineEntity> shrines = TileEntityUtils.GetAllEntityOfType<VillageShrineEntity>().ToList();

                //HOWEVER, if the Town NPC can spawn here, we need to do additional checks to make sure it's not a non-villager spawning in a villager home
                //Additionally, we can't have villagers in a non-village home, which is the second check.
                if (modNPC is not Villager && shrines.Any(shrine => shrine.villageZone.ContainsPoint(roomInQuestion.Center().ToWorldCoordinates()))
                    || modNPC is Villager && !shrines.Any(shrine => shrine.villageZone.ContainsPoint(roomInQuestion.Center().ToWorldCoordinates()))) {
                    return false;
                }

                return true;
            });
            c.Emit(OpCodes.Brfalse_S, falseReturnLabel);

            //IL for the above edit (Local variable 1 is the return value calculated beforehand) ^:
            // /* (1296,4)-(1296,41) tModLoader\src\tModLoader\Terraria\WorldGen.cs */
            // /* 0x003CEBC0 */ IL_0000: ldarg.0
            // /* 0x003CEBC1 */ IL_0001: call      bool Terraria.ModLoader.NPCLoader::CheckConditions(int32)
            // /* 0x003CEBC6 */ IL_0006: brtrue.s  IL_000A

            // /* (1297,5)-(1297,18) tModLoader\src\tModLoader\Terraria\WorldGen.cs */
            // /* 0x003CEBC8 */ IL_0008: ldc.i4.0
            // /* 0x003CEBC9 */ IL_0009: ret

            // /* (1298,4)-(1298,20) tModLoader\src\tModLoader\Terraria\WorldGen.cs */
            // /* 0x003CEBCA */ IL_000A: ldarg.0
            // /* 0x003CEBCB */ IL_000B: ldc.i4    160
            // /* 0x003CEBD0 */ IL_0010: bne.un    IL_00B9
        }

        private void DrawSelectedVillagerOnMouse(ILContext il) {
            //Run-down of this edit:
            //We want to draw the selected villager near the mouse, similar to how vanilla draws NPC heads on the mouse when the player is changing around houses
            //So, we want to avoid the head being drawn since there is no head sprite for villagers; so we will get an "exit" instruction that is after the head
            // drawing code using a label. The head will draw as normal if it the NPC in question is a normal NPC, otherwise, our special draw code takes over.

            ILCursor c = new ILCursor(il);

            ILLabel exitLabel = c.DefineLabel();

            //Get label of instruction we will be transforming to
            c.ErrorOnFailedGotoNext(i => i.MatchCall<PlayerInput>("get_IgnoreMouseInterface"));

            exitLabel = c.MarkLabel();

            c.Index = 0;

            //If the target instruction is found and we found the exit instruction, draw the villager if applicable
            c.ErrorOnFailedGotoNext(MoveType.After, i => i.MatchCall<Main>(nameof(Main.SetMouseNPC_ToHousingQuery)));

            c.Index += 3;

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
            c.Emit(OpCodes.Brtrue_S, exitLabel);
        }

        private void BannersVisibleWhileInVillagerHousingMenu(ILContext il) {
            //Edit rundown:
            //Very simple this time. We need to simply allow for the banners to be drawn while in the villager housing menu along with the normal
            // vanilla housing menu.

            ILCursor c = new ILCursor(il);

            //Navigate to EquipPage check
            c.ErrorOnFailedGotoNext(i => i.MatchLdsfld<Main>(nameof(Main.EquipPage)));

            //If we correctly navigated to the EquipPage, we need to get the label of the beq_s that is associated with it (see IL block below)
            if (c.Instrs[c.Index + 2].OpCode == OpCodes.Beq_S) {
                //Steal the label from the beq_s
                ILLabel stolenLabel = (ILLabel)c.Instrs[c.Index + 2].Operand;

                //Remove all the instructions in this IL block so we can re-add it and slip in our own check
                for (int i = 0; i < 3; i++) {
                    c.Remove();
                }

                //Re-add our check, making sure it's inverted compared to the IL, since in IL it determines if the code should run if these values are FALSE,
                // but since we took control of the instructions, we can test based on if it's true or not for easy understanding
                c.EmitDelegate<Func<bool>>(() => Main.EquipPage == 1 || ModContent.GetInstance<VillagerHousingUISystem>().correspondingUIState.isMenuVisible);

                c.Emit(OpCodes.Brtrue_S, stolenLabel);
            }
            else {
                //Throw error is the code does not match, since this IL edit is kind of a necessary functionality of the mod
                throw new PatchingUtils.InstructionNotFoundException();
            }

            //IL Block in question ^:
            // /* 0x00112F7D 7E2F030004   */ IL_0001: ldsfld    int32 Terraria.Main::EquipPage
            // /* 0x00112F82 17           */ IL_0006: ldc.i4.1
            // /* 0x00112F83 2E14         */ IL_0007: beq.s     IL_001D
        }

        private void DrawVillagerBannerInHouses(ILContext il) {
            //Edit rundown (this one is kinda long):
            //We need to have the villagers who are living in villager homes be displayed on their banners, like they are for normal town NPCs
            //We will do this by hijacking the if statement that tests if the NPC is a town NPC and also test whether or not they are a villager
            //If they are a villager, modify vanilla draw statements, and make sure to transfer the code to draw the villager in their entirety
            //Next, and lastly, make it so the player cannot modify the housing of the villagers if they are not well-liked enough

            ILCursor c = new ILCursor(il);

            //Navigate to first TownNPC check
            c.ErrorOnFailedGotoNext(i => i.MatchLdfld<NPC>(nameof(NPC.townNPC)));

            //Remove townNPC check in order to add our villager check
            c.Remove();
            //Add villager check
            c.EmitDelegate<Func<NPC, bool>>(npc => npc.ModNPC is Villager || npc.townNPC);

            //First IL block in question for the edit above ^:
            // /* 0x00101758 7ED8040004   */ IL_0020: ldsfld    class Terraria.NPC[] Terraria.Main::npc
            // /* 0x0010175D 06           */ IL_0025: ldloc.0
            // /* 0x0010175E 9A           */ IL_0026: ldelem.ref
            // /* 0x0010175F 7BE3070004   */ IL_0027: ldfld     bool Terraria.NPC::townNPC
            // /* 0x00101764 2C41         */ IL_002C: brfalse.s IL_006F

            //Navigate to second TownNPC check
            c.ErrorOnFailedGotoNext(i => i.MatchLdfld<NPC>(nameof(NPC.townNPC)));

            //Remove townNPC check in order to add our villager check (again)
            c.Remove();
            //In this context, the game is checking if the npc is NOT a villager/townNPC
            c.EmitDelegate<Func<NPC, bool>>(npc => npc.ModNPC is not Villager || !npc.townNPC);

            //Second IL block in question for the edit above ^:
            // /* 0x0010180E 1105         */ IL_00D6: ldloc.s   nPC
            // /* 0x00101810 7BE3070004   */ IL_00D8: ldfld     bool Terraria.NPC::townNPC
            // /* 0x00101815 2C2A         */ IL_00DD: brfalse.s IL_0109

            //Next, we will swap out the background banners for our own

            byte npcLocalNumber = 3;
            byte bannerAssetLocalNumber = 16;
            byte framingRectangleLocalNumber = 17;

            //Navigate to the asset of the background banner
            c.ErrorOnFailedGotoNext(i => i.MatchStloc(bannerAssetLocalNumber));

            //Replace the normal call with our own if the npc is a villager
            //Pop the normal texture asset off the stack
            c.Emit(OpCodes.Pop);
            //Load this NPC to stack
            c.Emit(OpCodes.Ldloc_S, npcLocalNumber);
            //If this NPC is a villager, use our own modded banners. If not, return the normal one
            c.EmitDelegate<Func<NPC, Texture2D>>(npc => npc.ModNPC is Villager ? ModContent.Request<Texture2D>(LivingWorldMod.LWMSpritePath + "UI/VillagerHousingUI/VillagerHousing_Banners").Value : TextureAssets.HouseBanner.Value);

            //Navigate to the banner framing rectangle
            c.ErrorOnFailedGotoNext(i => i.MatchStloc(framingRectangleLocalNumber));

            //In order for the drawing to be framed properly, we must take into account whether or not it's our modded banners or not
            //Pop the normal framing rectangle off the stack
            c.Emit(OpCodes.Pop);
            //Load this NPC to stack
            c.Emit(OpCodes.Ldloc_S, npcLocalNumber);
            //Load the current texture to the stack
            c.Emit(OpCodes.Ldloc_S, bannerAssetLocalNumber);
            //If this NPC is a villager, adjust the framing rectangle to use our modded proportions. If not, return the normal vanilla value
            c.EmitDelegate<Func<NPC, Texture2D, Rectangle>>((npc, texture) => npc.ModNPC is Villager ? texture.Frame(2, NPCUtils.GetTotalVillagerTypeCount()) : texture.Frame(2, 2));

            //IL block for the above two edits ^:
            /*/* (30124,5)-(30124,55) tModLoader\src\tModLoader\Terraria\Main.cs #1#
            /* 0x001019F0 7E59490004   #1# IL_02B8: ldsfld    class [ReLogic]ReLogic.Content.Asset`1<class [FNA]Microsoft.Xna.Framework.Graphics.Texture2D> Terraria.GameContent.TextureAssets::HouseBanner
            /* 0x001019F5 6F6F02000A   #1# IL_02BD: callvirt  instance !0 class [ReLogic]ReLogic.Content.Asset`1<class [FNA]Microsoft.Xna.Framework.Graphics.Texture2D>::get_Value()
            /* 0x001019FA 1312         #1# IL_02C2: stloc.s   'value'
            /* (30125,5)-(30125,66) tModLoader\src\tModLoader\Terraria\Main.cs #1#
            /* 0x001019FC 1112         #1# IL_02C4: ldloc.s   'value'
            /* 0x001019FE 18           #1# IL_02C6: ldc.i4.2
            /* 0x001019FF 18           #1# IL_02C7: ldc.i4.2
            /* 0x00101A00 16           #1# IL_02C8: ldc.i4.0
            /* 0x00101A01 16           #1# IL_02C9: ldc.i4.0
            /* 0x00101A02 16           #1# IL_02CA: ldc.i4.0
            /* 0x00101A03 16           #1# IL_02CB: ldc.i4.0
            /* 0x00101A04 28DF0C0006   #1# IL_02CC: call      valuetype [FNA]Microsoft.Xna.Framework.Rectangle Terraria.Utils::Frame(class [FNA]Microsoft.Xna.Framework.Graphics.Texture2D, int32, int32, int32, int32, int32, int32)
            /* 0x00101A09 1313         #1# IL_02D1: stloc.s   value2*/

            //Second to last, we must skip over the head drawing code if the NPC in question is a villager

            //Navigate to our exit instruction in order to skip over the head drawing
            ILLabel exitInstruction = c.DefineLabel();
            int preExitJumpIndex = c.Index;

            byte homeTileXLocalNumber = 6;
            byte homeTileYLocalNumber = 7;
            byte homeTileYInWorldLocalNumber = 14;
            byte npcProfileLocalNumber = 18;

            //Grab exit instruction & do custom drawing code
            c.ErrorOnFailedGotoNext(i => i.MatchStloc(homeTileXLocalNumber));

            //Move to instruction right AFTER the normal spritebatch call
            c.Index -= 14;
            //Load this NPC to stack
            c.Emit(OpCodes.Ldloc_S, npcLocalNumber);
            //Place exit instruction on the new instruction we just emitted ^ then return to old instruction
            c.Index--;
            exitInstruction = c.MarkLabel();
            c.Index++;
            //Load local variable homeTileX in tile coordinates
            c.Emit(OpCodes.Ldloc_S, homeTileXLocalNumber);
            //Load local variable homeTileY in tile coordinates
            c.Emit(OpCodes.Ldloc_S, homeTileYLocalNumber);
            //Load local variable homeTileY in world coordinates
            c.Emit(OpCodes.Ldloc_S, homeTileYInWorldLocalNumber);
            //Apply custom draw code
            c.EmitDelegate<Action<NPC, int, int, float>>((npc, homeTileX, homeTileY, homeTileYPixels) => {
                if (npc.ModNPC is Villager villager) {
                    float drawScale = 0.5f;

                    Texture2D bodyTexture = villager.bodyAssets[villager.bodySpriteType].Value;
                    Texture2D headTexture = villager.headAssets[villager.headSpriteType].Value;

                    Rectangle textureDrawRegion = new Rectangle(0, 0, bodyTexture.Width, bodyTexture.Height / Main.npcFrameCount[villager.Type]);
                    Vector2 drawPos = new Vector2(homeTileX * 16f - Main.screenPosition.X + 10f, homeTileYPixels - Main.screenPosition.Y + 14f);
                    Vector2 drawOrigin = new Vector2(textureDrawRegion.Width / 2f, textureDrawRegion.Height / 2f);

                    //Take into account possible gravity swapping
                    SpriteEffects spriteEffect = Main.LocalPlayer.gravDir != -1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

                    Main.spriteBatch.Draw(bodyTexture, drawPos, textureDrawRegion, Lighting.GetColor(homeTileX, homeTileY), 0f, drawOrigin, drawScale, spriteEffect, 0f);
                    Main.spriteBatch.Draw(headTexture, drawPos, textureDrawRegion, Lighting.GetColor(homeTileX, homeTileY), 0f, drawOrigin, drawScale, spriteEffect, 0f);
                }
            });

            c.Index = preExitJumpIndex;

            //Apply exit instruction transfer if the NPC is a villager
            c.ErrorOnFailedGotoNext(i => i.MatchLdloca(npcProfileLocalNumber));

            //Move to IL instruction that denotes the beginning of the line
            c.Index -= 2;
            //Load this NPC to stack
            c.Emit(OpCodes.Ldloc_S, npcLocalNumber);
            //Test for villager status
            c.EmitDelegate<Func<NPC, bool>>(npc => npc.ModNPC is Villager);
            c.Emit(OpCodes.Brtrue_S, exitInstruction);

            //I would put the IL block in question here, but it is a very small edit and the IL block is really big due to it including
            // a lot of math instructions and a spritebatch call

            //Finally, we are going to change the hover text and prevent the player from un-housing villagers if they are not well-liked enough
            byte bannerHoverTextLocalNumber = 26;

            //Navigate to banner hover text variable allocation
            c.ErrorOnFailedGotoNext(i => i.MatchStloc(bannerHoverTextLocalNumber));

            //Load this NPC to stack
            c.Emit(OpCodes.Ldloc_S, npcLocalNumber);
            //Add onto the normal text if villager and not well-liked enough
            c.EmitDelegate<Func<string, NPC, string>>((normalText, npc) =>
                normalText + (npc.ModNPC is Villager { RelationshipStatus: < VillagerRelationship.Like } villager ? $"\n{LocalizationUtils.GetLWMTextValue("UI.VillagerHousing.VillagerTypeLocked", villager.VillagerType.ToString())}" : "")
            );

            //IL block for above edit ^:
            /* (30154,6)-(30154,72) tModLoader\src\tModLoader\Terraria\Main.cs #1#
            /* 0x00101CD3 1105         #1# IL_059B: ldloc.s   nPC
            /* 0x00101CD5 1106         #1# IL_059D: ldloc.s   num2
            /* 0x00101CD7 2891020006   #1# IL_059F: call      string Terraria.Lang::GetNPCHouseBannerText(class Terraria.NPC, int32)
            /* 0x00101CDC 132A         #1# IL_05A4: stloc.s   nPCHouseBannerText
            /* (30155,6)-(30155,42) tModLoader\src\tModLoader\Terraria\Main.cs #1#
            /* 0x00101CDE 02           #1# IL_05A6: ldarg.0
            /* 0x00101CDF 112A         #1# IL_05A7: ldloc.s   nPCHouseBannerText
            /* 0x00101CE1 16           #1# IL_05A9: ldc.i4.0
            /* 0x00101CE2 16           #1# IL_05AA: ldc.i4.0
            /* 0x00101CE3 15           #1# IL_05AB: ldc.i4.m1
            /* 0x00101CE4 15           #1# IL_05AC: ldc.i4.m1
            /* 0x00101CE5 15           #1# IL_05AD: ldc.i4.m1
            /* 0x00101CE6 15           #1# IL_05AE: ldc.i4.m1
            /* 0x00101CE7 16           #1# IL_05AF: ldc.i4.0
            /* 0x00101CE8 28C3030006   #1# IL_05B0: call      instance void Terraria.Main::MouseText(string, int32, uint8, int32, int32, int32, int32, int32)
            /* 0x00101CED 00           #1# IL_05B5: nop
            */

            //Navigate to right-click call in main
            c.ErrorOnFailedGotoNext(i => i.MatchLdsfld<Main>(nameof(Main.mouseRight)));

            //Move past call and copy the brfalse_S label, then move after the transform command
            c.Index++;
            ILLabel falseTransformLabel = (ILLabel)c.Next.Operand;
            c.Index++;
            //Load this NPC to stack
            c.Emit(OpCodes.Ldloc_S, npcLocalNumber);
            //Remember that we moved after the right click loading instruction, so we have two things on the stack now
            c.EmitDelegate<Func<NPC, bool>>(npc => {
                //Check if the npc is a villager, then return based on whether or not the player is well-liked enough
                if (npc.ModNPC is Villager villager) {
                    return villager.RelationshipStatus >= VillagerRelationship.Like;
                }

                //If the npc is not a villager, then assume vanilla functionality
                return true;
            });
            //Finally, add that copied false label we took
            c.Emit(OpCodes.Brfalse_S, falseTransformLabel);
        }

        private void RoomOccupancyCheck(ILContext il) {
            //This edit is simple; we will be allowing villagers to properly check along-side normal TownNPCs for the occupancy check vanilla does.

            ILCursor c = new ILCursor(il);

            //Navigate to townNPC check
            c.ErrorOnFailedGotoNext(i => i.MatchLdfld<NPC>(nameof(NPC.townNPC)));

            //Remove the check, add our own delegate check
            c.Remove();
            c.EmitDelegate<Func<NPC, bool>>(npc => npc.townNPC || npc.ModNPC is Villager);
        }

        private void IgnoreRoomOccupancy(ILContext il) {
            //Another simple edit; this edit will allow us to get WorldGen.bestX and WorldGen.bestY for a room, ignoring if the room is occupied.

            ILCursor c = new ILCursor(il);

            //Navigate to occupancy check
            c.ErrorOnFailedGotoNext(MoveType.After, i => i.MatchCall<WorldGen>("ScoreRoom_IsThisRoomOccupiedBySomeone"));

            //Emit our check to see if our IgnoreOccupancy in HousingUtils.cs is true, and if so, automatically return false, ignoring occupancy
            c.EmitDelegate<Func<bool, bool>>((isOccupied) => !HousingUtils.IgnoreHouseOccupancy && isOccupied);
        }

        private void FindRoomBed(ILContext il) {
            //One of two patches that injects itself into the ScoreRoom method in order to find a corresponding bed for an NPC

            ILCursor c = new ILCursor(il);

            //Navigate to right before tile loop
            c.ErrorOnFailedGotoNext(i => i.MatchLdsfld<WorldGen>(nameof(WorldGen.roomY2)));
            c.Index += 2;
            //Load all loop local variables
            c.Emit(OpCodes.Ldloc_S, (byte)2);
            c.Emit(OpCodes.Ldloc_S, (byte)3);
            c.Emit(OpCodes.Ldloc_S, (byte)4);
            c.Emit(OpCodes.Ldloc_S, (byte)5);
            c.EmitDelegate<Action<int, int, int, int>>((startX, endX, startY, endY) => {
                //Same check vanilla does
                for (int i = startX + 1; i < endX; i++) {
                    for (int j = startY + 2; j < endY + 2; j++) {
                        Tile tile = Framing.GetTileSafely(i, j);
                        if (tile.HasTile && tile.TileType == TileID.Beds) {
                            HouseBedPosition = TileUtils.GetTopLeftOfMultiTile(tile, i, j, 4).ToPoint();
                            return;
                        }
                    }
                }
            });
        }

        private void AssignBedToNPC(ILContext il) {
            //Second to of two patches that takes the potential calculated bed position in an NPC's house, and assigns it to them

            ILCursor c = new ILCursor(il);

            //Navigate to right after homeless assignment
            c.ErrorOnFailedGotoNext(i => i.MatchCall<AchievementsHelper>(nameof(AchievementsHelper.NotifyProgressionEvent)));
            c.Index--;
            //Load NPC onto stack
            c.Emit(OpCodes.Ldsfld, typeof(Main).GetField(nameof(Main.npc), BindingFlags.Public | BindingFlags.Static));
            c.Emit(OpCodes.Ldloc_0);
            c.Emit(OpCodes.Ldelem_Ref);
            c.EmitDelegate<Action<NPC>>(npc => {
                if (HouseBedPosition != Point.Zero) {
                    npc.GetGlobalNPC<TownChangesNPC>().ownedBed = new BedData(new Point(HouseBedPosition.X, HouseBedPosition.Y));
                }

                HouseBedPosition = Point.Zero;
            });
        }
    }
}