using LivingWorldMod.Content.Tiles.Building;
using LivingWorldMod.Custom.Classes.WorldGen.GenConditions;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Structs;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.NPCs.Villagers;
using LivingWorldMod.Content.TileEntities.VillageShrines;
using Terraria.DataStructures;
using LivingWorldMod.Content.Tiles.Interactables.VillageShrines;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Common.Systems {

    /// <summary>
    /// System that handles the INITIAL world generation steps. This system does NOT handle world
    /// events that occur AFTER the world is created.
    /// </summary>
    public class WorldCreationSystem : ModSystem {

        /// <summary>
        /// List of all the areas which are the "zones" belonging to each village. Different for
        /// each world.
        /// </summary>
        public Rectangle[] villageZones;

        /// <summary>
        /// List of actions that will be run after world gen is completed per world.
        /// </summary>
        private List<Action> postWorldGenActions;

        public override void Load() {
            postWorldGenActions = new List<Action>();
            villageZones = new Rectangle[(int)VillagerType.TypeCount];
        }

        public override void Unload() {
        }

        public override void SaveWorldData(TagCompound tag) {
            tag["VillageZones"] = villageZones.ToList();
        }

        public override void LoadWorldData(TagCompound tag) {
            villageZones = tag.Get<List<Rectangle>>("VillageZones").ToArray();
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight) {
            //Harpy Villages
            int microBiomeIndex = tasks.FindIndex(pass => pass.Name.Equals("Micro Biomes"));
            if (microBiomeIndex != -1) {
                tasks.Insert(microBiomeIndex + 1, new PassLegacy("Harpy Village", GenerateHarpyVillage));
            }
        }

        public override void PostWorldGen() {
            //Invoke all post-world-gen actions then reset the list
            foreach (Action action in postWorldGenActions) {
                action.Invoke();
            }

            postWorldGenActions = new List<Action>();
        }

        private void GenerateHarpyVillage(GenerationProgress progress, GameConfiguration gameConfig) {
            progress.Message = "Generating Structures... Harpy Village";
            progress.Set(0f);

            WorldSize currentWorldSize = WorldGenUtilities.CurrentWorldSize;

            //Used to define the rectangle to search and to displace the origin to the actual correct position when generating the village so it doesn't generate on the top left of the rectangle
            int originHorizontalDisplacement = 175;
            int originVerticalDisplacement = currentWorldSize != WorldSize.Small ? 175 : 123;

            int yLevel = currentWorldSize switch {
                WorldSize.Small => 45,
                WorldSize.Medium => 134,
                WorldSize.Large => 200,
                _ => 100
            };

            List<Point> possibleIslandPlacements = new List<Point>();

            for (int i = (Main.maxTilesX / 2) - 1000; i < (Main.maxTilesX / 2) + 1000; i++) {
                progress.Set((float)(i - ((Main.maxTilesX / 2) - 1000)) / (float)((Main.maxTilesX / 2) + 1000));

                if (WorldUtils.Find(new Point(i, yLevel), Searches.Chain(
                        new Searches.Down(5),
                        new IsAir().AreaAnd(originHorizontalDisplacement, originVerticalDisplacement)
                        ),
                    out Point result)) {
                    possibleIslandPlacements.Add(result);
                }
            }

            Point originPoint;
            if (possibleIslandPlacements.Any()) {
                //Get point closest to middle of the world: order the list by distance to the center of the world (ascending), then grab the first element in said list
                originPoint = possibleIslandPlacements.OrderBy(point => Math.Abs(point.X - (Main.maxTilesX / 2))).First();

                //Set Harpy Village Zone
                villageZones[(int)VillagerType.Harpy] = new Rectangle(originPoint.X, originPoint.Y, originHorizontalDisplacement, originVerticalDisplacement);

                //Adjust origin point to be placed correctly within the village "zone" and actually an origin rather than the top corner
                originPoint.X += (int)(originHorizontalDisplacement * (0.88f / 1.75f));
                originPoint.Y += (int)(originVerticalDisplacement * (1.34f / 1.75f));
            }
            else {
                //If there is no point found (ever) the structure will not generate and the logger will throw a warning
                ModContent.GetInstance<LivingWorldMod>().Logger.Warn("Harpy Village unable to generate due to no suitable placement.");
                return;
            }

            float xScale = WorldGen.genRand.NextFloat(1.6f, 1.75f);
            float yScale = currentWorldSize != WorldSize.Small ? 0.65f : 0.45f; //Village is overall much smaller in smaller worlds
            int radius = 35;

            ShapeData mainIslandData = new ShapeData();

            //Generate base-cloud structure
            WorldUtils.Gen(originPoint, new Shapes.Slime(radius, xScale, yScale), Actions.Chain(
                new Modifiers.Flip(false, true),
                new Actions.SetTile(TileID.Cloud, true),
                new Actions.PlaceWall(WallID.Cloud),
                new Actions.SetFrames(),
                new Actions.Blank().Output(mainIslandData)
            ));

            //Generate dirt within cloud base
            WorldUtils.Gen(originPoint, new Shapes.Slime(radius, xScale * 0.75f, yScale * 0.75f), Actions.Chain(
                new Modifiers.Flip(false, true),
                new Modifiers.Offset(0, -(int)(radius * yScale * 0.215f)),
                new Modifiers.Blotches(2, 1, 0.9f),
                new Actions.SetTile(TileID.Dirt),
                new Actions.PlaceWall(WallID.Cloud),
                new Actions.SetFrames(),
                new Actions.Blank().Output(mainIslandData)
            ));

            //Flatten Surface
            WorldUtils.Gen(originPoint, new Shapes.Rectangle((int)(radius * xScale) * 2, 3), Actions.Chain(
                new Modifiers.Offset(mainIslandData.GetData().Min(point => point.X), mainIslandData.GetData().Min(point => point.Y)),
                new Actions.ClearTile(true),
                new Actions.Blank().Output(mainIslandData)
            ));

            //These two fields represent the left and upper offset that you must move from the origin of the island to reach the top left of it
            int leftOffset = mainIslandData.GetData().Min(point => point.X);
            int upOffset = mainIslandData.GetData().Min(point => point.Y);

            //Generate small pond/lake
            WorldUtils.Gen(originPoint, new Shapes.Slime(radius, xScale * 0.125f, yScale * 0.25f), Actions.Chain(
                new Modifiers.Flip(false, true),
                new Modifiers.Offset(0, -(int)(radius * yScale * (currentWorldSize != WorldSize.Small ? 0.465f : 0.4f))),
                new Modifiers.Conditions(new Conditions.IsTile(TileID.Dirt)),
                new Actions.ClearTile(true),
                new Actions.ClearWall(true),
                new Actions.SetLiquid(),
                new Actions.Blank().Output(mainIslandData)
            ));

            WorldUtils.Gen(originPoint, new ModShapes.All(mainIslandData), Actions.Chain(
                //Remove walls on outer-most tiles
                new Actions.ContinueWrapper(Actions.Chain(
                    new Modifiers.IsTouchingAir(),
                    new Actions.RemoveWall(),
                    new Actions.Blank().Output(mainIslandData)
                )),
                //Create Grass on dirt tiles
                new Actions.ContinueWrapper(Actions.Chain(
                    new Modifiers.Conditions(new Conditions.IsTile(TileID.Dirt)),
                    new Modifiers.IsTouchingAir(true),
                    new Actions.Custom((i, j, args) => {
                        WorldGen.SpreadGrass(i, j, repeat: false);
                        return true;
                    }),
                    new Actions.Blank().Output(mainIslandData)
                ))
            ));

            //Place stone blotches in main island dirt
            WorldUtils.Gen(originPoint, new ModShapes.All(mainIslandData), Actions.Chain(
                new Modifiers.Dither(0.967),
                new Modifiers.Conditions(new Conditions.IsTile(TileID.Dirt)),
                new Actions.Custom((i, j, args) => {
                    WorldGen.TileRunner(i, j, WorldGen.genRand.Next(2, 5), 5, TileID.Stone, ignoreTileType: TileID.Grass);
                    return true;
                }),
                new Actions.Blank().Output(mainIslandData)
            ));

            //Place starshard cloud blotches on main island
            WorldUtils.Gen(originPoint, new ModShapes.All(mainIslandData), Actions.Chain(
                new Modifiers.Offset(-2, -2),
                new Modifiers.Conditions(new Conditions.IsTile(TileID.Cloud).AreaAnd(5, 5)),
                new Modifiers.Offset(2, 2),
                new Modifiers.Blotches(chance: 0.125f),
                new Actions.SetTileKeepWall((ushort)ModContent.TileType<StarshardCloudTile>(), true)
            ));

            //Place ground houses on main island
            List<string> possibleHouses = new List<string>() {
                "HighRise0", "HighRise1", "HighRise2",
                "GroundHouse0", "GroundHouse1", "GroundHouse2", "GroundHouse3", "GroundHouse4", "GroundHouse5"
            };

            for (int i = 0; i < 2; i++) {
                List<Point> possiblePlacementPoints = new List<Point>();

                string selectedHouseType = WorldGen.genRand.Next(possibleHouses);

                possibleHouses.Remove(selectedHouseType);

                StructureData groundHouseData = IOUtilities.GetStructureFromFile(LivingWorldMod.LWMStructurePath + $"/Villages/Harpy/{selectedHouseType}.struct");

                for (int xOffset = leftOffset * (1 - i); xOffset <= 0 + i * Math.Abs(leftOffset); xOffset++) {
                    if (WorldUtils.Find(new Point(originPoint.X + xOffset, originPoint.Y), Searches.Chain(new Searches.Up(25), new Conditions.IsTile(TileID.Grass).AreaAnd(groundHouseData.structureWidth, 1)), out Point groundHouseResult)) {
                        //Populates a list of possible placement points where the house can be placed
                        possiblePlacementPoints.Add(groundHouseResult);
                    }
                }

                //If there is anywhere possible for the house to be placed, it takes the middle element in order to center the house as much as possible
                if (possiblePlacementPoints.Any()) {
                    Point middlePlacement = possiblePlacementPoints.ElementAt(possiblePlacementPoints.Count / 2);

                    WorldGenUtilities.GenerateStructure(groundHouseData, middlePlacement.X, middlePlacement.Y - groundHouseData.structureHeight);
                }
            }

            //Change grass tiles below any buildings to dirt, and promptly frame the tiles and smooth 50% of them
            WorldUtils.Gen(originPoint, new ModShapes.All(mainIslandData), Actions.Chain(
                new Actions.ContinueWrapper(Actions.Chain(
                    new Modifiers.Conditions(new Conditions.IsTile(TileID.Grass)),
                    new Modifiers.Offset(0, -1),
                    new Modifiers.IsSolid(),
                    new Modifiers.Conditions(new Conditions.IsTile(TileID.Cloud, TileID.Grass).Not()),
                    new Modifiers.Offset(0, 1),
                    new Actions.SetTile(TileID.Dirt, true)
                    )),
                new Actions.SetFrames(true),
                new Modifiers.Dither(0.4f),
                new Actions.Smooth()
            ));

            //Place "church" building
            StructureData churchBuildingData = IOUtilities.GetStructureFromFile(LivingWorldMod.LWMStructurePath + $"/Villages/Harpy/ChurchBuilding{WorldGen.genRand.Next(2)}.struct");

            if (WorldUtils.Find(new Point(originPoint.X - (churchBuildingData.structureWidth / 2), originPoint.Y + upOffset), Searches.Chain(new Searches.Up(75), new IsAir().AreaAnd(churchBuildingData.structureWidth, churchBuildingData.structureHeight)), out Point churchResult)) {
                WorldGenUtilities.GenerateStructure(churchBuildingData, churchResult.X, churchResult.Y);
            }

            //"High Rise" houses are not allowed to generate on the mini islands, so they are removed before the mini islands are generated
            possibleHouses = possibleHouses.Where(house => !house.Contains("HighRise")).ToList();

            //Place smaller island clouds and houses on top of them, for a total of 4 (in Medium+ worlds)
            for (int i = -1; i < 2; i += 2) {
                for (int j = 0; j < (currentWorldSize != WorldSize.Small ? 2 : 1); j++) { //Only worlds that are Medium size or bigger will generate the top islands
                    string selectedHouseType = WorldGen.genRand.Next(possibleHouses);
                    possibleHouses.Remove(selectedHouseType);

                    StructureData cloudHouseData = IOUtilities.GetStructureFromFile(LivingWorldMod.LWMStructurePath + $"/Villages/Harpy/{selectedHouseType}.struct");
                    Point miniIslandOrigin = new Point(originPoint.X + (int)(leftOffset * i * (j == 0 ? 1.15f : 0.775f)), originPoint.Y + (upOffset * (int)(j == 0 ? 2f : 6.25f)));

                    float miniYScale = 0.34f;
                    //Diameter will be the radius of the struct plus 5 extra "padding"
                    int miniRadius = (cloudHouseData.structureWidth / 2) + 5;

                    ShapeData miniIslandData = new ShapeData();

                    //Generate base cloud
                    WorldUtils.Gen(miniIslandOrigin, new Shapes.Slime(miniRadius, 1f, miniYScale), Actions.Chain(
                        new Modifiers.Flip(false, true),
                        new Modifiers.Blotches(2, 1, 2, 3, 0.125f),
                        new Actions.PlaceTile(TileID.Cloud),
                        new Actions.Blank().Output(miniIslandData)
                    ));

                    WorldUtils.Gen(miniIslandOrigin, new ModShapes.All(miniIslandData), Actions.Chain(
                        //Smooth 50% of applicable blocks
                        new Actions.ContinueWrapper(Actions.Chain(
                            new Modifiers.Dither(),
                            new Actions.Smooth()
                        )),
                        //Place rain cloud blotches in clouds touching air
                        new Actions.ContinueWrapper(Actions.Chain(
                            new Modifiers.IsTouchingAir(),
                            new Modifiers.Dither(0.9),
                            new Modifiers.Blotches(3, 1f),
                            new Modifiers.Conditions(new Conditions.IsSolid()),
                            new Actions.SetTile(TileID.RainCloud, true)
                        )),
                        //Place starshard cloud tiles throughout cloud
                        new Actions.ContinueWrapper(Actions.Chain(
                            new Modifiers.Dither(0.975),
                            new Modifiers.Conditions(new Conditions.IsSolid()),
                            new Actions.SetTile((ushort)ModContent.TileType<StarshardCloudTile>(), true)
                        ))
                    ));

                    //Generate building
                    WorldGenUtilities.GenerateStructure(cloudHouseData,
                        miniIslandOrigin.X - (cloudHouseData.structureWidth / 2),
                        miniIslandOrigin.Y + miniIslandData.GetData().Min(point => point.Y) - cloudHouseData.structureHeight);
                }
            }

            //Adds method to spawn shrine tile entity (To be added later, when tile entity saving is fixed)
            /*postWorldGenActions.Add(() => {
                if (villageZones[(int)VillagerType.Harpy] is Rectangle rectangle) {
                    for (int i = 0; i < rectangle.Width; i++) {
                        for (int j = 0; j < rectangle.Height; j++) {
                            Point currentPos = new Point(rectangle.X + i, rectangle.Y + j);

                            Tile currentTile = Framing.GetTileSafely(currentPos);

                            if (currentTile.type != ModContent.TileType<HarpyShrineTile>()) {
                                continue;
                            }

                            VillageShrineEntity entity = ((HarpyShrineTile)ModContent.GetModTile(ModContent.TileType<HarpyShrineTile>())).ShrineEntity;

                            Point16 topLeft = TileUtilities.GetTopLeftOfMultiTile(currentTile, currentPos.X, currentPos.Y);

                            entity.Place(topLeft.X, topLeft.Y);
                        }
                    }
                }
            });*/

            //Add method to spawn Harpies themselves (Also to be added later when NPC Save & Load PR gets merged)
            /*postWorldGenActions.Add(() => {
                if (villageZones[(int)VillagerType.Harpy] is Rectangle rectangle) {
                    for (int i = 0; i < rectangle.Width; i++) {
                        for (int j = 0; j < rectangle.Height; j++) {
                            Point position = new Point(rectangle.X + i, rectangle.Y + j);
                            int harpyType = ModContent.NPCType<HarpyVillager>();

                            if (WorldGen.StartRoomCheck(position.X, position.Y) && WorldGen.RoomNeeds(harpyType)) {
                                WorldGen.ScoreRoom(npcTypeAskingToScoreRoom: harpyType);

                                if (Main.npc.Any(npc => npc.homeTileX == WorldGen.bestX && npc.homeTileY == WorldGen.bestY)) {
                                    continue;
                                }

                                int npc = NPC.NewNPC(WorldGen.bestX * 16, WorldGen.bestY * 16, harpyType);

                                Main.npc[npc].homeTileX = WorldGen.bestX;
                                Main.npc[npc].homeTileY = WorldGen.bestY;
                            }
                        }
                    }
                }
            });*/
        }
    }
}