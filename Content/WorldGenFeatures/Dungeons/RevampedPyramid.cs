using System.Collections.Generic;
using Terraria;
using LivingWorldMod.Common.ModTypes;
using LivingWorldMod.Content.Tiles.Interactables;
using LivingWorldMod.Custom.Structs;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Content.WorldGenFeatures.Dungeons {
    /// <summary>
    /// The completely revamped pyramid structure, now with curses!
    /// </summary>
    public class RevampedPyramid : WorldGenFeature {
        public override string InternalGenerationName => "Revamped Pyramid";

        public override string InsertionPassNameForFeature => "Pyramids";

        public override bool PlaceBeforeInsertionPoint => false;

        //Don't load for the time being while in INDEV
        public override bool IsLoadingEnabled(Mod mod) => LivingWorldMod.IsDebug;

        public override void ModifyTaskList(List<GenPass> tasks) {
            //Decided that vanilla pyramids will be unable to generate. We must "nullify" the pass here, by removing and re-adding it in an
            // empty pass in its place, to preserve mod compatibility.
            int pyramidIndex = tasks.FindIndex(pass => pass.Name == "Pyramids");
            if (pyramidIndex != -1) {
                //Remove vanilla pass
                tasks.RemoveAt(pyramidIndex);
                //Replace with empty pass
                tasks.Insert(pyramidIndex, new PassLegacy("Pyramids", (progress, configuration) => { }));
            }
            else {
                ModContent.GetInstance<LivingWorldMod>().Logger.Warn("Pyramid pass not found. Generating revamped pyramid at end of the task list!");
            }
        }

        public override void Generate(GenerationProgress progress, GameConfiguration gameConfig) {
            progress.Message = "Cursing the Pyramid";
            progress.Set(0f);

            int xLocation = WorldGen.UndergroundDesertLocation.Center.X + 75 * WorldGen.genRand.NextBool().ToDirectionInt();
            int yLocation = WorldGen.UndergroundDesertLocation.Top;
            int attempts = 0;

            while (Framing.GetTileSafely(xLocation, yLocation).HasTile) {
                yLocation--;

                if (++attempts <= 300) {
                    continue;
                }

                //If we got this far, then some kind of shenanigans have occurred and we need to compensate before an infinite loop happens
                ModContent.GetInstance<LivingWorldMod>().Logger.Warn("Revamped Pyramid unable to generate due to no valid placement.");
                return;
            }

            GenOverworldPyramid(xLocation, yLocation);
        }

        //This is adapted vanilla code that creates a pyramid with no tunnel past the treasure room, and the tunnel room itself is changed
        private void GenOverworldPyramid(int i, int j) {
            int entranceDisplacement = WorldGen.genRand.Next(9, 13);
            int topDisplacement = 1;
            int bottomY = j + 75;
            Tile tile;
            for (int y = j; y < bottomY; y++) {
                for (int x = i - topDisplacement; x < i + topDisplacement - 1; x++) {
                    tile = Framing.GetTileSafely(x, y);
                    tile.TileType = TileID.SandStoneSlab;
                    tile.HasTile = true;
                    tile.IsHalfBlock = false;
                    tile.Slope = SlopeType.Solid;
                }
                ++topDisplacement;
            }
            for (int index1 = i - topDisplacement - 5; index1 <= i + topDisplacement + 5; index1++) {
                for (int index2 = j - 1; index2 <= bottomY + 1; index2++) {
                    bool sandstoneFound = true;
                    for (int x = index1 - 1; x <= index1 + 1; x++) {
                        for (int y = index2 - 1; y <= index2 + 1; y++) {
                            if (Framing.GetTileSafely(x, y).TileType != TileID.SandStoneSlab) {
                                sandstoneFound = false;
                            }
                        }
                    }
                    if (!sandstoneFound) {
                        continue;
                    }

                    tile = Framing.GetTileSafely(index1, index2);
                    tile.WallType = WallID.SandstoneBrick;
                    WorldGen.SquareWallFrame(index1, index2);
                }
            }
            int entranceDirection = WorldGen.genRand.NextBool().ToDirectionInt();
            int entranceX = i - entranceDisplacement * entranceDirection;
            int entranceY = j + entranceDisplacement;
            int tunnelSize = WorldGen.genRand.Next(5, 8);
            bool continueInitialTunnelGen = true;
            int roomTunnelDisplacement = WorldGen.genRand.Next(20, 30);
            while (continueInitialTunnelGen) {
                continueInitialTunnelGen = false;
                bool sandFound = false;
                for (int y = entranceY; y <= entranceY + tunnelSize; y++) {
                    if (Framing.GetTileSafely(entranceX, y - 1).TileType == TileID.Sand) {
                        sandFound = true;
                    }
                    if (Framing.GetTileSafely(entranceX, y).TileType == TileID.SandStoneSlab) {
                        Framing.GetTileSafely(entranceX, y + 1).WallType = WallID.SandstoneBrick;
                        Framing.GetTileSafely(entranceX + entranceDirection, y).WallType = WallID.SandstoneBrick;
                        tile = Framing.GetTileSafely(entranceX, y);
                        tile.HasTile = false;
                        continueInitialTunnelGen = true;
                    }
                    if (sandFound) {
                        tile = Framing.GetTileSafely(entranceX, y);
                        tile.TileType = WallID.DiamondUnsafe;
                        tile.HasTile = true;
                        tile.IsHalfBlock = false;
                        tile.Slope = SlopeType.Solid;
                    }
                }
                entranceX -= entranceDirection;
            }
            int roomTunnelX = i - entranceDisplacement * entranceDirection;
            bool doRoomGen = false;
            bool continueRoomTunnelGen = true;
            while (continueRoomTunnelGen) {
                for (int y = entranceY; y <= entranceY + tunnelSize; y++) {
                    tile = Framing.GetTileSafely(roomTunnelX, y);
                    tile.HasTile = false;
                }
                roomTunnelX += entranceDirection;
                ++entranceY;
                --roomTunnelDisplacement;
                if (entranceY >= bottomY - tunnelSize * 2) {
                    roomTunnelDisplacement = 10;
                }
                if (roomTunnelDisplacement <= 0) {
                    if (doRoomGen) {
                        int roomHeight = WorldGen.genRand.Next(7, 13);
                        int roomX = WorldGen.genRand.Next(23, 28);
                        int roomWidth = roomX;
                        int roomY = roomTunnelX;
                        while (roomX > 0) {
                            for (int y = entranceY - roomHeight + tunnelSize; y <= entranceY + tunnelSize; y++) {
                                if (roomX == roomWidth || roomX == 1) {
                                    if (y < entranceY - roomHeight + tunnelSize + 2) {
                                        continue;
                                    }

                                    tile = Framing.GetTileSafely(roomTunnelX, y);
                                    tile.HasTile = false;
                                }
                                else if (roomX == roomWidth - 1 || roomX == 2 || roomX == roomWidth - 2 || roomX == 3) {
                                    if (y < entranceY - roomHeight + tunnelSize + 1) {
                                        continue;
                                    }

                                    tile = Framing.GetTileSafely(roomTunnelX, y);
                                    tile.HasTile = false;
                                }
                                else {
                                    tile = Framing.GetTileSafely(roomTunnelX, y);
                                    tile.HasTile = false;
                                }
                            }
                            --roomX;
                            roomTunnelX += entranceDirection;
                        }
                        int edgeCheck = roomTunnelX - entranceDirection;
                        int roomEdge1 = edgeCheck;
                        int roomEdge2 = roomY;
                        if (edgeCheck > roomY) {
                            roomEdge1 = roomY;
                            roomEdge2 = edgeCheck;
                        }
                        WorldGen.PlaceTile(roomEdge1 + 2, entranceY - roomHeight + tunnelSize + 1, TileID.Banners, true, style: WorldGen.genRand.Next(4, 7));
                        WorldGen.PlaceTile(roomEdge1 + 3, entranceY - roomHeight + tunnelSize, TileID.Banners, true, style: WorldGen.genRand.Next(4, 7));
                        WorldGen.PlaceTile(roomEdge2 - 2, entranceY - roomHeight + tunnelSize + 1, TileID.Banners, true, style: WorldGen.genRand.Next(4, 7));
                        WorldGen.PlaceTile(roomEdge2 - 3, entranceY - roomHeight + tunnelSize, TileID.Banners, true, style: WorldGen.genRand.Next(4, 7));

                        WorldGen.PlaceObject(edgeCheck > roomY ? roomEdge1 + 6 : roomEdge2 - 7, entranceY + tunnelSize - 3, ModContent.TileType<PyramidDoorTile>(), style: 0);

                        for (int x3 = roomEdge1; x3 <= roomEdge2; x3++) {
                            WorldGen.PlacePot(x3, entranceY + tunnelSize, style: WorldGen.genRand.Next(25, 28));
                        }
                        return;
                    }
                    doRoomGen = true;
                    entranceDirection *= -1;
                    roomTunnelDisplacement = WorldGen.genRand.Next(15, 20);
                }
                if (entranceY >= bottomY - tunnelSize) {
                    continueRoomTunnelGen = false;
                }
            }
        }
    }
}