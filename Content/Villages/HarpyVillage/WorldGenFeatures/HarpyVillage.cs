using System;
using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.Villages.HarpyVillage.NPCs;
using LivingWorldMod.Content.Villages.HarpyVillage.Tiles.Blocks;
using LivingWorldMod.Content.Villages.HarpyVillage.Tiles.Furniture;
using LivingWorldMod.DataStructures.Classes.GenConditions;
using LivingWorldMod.DataStructures.Enums;
using LivingWorldMod.DataStructures.Structs;
using LivingWorldMod.Globals.ModTypes;
using LivingWorldMod.Globals.Systems;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Content.Villages.HarpyVillage.WorldGenFeatures;

/// <summary>
/// The Harpy Village structure, generated in the sky as close to the middle of the world as possible.
/// </summary>
public class HarpyVillage : WorldGenFeature {
    /// <summary>
    /// The name given to the temporary variable created during world generation which holds
    /// the rectangle in which the original Harpy Village resides in.
    /// </summary>
    public const string TemporaryZoneVariableName = "HarpyVillageZone";

    private const string VillageStructurePath = "Content/Villages/HarpyVillage/Structures/";

    public override string InternalGenerationName => "Harpy Village";

    public override string InsertionPassNameForFeature => "Micro Biomes";

    public override bool PlaceBeforeInsertionPoint => false;

