using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.Waystones.DataStructures.Enums;
using LivingWorldMod.Content.Waystones.Tiles;
using LivingWorldMod.DataStructures.Classes.GenConditions;
using LivingWorldMod.DataStructures.Enums;
using LivingWorldMod.Globals.ModTypes;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Content.Waystones.WorldGenFeatures;

/// <summary>
/// Handles the Worldgen pass that handles the placement of Waystones.
/// </summary>
public class Waystones : WorldGenFeature {
    public override string InternalGenerationName => "Waystones";

    public override string InsertionPassNameForFeature => "Statues";

    public override void Generate(GenerationProgress progress, GameConfiguration gameConfig) {
        progress.Message = "Paving the way";
        progress.Set(0f);

        // How many times we will cut up the world when searching for places to put Waystones
        float worldDivisions = LWMUtils.CurrentWorldSize switch {
            WorldSize.Small => 500f,
            WorldSize.Medium => 1000f,
            WorldSize.Large => 1500f,
            WorldSize.Custom => 300f,
            _ => 50f
        };

        int minTilesBetweenWaystones = LWMUtils.CurrentWorldSize switch {
            WorldSize.Small => 250,
            WorldSize.Medium => 325,
            WorldSize.Large => 375,
            WorldSize.Custom => 250,
            _ => 175
        };

        //Define what waystone types can have what tiles under them
        Dictionary<WaystoneType, ushort[]> typeToValidTiles = new() {
            { WaystoneType.Desert, new[] { TileID.HardenedSand, TileID.Sandstone } },
            { WaystoneType.Jungle, new[] { TileID.JungleGrass } },
            { WaystoneType.Mushroom, new[] { TileID.MushroomGrass } },
            { WaystoneType.Caverns, new[] { TileID.Stone } },
            { WaystoneType.Ice, new[] { TileID.IceBlock, TileID.SnowBlock } }
        };

        //Get all the valid tiles together for faster searching
        ushort[] allValidTiles = typeToValidTiles.Values.ToArray().SelectMany(item => item).Distinct().ToArray();

        //Maximum height is the top of the RockLayer, and the lowest being right above the underworld
        int yBottomBound = (int)Main.rockLayer;
        int yTopBound = Main.maxTilesY - 200; //Vanilla definition of the transition point between underworld and not-underworld
        int xBoundFluff = 40;

        WaystoneEntity waystoneEntity = ModContent.GetInstance<WaystoneEntity>();

        //First, we will simply satisfy that at least one mushroom biome waystone spawns, since that is the least likely
        for (int i = xBoundFluff; i < Main.maxTilesX - xBoundFluff; i += (int)(Main.maxTilesX / worldDivisions)) {
            for (int j = yBottomBound; j < yTopBound; j++) {
                //Make sure to prevent any out of world shenanigans, if possible
                if (!WorldGen.InWorld(i, j, 4)) {
                    continue;
                }
                Point searchOrigin = new(i, j);

                // Test for 2x3 pocket of air
                if (!WorldUtils.Find(searchOrigin, Searches.Chain(
                            new Searches.Rectangle(2, 3),
                            new IsAirOrCuttable().AreaAnd(2, 3),
                            new IsDry().AreaAnd(2, 3)),
                        out Point _)) {
                    continue;
                }

                //Make sure there are two solid mushroom grass tiles
                if (!WorldUtils.Find(searchOrigin + new Point(0, 3), Searches.Chain(
                            new Searches.Rectangle(2, 1),
                            new Conditions.IsSolid().AreaAnd(2, 1),
                            new Conditions.IsTile(typeToValidTiles[WaystoneType.Mushroom]).AreaAnd(2, 1)),
                        out _)) {
                    continue;
                }

                //Clear objects out of the way, since there might be things like grass or mushrooms in the way the technically prevent placement
                WorldUtils.Gen(searchOrigin, new Shapes.Rectangle(2, 3), new Actions.ClearTile(true));

                //Attempt to place. If it does not place, continue and try a different location
                if (!WorldGen.PlaceObject(i, j, ModContent.TileType<WaystoneTile>(), style: (int)WaystoneType.Mushroom)) {
                    continue;
                }

                //Place tile entities
                if (waystoneEntity.ManualPlace(i, j, WaystoneType.Mushroom, LWM.IsDebug) && LWM.IsDebug) {
                    ModContent.GetInstance<LWM>().Logger.Info($"Placed Waystone at {i}, {j}");
                }

                //Assuming we get here, break and move out of loop
                i = Main.maxTilesX;
                break;
            }
        }

        //Secondly (and finally), we will generate the rest of the waystone types, with varying conditions based on world size
        for (int i = xBoundFluff; i < Main.maxTilesX - xBoundFluff; i += (int)(Main.maxTilesX / worldDivisions)) {
            for (int j = yBottomBound; j < yTopBound; j++) {
                if (!WorldGen.InWorld(i, j, 4)) {
                    continue;
                }
                Point searchOrigin = new(i, j);

                // Test for 2x3 pocket of air
                if (!WorldUtils.Find(searchOrigin, Searches.Chain(
                            new Searches.Rectangle(2, 3),
                            new IsAirOrCuttable().AreaAnd(2, 3),
                            new IsDry().AreaAnd(2, 3)),
                        out Point _)) {
                    continue;
                }

                //Make sure there are two solid tiles below that pocket of air, and is a valid tile
                if (!WorldUtils.Find(searchOrigin + new Point(0, 3), Searches.Chain(
                            new Searches.Rectangle(2, 1),
                            new Conditions.IsSolid().AreaAnd(2, 1),
                            new Conditions.IsTile(allValidTiles)),
                        out Point tileBasePoint)) {
                    continue;
                }

                //Do a random check in order to not simply spam the waystones all over the place
                if (WorldGen.genRand.NextFloat() > 0.075f) {
                    continue;
                }

                //Finally, for the last check, make sure it isn't too close to any other Waystones
                foreach (WaystoneEntity entity in LWMUtils.GetAllEntityOfType<WaystoneEntity>()) {
                    if (entity.Position.ToVector2().Distance(searchOrigin.ToVector2()) < minTilesBetweenWaystones) {
                        goto ContinueLoop;
                    }
                }

                //Place waystone depending on the tile base
                WaystoneType determinedWaystoneType = WaystoneType.Desert;
                for (WaystoneType waystoneType = WaystoneType.Desert; waystoneType <= WaystoneType.Ice; waystoneType = waystoneType.NextEnum()) {
                    if (WorldUtils.Find(tileBasePoint, Searches.Chain(
                                new Searches.Rectangle(2, 1),
                                new Conditions.IsTile(typeToValidTiles[waystoneType])),
                            out _)) {
                        determinedWaystoneType = waystoneType;
                        break;
                    }
                }

                //Clear objects out of the way, since there might be things like grass or mushrooms in the way the technically prevent placement
                WorldUtils.Gen(searchOrigin, new Shapes.Rectangle(2, 3), new Actions.ClearTile(true));

                //Attempt to place. If it does not place, continue and try a different location
                if (!WorldGen.PlaceObject(i, j, ModContent.TileType<WaystoneTile>(), style: (int)determinedWaystoneType)) {
                    continue;
                }

                //Place tile entities
                if (waystoneEntity.ManualPlace(i, j, determinedWaystoneType, LWM.IsDebug) && LWM.IsDebug) {
                    ModContent.GetInstance<LWM>().Logger.Info($"Placed Waystone at {i}, {j}");
                }

                ContinueLoop: ;
            }
        }
    }
}