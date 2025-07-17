using System;
using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.Villages.DataStructures.Enums;
using LivingWorldMod.Content.Villages.Globals.BaseTypes.NPCs;
using LivingWorldMod.Content.Villages.Globals.Systems.UI;
using LivingWorldMod.Content.Villages.HarpyVillage.Tiles.Furniture;
using LivingWorldMod.DataStructures.Classes;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.Localization;

namespace LivingWorldMod.Globals.Patches;

/// <summary>
///     Class that contains IL/On patches for NPC housing-related manners.
/// </summary>
public class NPCHousingPatches : LoadablePatch {
    public override void LoadPatches() {
        IL_WorldGen.CheckSpecialTownNPCSpawningConditions += TestForVillageHouse;

        IL_Main.DrawInterface_38_MouseCarriedObject += DrawSelectedVillagerOnMouse;

        IL_Main.DrawInterface_7_TownNPCHouseBanners += BannersVisibleWhileInVillagerHousingMenu;

        IL_Main.DrawNPCHousesInWorld += DrawVillagerBannerInHouses;

        IL_WorldGen.ScoreRoom_IsThisRoomOccupiedBySomeone += RoomOccupancyCheck;

        IL_WorldGen.ScoreRoom += IgnoreRoomOccupancy;

        IL_WorldGen.MoveTownNPC += SpecialMoveNPCErrorMessageEdit;
    }

    private void TestForVillageHouse(ILContext il) {
        currentContext = il;

        ILCursor c = new(il);

        //We do not want non-villagers spawning in villager homes, which is what this patch is for
        //In this method, a return of false will mean that the specific NPC cannot spawn in this house, and true means the opposite
        //We check to see if this NPC already CANNOT spawn in said house for whatever reason at the beginning, and that acts as normal if true
        //If the NPC CAN spawn here by normal means, we check to see if the room is within a village and if the NPC is a type of villager, and if both are true, prevent the NPC from taking that house
        c.GotoNext(i => i.MatchCall(typeof(NPCLoader), nameof(NPCLoader.CheckConditions)));

        ILLabel doVanillaChecksIfFalseLabel = null;
        c.GotoNext(i => i.MatchBrtrue(out doVanillaChecksIfFalseLabel));

        c.Index = 0;
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<int, bool>>(npcType => {
                LWMUtils.VillagerHousingErrorKey = null;

                ModNPC modNPC = ModContent.GetModNPC(npcType);
                List<VillageShrineEntity> shrines = LWMUtils.GetAllEntityOfType<VillageShrineEntity>().ToList();

                //HOWEVER, if the Town NPC can spawn here, we need to do additional checks to make sure it's not a non-villager spawning in a villager home
                //Additionally, we can't have villagers in a non-village home, which is the second check.
                bool anyVillageContainsHome = shrines.Any(shrine => shrine.villageZone.ToTileCoordinates().ContainsPoint(new Vector2(WorldGen.bestX, WorldGen.bestY)));
                bool isVillagerAndNotInVillage = modNPC is Villager && !anyVillageContainsHome;
                bool isNotVillagerAndInVillage = modNPC is not Villager && anyVillageContainsHome;

                if (isVillagerAndNotInVillage) {
                    LWMUtils.VillagerHousingErrorKey = "UI.VillagerHousing.VillagerHousedOutsideOfVillageError".PrependModKey();
                    return true;
                }

                if (isNotVillagerAndInVillage) {
                    LWMUtils.VillagerHousingErrorKey = "UI.VillagerHousing.NonVillagerHousedInsideOfVillageError".PrependModKey();
                    return true;
                }

                return false;
            }
        );
        c.Emit(OpCodes.Brfalse_S, doVanillaChecksIfFalseLabel);

        c.Emit(OpCodes.Ldc_I4_0);
        c.Emit(OpCodes.Ret);
    }