    public override void Generate(GenerationProgress progress, GameConfiguration gameConfig) {
        progress.Message = "Generating Structures... Harpy Village";
        progress.Set(0f);

        //Used to define the rectangle to search and to displace the origin to the actual correct position when generating the village so it doesn't generate on the top left of the rectangle
        int rectangleWidth = 175;
        int rectangleHeight = CurrentWorldSize != WorldSize.Small ? 175 : 123;
        int originHorizontalDisplacement = (int)(rectangleWidth * (0.88f / 1.75f));
        int originVerticalDisplacement = (int)(rectangleHeight * (1.34f / 1.75f));

        int midWorld = Main.maxTilesX / 2;
        int searchLeftX = midWorld - 400;
        int searchRightX = midWorld + 400;
        int startingYLevel = (int)(Main.maxTilesY * 0.025f);

        List<Point> possibleIslandPlacements = new();
        for (int i = searchLeftX; i < searchRightX; i += 5) {
            progress.Set((i - searchLeftX) / (float)(searchRightX - searchLeftX));

            for (int j = startingYLevel; j < GenVars.worldSurface; j++) {
                if (WorldUtils.Find(new Point(midWorld, j), Searches.Chain(
                            new Searches.Down(1),
                            new IsAir().AreaAnd(rectangleWidth, rectangleHeight)
                        ),
                        out Point result)) {
                    possibleIslandPlacements.Add(result);
                }
            }
        }

        Point originPoint;
        if (possibleIslandPlacements.Count != 0) {
            //Get point closest to middle of the world: order the list by distance to the relative "center" of the sky
            originPoint = possibleIslandPlacements.OrderBy(point => point.ToVector2().Distance(new Vector2(midWorld - originHorizontalDisplacement, Main.maxTilesY * 0.06f))).First();

            //Set Harpy Village Zone temporarily
            WorldCreationSystem.Instance.tempWorldGenValues.Add(
                new TemporaryGenValue<Rectangle>(new Rectangle(originPoint.X, originPoint.Y, rectangleWidth, rectangleHeight), TemporaryZoneVariableName)
            );

            //Adjust origin point to be placed correctly within the village "zone" and actually an origin rather than the top corner
            originPoint.X += originHorizontalDisplacement;
            originPoint.Y += originVerticalDisplacement;
        }
        //If no valid placement is found, forcefully place and purge structures in the way.
        else {
            ModContent.GetInstance<LWM>().Logger.Info("No suitable placement found naturally. Forcing Placement.");

            //Set origin point manually
            originPoint = new Point(midWorld, startingYLevel + originVerticalDisplacement);

            //Set Village Zone manually
            Rectangle villageZone = new(originPoint.X - originHorizontalDisplacement, startingYLevel, rectangleWidth, rectangleHeight);
            WorldCreationSystem.Instance.tempWorldGenValues.Add(
                new TemporaryGenValue<Rectangle>(villageZone, TemporaryZoneVariableName)
            );

            //Clear village zone
            for (int i = villageZone.Left; i < villageZone.Right; i++) {
                for (int j = villageZone.Top; j < villageZone.Bottom; j++) {
                    LWMUtils.PurgeStructure(i, j);
                }
            }
        }

        float xScale = WorldGen.genRand.NextFloat(1.6f, 1.75f);
        float yScale = CurrentWorldSize != WorldSize.Small ? 0.65f : 0.45f; //Village is overall much smaller in smaller worlds
        int radius = 35;

        ShapeData mainIslandData = new();

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
            new Modifiers.Offset(0, -(int)(radius * yScale * (CurrentWorldSize != WorldSize.Small ? 0.465f : 0.4f))),
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
        List<string> possibleHouses = new() {
            "HighRise0", "HighRise1", "HighRise2",
            "GroundHouse0", "GroundHouse1", "GroundHouse2", "GroundHouse3", "GroundHouse4", "GroundHouse5"
        };

        for (int i = 0; i < 2; i++) {
            List<Point> possiblePlacementPoints = new();

            string selectedHouseType = WorldGen.genRand.Next(possibleHouses);

            possibleHouses.Remove(selectedHouseType);

            StructureData groundHouseData = LWMUtils.GetStructureFromFile($"{VillageStructurePath}{selectedHouseType}.struct");

            for (int xOffset = leftOffset * (1 - i); xOffset <= 0 + i * Math.Abs(leftOffset); xOffset++) {
                if (WorldUtils.Find(new Point(originPoint.X + xOffset, originPoint.Y), Searches.Chain(new Searches.Up(25), new Conditions.IsTile(TileID.Grass).AreaAnd(groundHouseData.structureWidth, 1)), out Point groundHouseResult)) {
                    //Populates a list of possible placement points where the house can be placed
                    possiblePlacementPoints.Add(groundHouseResult);
                }
            }

            //If there is anywhere possible for the house to be placed, it takes the middle element in order to center the house as much as possible
            if (possiblePlacementPoints.Count != 0) {
                Point middlePlacement = possiblePlacementPoints.ElementAt(possiblePlacementPoints.Count / 2);

                LWMUtils.GenerateStructure(groundHouseData, middlePlacement.X, middlePlacement.Y - groundHouseData.structureHeight);
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
        StructureData churchBuildingData = LWMUtils.GetStructureFromFile($"{VillageStructurePath}ChurchBuilding{WorldGen.genRand.Next(2)}.struct");

        if (WorldUtils.Find(new Point(originPoint.X - churchBuildingData.structureWidth / 2, originPoint.Y + upOffset),
                Searches.Chain(new Searches.Up(75), new IsAir().AreaAnd(churchBuildingData.structureWidth, churchBuildingData.structureHeight)), out Point churchResult)) {
            LWMUtils.GenerateStructure(churchBuildingData, churchResult.X, churchResult.Y);
        }

        //"High Rise" houses are not allowed to generate on the mini islands, so they are removed before the mini islands are generated
        possibleHouses = possibleHouses.Where(house => !house.Contains("HighRise")).ToList();

        //Place smaller island clouds and houses on top of them, for a total of 4 (in Medium+ worlds)
        for (int i = -1; i < 2; i += 2) {
            for (int j = 0; j < (CurrentWorldSize != WorldSize.Small ? 2 : 1); j++) {
                //Only worlds that are Medium size or bigger will generate the top islands
                string selectedHouseType = WorldGen.genRand.Next(possibleHouses);
                possibleHouses.Remove(selectedHouseType);

                StructureData cloudHouseData = LWMUtils.GetStructureFromFile($"{VillageStructurePath}{selectedHouseType}.struct");
                Point miniIslandOrigin = new(originPoint.X + (int)(leftOffset * i * (j == 0 ? 1.15f : 0.775f)), originPoint.Y + upOffset * (int)(j == 0 ? 2f : 6.25f));

                float miniYScale = 0.34f;
                //Diameter will be the radius of the struct plus 5 extra "padding"
                int miniRadius = cloudHouseData.structureWidth / 2 + 5;

                ShapeData miniIslandData = new();

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
                LWMUtils.GenerateStructure(cloudHouseData,
                    miniIslandOrigin.X - cloudHouseData.structureWidth / 2,
                    miniIslandOrigin.Y + miniIslandData.GetData().Min(point => point.Y) - cloudHouseData.structureHeight);
            }
        }
    }

    public override void PostWorldGenAction() {
        if (WorldCreationSystem.Instance.GetTempWorldGenValue<Rectangle>(TemporaryZoneVariableName) is Rectangle rectangle) {
            for (int i = 0; i < rectangle.Width; i++) {
                for (int j = 0; j < rectangle.Height; j++) {
                    Point currentPos = new(rectangle.X + i, rectangle.Y + j);

                    Tile currentTile = Framing.GetTileSafely(currentPos);

                    if (currentTile.TileType != ModContent.TileType<VillageShrineTile>()) {
                        continue;
                    }

                    Point topLeft = LWMUtils.GetCornerOfMultiTile(currentTile, currentPos.X, currentPos.Y, LWMUtils.CornerType.TopLeft);

                    ModContent.GetInstance<VillageShrineEntity>().Place(topLeft.X, topLeft.Y);
                }
            }

            for (int i = 0; i < rectangle.Width; i++) {
                for (int j = 0; j < rectangle.Height; j++) {
                    Point position = new(rectangle.X + i, rectangle.Y + j);
                    int harpyType = ModContent.NPCType<HarpyVillager>();

                    if (WorldGen.StartRoomCheck(position.X, position.Y) && WorldGen.RoomNeeds(harpyType)) {
                        WorldGen.ScoreRoom(npcTypeAskingToScoreRoom: harpyType);

                        //A "high score" of 0 or less means the room is occupied or the score otherwise failed
                        if (WorldGen.hiScore <= 0) {
                            continue;
                        }

                        int npc = NPC.NewNPC(new EntitySource_WorldGen(), WorldGen.bestX * 16, WorldGen.bestY * 16, harpyType);

                        Main.npc[npc].homeTileX = WorldGen.bestX;
                        Main.npc[npc].homeTileY = WorldGen.bestY;
                    }
                }
            }
        }
    }
}