using System;
using System.Linq;
using LivingWorldMod.Common.Systems;
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
        }

        public void Unload() { }

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