using System;
using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Common.ModTypes;
using LivingWorldMod.Common.Systems;
using LivingWorldMod.Common.VanillaOverrides.WorldGen.GenConditions;
using LivingWorldMod.Content.TileEntities.Interactables;
using LivingWorldMod.Content.Tiles.Interactables;
using LivingWorldMod.Custom.Classes;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Content.WorldGenFeatures.Miscellaneous {
    /// <summary>
    /// Handles the Worldgen pass that handles the placement of Waystones.
    /// </summary>
    public class Waystones : WorldGenFeature {
        /// <summary>
        /// Generates an instance of the current scene metrics for the generating world.
        /// </summary>
        public SceneMetrics GenerateMetrics => new SceneMetrics(Main.ActiveWorld);

        public override string InternalGenerationName => "Waystones";

        public override string InsertionPassNameForFeature => "Micro Biomes";

        public override void Generate(GenerationProgress progress, GameConfiguration gameConfig) {
            progress.Message = "Paving the way";
            progress.Set(0f);

            WaystoneType BiomeToWaystoneType(int i, int j) {
                SceneMetrics grabbedMetrics = GenerateMetrics;
                SceneMetricsScanSettings settings = new SceneMetricsScanSettings {
                    VisualScanArea = null,
                    BiomeScanCenterPositionInWorld = new Vector2(i * 16, j * 16),
                    ScanOreFinderData = false
                };

                grabbedMetrics.ScanAndExportToMain(settings);

                //Kinda bad to look at, but not much else that can be changed in this case
                if (grabbedMetrics.EnoughTilesForDesert) {
                    return WaystoneType.Desert;
                }
                else if (grabbedMetrics.EnoughTilesForJungle) {
                    return WaystoneType.Jungle;
                }
                else if (grabbedMetrics.EnoughTilesForGlowingMushroom) {
                    return WaystoneType.Mushroom;
                }
                else if (grabbedMetrics.EnoughTilesForSnow) {
                    return WaystoneType.Ice;
                }
                else {
                    return WaystoneType.Caverns;
                }
            }

            // How many times we will cut up the world when searching for places to put Waystones
            float worldDivisions = WorldGenUtils.CurrentWorldSize switch {
                WorldSize.Small => 100f,
                WorldSize.Medium => 200f,
                WorldSize.Large => 300f,
                WorldSize.Custom => 175f,
                _ => 50f
            };

            //Maximum height is the top of the RockLayer, and the lowest being right above the underworld
            int yBottomBound = (int)Main.rockLayer;
            int yTopBound = Main.maxTilesY - 200; //Vanilla definition of the transition point between underworld and not-underworld
            int xBoundFluff = 40;
            int minDistanceBetweenWaystones = 100; //Distance is in tiles

            //First, we will simply satisfy that at least one mushroom biome waystone spawns, since that is the least likely
            for (int i = xBoundFluff; i < Main.maxTilesX - xBoundFluff; i += (int)(Main.maxTilesX / worldDivisions)) {
                for (int j = yBottomBound; j < yTopBound; j++) {
                    //Make sure to prevent any out of world shenanigans, if possible
                    if (!WorldGen.InWorld(i, j)) {
                        continue;
                    }
                    Point searchOrigin = new Point(i, j);

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
                                new Conditions.IsTile(TileID.MushroomGrass).AreaAnd(2, 1)),
                            out Point _)) {
                        continue;
                    }

                    //Clear objects out of the way, since there might be things like grass or mushrooms in the way the technically prevent placement
                    WorldUtils.Gen(searchOrigin, new Shapes.Rectangle(2, 3), new Actions.ClearTile(true));

                    //Attempt to place. If it does not place, continue and try a different location
                    if (!WorldGen.PlaceObject(i, j, ModContent.TileType<WaystoneTile>(), style: (int)WaystoneType.Mushroom)) {
                        continue;
                    }

                    //Place tile entities
                    WaystoneSystem.BaseWaystoneEntity.PlaceEntity(i, j, (int)WaystoneType.Mushroom);
                    ModContent.GetInstance<LivingWorldMod>().Logger.Info($"Placed Waystone at {i}, {j}");

                    //Assuming we get here, break and move out of loop
                    i = Main.maxTilesX;
                    break;
                }
            }

            //Secondly (and finally), we will generate the rest of the waystone types, with varying conditions based on world size
            for (int i = xBoundFluff; i < Main.maxTilesX - xBoundFluff; i += (int)(Main.maxTilesX / worldDivisions)) {
                for (int j = yBottomBound; j < yTopBound; j++) {
                    if (!WorldGen.InWorld(i, j)) {
                        continue;
                    }
                    Point searchOrigin = new Point(i, j);

                    // Test for 2x3 pocket of air
                    if (!WorldUtils.Find(searchOrigin, Searches.Chain(
                                new Searches.Rectangle(2, 3),
                                new IsAirOrCuttable().AreaAnd(2, 3),
                                new IsDry().AreaAnd(2, 3)),
                            out Point _)) {
                        continue;
                    }

                    //Make sure there are two solid tiles below that pocket of air, and they aren't dungeon tiles
                    if (!WorldUtils.Find(searchOrigin + new Point(0, 3), Searches.Chain(
                                new Searches.Rectangle(2, 1),
                                new Conditions.IsSolid().AreaAnd(2, 1),
                                new Conditions.IsTile(TileID.BlueDungeonBrick, TileID.GreenDungeonBrick, TileID.PinkDungeonBrick).AreaAnd(2, 1).Not()),
                            out _)) {
                        continue;
                    }

                    //Do a random check in order to not simply spam the waystones all over the place
                    if (WorldGen.genRand.NextFloat() > 0.075f) {
                        continue;
                    }

                    //Finally, for the last check, make sure it isn't too close to any other Waystones
                    foreach (WaystoneInfo info in ModContent.GetInstance<WaystoneSystem>().waystoneData) {
                        if (info.iconLocation.Distance(new Vector2(i, j)) < minDistanceBetweenWaystones) {
                            goto ContinueLoop;
                        }
                    }

                    //Place waystone depending on the biome of its location
                    WaystoneType determinedWaystoneType = BiomeToWaystoneType(i, j);

                    //Clear objects out of the way, since there might be things like grass or mushrooms in the way the technically prevent placement
                    WorldUtils.Gen(searchOrigin, new Shapes.Rectangle(2, 3), new Actions.ClearTile(true));

                    //Attempt to place. If it does not place, continue and try a different location
                    if (!WorldGen.PlaceObject(i, j, ModContent.TileType<WaystoneTile>(), style: (int)determinedWaystoneType)) {
                        continue;
                    }

                    //Place tile entities
                    WaystoneSystem.BaseWaystoneEntity.PlaceEntity(i, j, (int)determinedWaystoneType);
                    ModContent.GetInstance<LivingWorldMod>().Logger.Info($"Placed Waystone at {i}, {j}");

                    ContinueLoop:
                    continue;
                }
            }
        }
    }
}