    private void DrawSelectedVillagerOnMouse(ILContext il) {
        currentContext = il;

        //Run-down of this edit:
        //We want to draw the selected villager near the mouse, similar to how vanilla draws NPC heads on the mouse when the player is changing around houses
        //So, we want to avoid the head being drawn since there is no head sprite for villagers; so we will get an "exit" instruction that is after the head
        // drawing code using a label. The head will draw as normal if it the NPC in question is a normal NPC, otherwise, our special draw code takes over.

        ILCursor c = new(il);

        ILLabel exitLabel = c.DefineLabel();

        //Get label of instruction we will be transforming to
        c.GotoNext(i => i.MatchCall<PlayerInput>("get_IgnoreMouseInterface"));
        exitLabel = c.MarkLabel();

        //If the target instruction is found and we found the exit instruction, draw the villager if applicable
        c.Index = 0;
        c.GotoNext(i => i.MatchCall<Main>(nameof(Main.SetMouseNPC_ToHousingQuery)));
        c.GotoNext(i => i.MatchLdfld<Main>(nameof(Main.mouseNPCIndex)));
        c.GotoPrev(i => i.MatchLdarg0());

        //What we return here will determine whether or not we skip past the drawing head step in the vanilla function.
        c.EmitDelegate(() => {
                //If not a type of villager or otherwise an invalid index (or just the above if statement failing in general), then return false and have the head draw as normal.
                if (Main.instance.mouseNPCIndex <= -1 || Main.npc[Main.instance.mouseNPCIndex].ModNPC is not Villager villager) {
                    return false;
                }

                LayeredDrawObject drawObject = villager.drawObject;
                Rectangle textureDrawRegion = new(0, 0, drawObject.GetLayerFrameWidth(), drawObject.GetLayerFrameHeight(frameCount: Main.npcFrameCount[villager.Type]));
                drawObject.Draw(
                    Main.spriteBatch,
                    new DrawData(
                        null,
                        new Vector2(Main.mouseX, Main.mouseY),
                        textureDrawRegion,
                        Color.White,
                        0f,
                        Vector2.Zero,
                        Main.cursorScale * 0.67f,
                        SpriteEffects.None
                    ),
                    villager.DrawIndices
                );

                //If a type of villager, we do not want the head drawing function, so skip it by returning true
                return true;
            }
        );
        //Actual instruction that causes the "skipping." This instruction is why the exit label is necessary, since without it, the IL literally won't function and the head will draw.
        c.Emit(OpCodes.Brtrue_S, exitLabel);
    }

    private void BannersVisibleWhileInVillagerHousingMenu(ILContext il) {
        currentContext = il;

        //Edit rundown:
        //Very simple this time. We need to simply allow for the banners to be drawn while in the villager housing menu along with the normal
        // vanilla housing menu.

        ILCursor c = new(il);

        c.GotoNext(i => i.MatchLdsfld<Main>(nameof(Main.EquipPage)));

        ILLabel showBannersIfTrue = null;
        c.GotoNext(i => i.MatchBeq(out showBannersIfTrue));

        c.Index = 0;
        c.EmitDelegate(() => ModContent.GetInstance<VillagerHousingUISystem>().correspondingUIState.villagerHousingZone.IsVisible);
        c.Emit(OpCodes.Brtrue_S, showBannersIfTrue);
    }

