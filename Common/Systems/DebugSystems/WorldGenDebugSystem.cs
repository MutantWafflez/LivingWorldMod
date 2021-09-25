#if DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.Tiles.Building;
using LivingWorldMod.Custom.Classes.WorldGen.GenConditions;
using LivingWorldMod.Custom.Structs;
using LivingWorldMod.Custom.Utilities;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Common.Systems.DebugSystems {

    /// <summary>
    /// Debug ModSystem used for testing world generation code.
    /// </summary>
    public class WorldGenDebugSystem : ModSystem {

        public override void PostUpdateEverything() {
            //Trigger the generation method by pressing 0 on the numpad
            if (Main.keyState.IsKeyDown(Keys.NumPad0) && !Main.oldKeyState.IsKeyDown(Keys.NumPad0)) {
                GenerationMethod((int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16);
            }
        }

        private void GenerationMethod(int x, int y) {
            Dust.QuickBox(new Vector2(x, y) * 16, new Vector2(x + 1, y + 1) * 16, 2, Color.YellowGreen, null);

            // Code to test placed here:
            float xScale = WorldGen.genRand.NextFloat(1.6f, 1.75f);
            float yScale = WorldGen.genRand.NextFloat(0.6f, 0.8f);
            int radius = 35;
            Point originPoint = new Point(x, y);

            ShapeData fullShapeData = new ShapeData();

            //Generate base-cloud structure
            WorldUtils.Gen(originPoint, new Shapes.Slime(radius, xScale, yScale), Actions.Chain(
                new Modifiers.Flip(false, true),
                new Actions.SetTile(TileID.Cloud, true),
                new Modifiers.Blotches(2, 1, 0.9f),
                new Actions.PlaceWall(WallID.Cloud),
                new Actions.SetFrames(),
                new Actions.Blank().Output(fullShapeData)
            ));

            //Generate dirt within cloud base
            WorldUtils.Gen(originPoint, new Shapes.Slime(radius, xScale * 0.75f, yScale * 0.75f), Actions.Chain(
                new Modifiers.Flip(false, true),
                new Modifiers.Offset(0, -(int)(radius * yScale * 0.215f)),
                new Modifiers.Blotches(2, 1, 0.9f),
                new Actions.SetTile(TileID.Dirt),
                new Actions.PlaceWall(WallID.Cloud),
                new Actions.SetFrames(),
                new Actions.Blank().Output(fullShapeData)
            ));

            //Flatten Surface
            WorldUtils.Gen(originPoint, new Shapes.Rectangle((int)(radius * xScale) * 2, 3), Actions.Chain(
                new Modifiers.Offset(fullShapeData.GetData().Min(point => point.X), fullShapeData.GetData().Min(point => point.Y)),
                new Actions.ClearTile(true),
                new Actions.Blank().Output(fullShapeData)
            ));

            //These two fields represent the left and upper offset that you must move from the origin of the island to reach the top left of it
            int leftOffset = fullShapeData.GetData().Min(point => point.X);
            int upOffset = fullShapeData.GetData().Min(point => point.Y);

            //Generate small pond/lake
            WorldUtils.Gen(originPoint, new Shapes.Slime(radius, xScale * 0.125f, yScale * 0.25f), Actions.Chain(
                new Modifiers.Flip(false, true),
                new Modifiers.Offset(0, -(int)(radius * yScale * 0.5f)),
                new Modifiers.Conditions(new Conditions.IsTile(TileID.Dirt)),
                new Actions.ClearTile(true),
                new Actions.ClearWall(true),
                new Actions.SetLiquid(),
                new Actions.Blank().Output(fullShapeData)
            ));

            WorldUtils.Gen(originPoint, new ModShapes.All(fullShapeData), Actions.Chain(
                //Remove walls on outer-most tiles
                new Actions.ContinueWrapper(Actions.Chain(
                    new Modifiers.IsTouchingAir(),
                    new Actions.RemoveWall()
                )),
                //Create Grass on dirt tiles
                new Actions.ContinueWrapper(Actions.Chain(
                    new Modifiers.Conditions(new Conditions.IsTile(TileID.Dirt)),
                    new Modifiers.IsTouchingAir(true),
                    new Actions.Custom((i, j, args) => {
                        WorldGen.SpreadGrass(i, j, repeat: false);
                        return true;
                    })
                ))
            ));

            //Place ground houses on main island
            List<string> possibleHouses = new List<string>() {
                "HighRise0", "HighRise1", "HighRise2",
                "GroundHouse0", "GroundHouse1", "GroundHouse2", "GroundHouse3", "GroundHouse4", "GroundHouse5", "GroundHouse6"
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

            //Change grass tiles below any buildings to dirt, and promptly smooth & frame the tiles properly
            WorldUtils.Gen(originPoint, new ModShapes.All(fullShapeData), Actions.Chain(
                new Actions.ContinueWrapper(Actions.Chain(
                    new Modifiers.Conditions(new Conditions.IsTile(TileID.Grass)),
                    new Modifiers.Offset(0, -1),
                    new Modifiers.IsSolid(),
                    new Modifiers.Conditions(new Conditions.IsTile(TileID.Cloud, TileID.Grass).Not()),
                    new Modifiers.Offset(0, 1),
                    new Actions.SetTile(TileID.Dirt, true)
                    )),
                new Actions.SetFrames(true),
                new Actions.Smooth(true)
            ));

            //"High Rise" houses are not allowed to generate on the mini islands, so they are removed before the mini islands are generated
            possibleHouses = possibleHouses.Where(house => !house.Contains("HighRise")).ToList();

            //Place smaller island clouds and houses on top of them
            for (int i = -1; i < 2; i += 2) {
                for (int j = 0; j < 2; j++) {
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
                        new Actions.ContinueWrapper(Actions.Chain(
                            new Modifiers.Dither(),
                            new Actions.Smooth()
                        )),
                        new Actions.ContinueWrapper(Actions.Chain(
                            new Modifiers.IsTouchingAir(),
                            new Modifiers.Dither(0.9),
                            new Modifiers.Blotches(3, 1f),
                            new Modifiers.Conditions(new Conditions.IsSolid()),
                            new Actions.SetTile(TileID.RainCloud, true)
                        ))
                    ));

                    //Generate building
                    WorldGenUtilities.GenerateStructure(cloudHouseData,
                        miniIslandOrigin.X - (cloudHouseData.structureWidth / 2),
                        miniIslandOrigin.Y + miniIslandData.GetData().Min(point => point.Y) - cloudHouseData.structureHeight);
                }
            }

            //Place "church" building
            StructureData churchBuildingData = IOUtilities.GetStructureFromFile(LivingWorldMod.LWMStructurePath + $"/Villages/Harpy/ChurchBuilding{WorldGen.genRand.Next(2)}.struct");

            if (WorldUtils.Find(new Point(originPoint.X - (churchBuildingData.structureWidth / 2), originPoint.Y + upOffset), Searches.Chain(new Searches.Up(75), new IsAir().AreaAnd(churchBuildingData.structureWidth, churchBuildingData.structureHeight)), out Point churchResult)) {
                WorldGenUtilities.GenerateStructure(churchBuildingData, churchResult.X, churchResult.Y);
            }

            //Place starshard cloud blotches
            WorldUtils.Gen(originPoint, new ModShapes.All(fullShapeData), Actions.Chain(
                new Modifiers.Offset(-2, -2),
                new Modifiers.Conditions(new Conditions.IsTile(TileID.Cloud).AreaAnd(5, 5)),
                new Modifiers.Offset(2, 2),
                new Modifiers.Blotches(chance: 0.125f),
                new Actions.SetTileKeepWall((ushort)ModContent.TileType<StarshardCloudTile>(), true)
            ));
        }
    }
}

#endif