    private void DrawVillagerBannerInHouses(ILContext il) {
        currentContext = il;

        // Due to the complexity of that this edit would otherwise require, we simply re-create this method for villager drawing instead of modifying
        // the method to work with villagers. Lose out on some code re-use, but pays off in the long term for the fragility of the edit

        ILCursor c = new(il);
        c.EmitDelegate(() => {
                foreach (NPC npc in LWMUtils.GetAllNPCs(npc => npc.ModNPC is Villager && !npc.homeless && npc.homeTileX > 0 && npc.homeTileY > 0)) {
                    int bannerTileX = npc.homeTileX;
                    int bannerTileY = npc.homeTileY - 1;

                    const int worldFluff = 10;
                    Tile tile = Main.tile[bannerTileX, bannerTileY];
                    while (!tile.HasTile || !(Main.tileSolid[tile.TileType] || TileID.Sets.Platforms[tile.TileType])) {
                        tile = Main.tile[bannerTileX, --bannerTileY];
                        if (bannerTileY < worldFluff) {
                            break;
                        }
                    }

                    const int bannerXPixelOffset = 8;
                    int bannerYPixelOffset = 18;
                    if (TileID.Sets.Platforms[tile.TileType]) {
                        bannerYPixelOffset -= 8;
                    }

                    int gravityYPixelCorrection = 0;
                    float bannerWorldY = bannerTileY * 16;
                    bannerWorldY += bannerYPixelOffset;

                    SpriteEffects spriteEffects = SpriteEffects.None;
                    Texture2D bannerTexture = ModContent.Request<Texture2D>(LWM.SpritePath + "Villages/UI/VillagerHousingUI/VillagerHousing_Banner").Value;
                    Rectangle bannerFrame = bannerTexture.Frame();
                    if (Main.LocalPlayer.gravDir == -1f) {
                        bannerWorldY -= Main.screenPosition.Y;
                        bannerWorldY = Main.screenPosition.Y + Main.screenHeight - bannerWorldY;
                        bannerWorldY -= bannerFrame.Height;
                        spriteEffects = SpriteEffects.FlipVertically;
                        gravityYPixelCorrection = 4;
                    }

                    Main.spriteBatch.Draw(
                        bannerTexture,
                        new Vector2(bannerTileX * 16 - (int)Main.screenPosition.X + bannerXPixelOffset, bannerWorldY - (int)Main.screenPosition.Y + bannerYPixelOffset + gravityYPixelCorrection),
                        bannerFrame,
                        Lighting.GetColor(bannerTileX, bannerTileY),
                        0f,
                        new Vector2(bannerFrame.Width / 2f, bannerFrame.Height / 2f),
                        1f,
                        spriteEffects,
                        0f
                    );

                    Villager villager = npc.ModNPC as Villager;
                    LayeredDrawObject drawObject = villager!.drawObject;
                    Rectangle textureDrawRegion = new(0, 0, drawObject.GetLayerFrameWidth(), drawObject.GetLayerFrameHeight(frameCount: Main.npcFrameCount[npc.type]));
                    drawObject.Draw(
                        Main.spriteBatch,
                        new DrawData(
                            null,
                            new Vector2(bannerTileX * 16 - (int)Main.screenPosition.X + bannerXPixelOffset, bannerWorldY - (int)Main.screenPosition.Y + bannerYPixelOffset + 2f),
                            textureDrawRegion,
                            Lighting.GetColor(bannerTileX, bannerTileY),
                            0f,
                            new Vector2(textureDrawRegion.Width / 2f, textureDrawRegion.Height / 2f),
                            0.5f,
                            spriteEffects
                        ),
                        villager.DrawIndices
                    );


                    bannerTileX = bannerTileX * 16 - (int)Main.screenPosition.X + bannerXPixelOffset - bannerFrame.Width / 2;
                    bannerTileY = (int)bannerWorldY - (int)Main.screenPosition.Y + 4;

                    if (Main.mouseX < bannerTileX || Main.mouseX > bannerTileX + bannerFrame.Width || Main.mouseY < bannerTileY || Main.mouseY > bannerTileY + bannerFrame.Height - 8) {
                        continue;
                    }

                    bool isLikedWithVillagers = villager.RelationshipStatus >= VillagerRelationship.Like;
                    string villagerLockText = $"\n{"UI.VillagerHousing.VillagerTypeLocked".Localized().FormatWith(villager.VillagerType.ToString())}";
                    Main.instance.MouseText(
                        Lang.GetNPCHouseBannerText(npc, 0)
                        + (!isLikedWithVillagers ? villagerLockText : "")
                    );

                    if (!Main.mouseRightRelease || !Main.mouseRight) {
                        continue;
                    }

                    Main.mouseRightRelease = false;
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    if (!isLikedWithVillagers) {
                        Main.NewText(villagerLockText);
                        return;
                    }

                    WorldGen.kickOut(npc.whoAmI);
                }
            }
        );
    }

    private void RoomOccupancyCheck(ILContext il) {
        currentContext = il;

        //This edit is simple; we will be allowing villagers to properly check along-side normal TownNPCs for the occupancy check vanilla does.

        ILCursor c = new(il);

        //Navigate to townNPC check
        c.ErrorOnFailedGotoNext(i => i.MatchLdfld<NPC>(nameof(NPC.townNPC)));

        //Remove the check, add our own delegate check
        c.Remove();
        c.EmitDelegate<Func<NPC, bool>>(npc => npc.townNPC || npc.ModNPC is Villager);
    }

    private void IgnoreRoomOccupancy(ILContext il) {
        currentContext = il;

        //Another simple edit; this edit will allow us to get WorldGen.bestX and WorldGen.bestY for a room, ignoring if the room is occupied.

        ILCursor c = new(il);

        //Navigate to occupancy check
        c.ErrorOnFailedGotoNext(MoveType.After, i => i.MatchCall<WorldGen>("ScoreRoom_IsThisRoomOccupiedBySomeone"));

        //Emit our check to see if our IgnoreOccupancy in HousingUtils.cs is true, and if so, automatically return false, ignoring occupancy
        c.EmitDelegate<Func<bool, bool>>(isOccupied => !LWMUtils.IgnoreHouseOccupancy && isOccupied);
    }

    private void SpecialMoveNPCErrorMessageEdit(ILContext il) {
        currentContext = il;

        ILCursor c = new(il);

        c.ErrorOnFailedGotoNext(i => i.MatchLdstr(" "));
        c.ErrorOnFailedGotoPrev(i => i.MatchLdsfld<Lang>(nameof(Lang.inter)));
        c.EmitDelegate(() => {
                if (LWMUtils.VillagerHousingErrorKey is not { } errorKey) {
                    return true;
                }

                Main.NewText(Language.GetText(errorKey), LWMUtils.YellowErrorTextColor);
                return false;
            }
        );

        // If the above delegate ^ return true, jump over our early return, and run vanilla code as usual
        c.Emit(OpCodes.Brtrue_S, c.DefineLabel());
        Instruction branchInstr = c.Prev;

        c.Emit(OpCodes.Ldc_I4_0);
        c.Emit(OpCodes.Ret);
        branchInstr.Operand = c.MarkLabel();
    }
}