using LivingWorldMod.Custom.Structs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Custom.Utilities {

    public static class WorldGenUtilities {

        /// <summary>
        /// Generates a given Structure into the world using a StructureData struct.
        /// </summary>
        /// <param name="data"> The struct containing data for the structure. </param>
        /// <param name="startingX"> Far left location of where the structure will begin to generate. </param>
        /// <param name="startingY"> Top-most location of where the structure will begin to generate. </param>
        public static void GenerateStructure(StructureData data, int startingX, int startingY) {
            for (int y = 0; y < data.structureHeight; y++) {
                for (int x = 0; x < data.structureWidth; x++) {
                    Tile selectedTile = Framing.GetTileSafely(startingX + x, startingY + y);
                    TileData tileData = data.structureTileData[x][y];

                    switch (tileData.type) {
                        case > 0: {
                                if (ModContent.TryFind(tileData.modName, tileData.modTileName, out ModTile modTile)) {
                                    selectedTile.type = modTile.Type;
                                }
                                else {
                                    selectedTile.type = (ushort)tileData.type;
                                }
                                selectedTile.IsActive = true;

                                selectedTile.IsHalfBlock = tileData.isHalfBlock;
                                selectedTile.FrameNumber = (byte)tileData.frameNumber;
                                selectedTile.frameX = (short)tileData.frameX;
                                selectedTile.frameY = (short)tileData.frameY;
                                selectedTile.Slope = (SlopeType)tileData.slopeType;
                                selectedTile.Color = (byte)tileData.color;
                                selectedTile.IsActuated = tileData.isActuated;
                                selectedTile.HasActuator = tileData.hasActuator;
                                selectedTile.RedWire = tileData.hasRedWire;
                                selectedTile.BlueWire = tileData.hasBlueWire;
                                selectedTile.GreenWire = tileData.hasGreenWire;
                                selectedTile.YellowWire = tileData.hasYellowWire;
                                selectedTile.LiquidType = tileData.liquidType;
                                selectedTile.LiquidAmount = (byte)tileData.liquidAmount;
                                break;
                            }
                        case -1:
                            selectedTile.type = 0;
                            selectedTile.IsActive = false;
                            break;
                    }

                    if (tileData.wallType != -1) {
                        selectedTile.wall = (ushort)tileData.wallType;
                        selectedTile.WallColor = (byte)tileData.wallColor;
                        selectedTile.WallFrameNumber = (byte)tileData.wallFrame;
                        selectedTile.WallFrameX = tileData.wallFrameX;
                        selectedTile.WallFrameY = tileData.wallFrameY;
                    }
                }
            }
        }

        /// <summary>
        /// Generates a given Structure into the world using a StructureData struct.
        /// </summary>
        /// <param name="data"> The struct containing data for the structure. </param>
        /// <param name="startingX"> Far left location of where the structure will begin to generate. </param>
        /// <param name="startingY"> Top-most location of where the structure will begin to generate. </param>
        /// <param name="progress">
        /// Progress of the loops to show the player how far along the generation is, with its
        /// primary usage being during world creation.
        /// </param>
        public static void GenerateStructure(StructureData data, int startingX, int startingY, ref GenerationProgress progress) {
            for (int y = 0; y < data.structureHeight; y++) {
                progress.Set((float)y / data.structureHeight);
                for (int x = 0; x < data.structureWidth; x++) {
                    Tile selectedTile = Framing.GetTileSafely(startingX + x, startingY + y);
                    TileData tileData = data.structureTileData[x][y];

                    switch (tileData.type) {
                        case > 0: {
                                if (ModContent.TryFind(tileData.modName, tileData.modTileName, out ModTile modTile)) {
                                    selectedTile.type = modTile.Type;
                                }
                                else {
                                    selectedTile.type = (ushort)tileData.type;
                                }
                                selectedTile.IsActive = true;

                                selectedTile.IsHalfBlock = tileData.isHalfBlock;
                                selectedTile.FrameNumber = (byte)tileData.frameNumber;
                                selectedTile.frameX = (short)tileData.frameX;
                                selectedTile.frameY = (short)tileData.frameY;
                                selectedTile.Slope = (SlopeType)tileData.slopeType;
                                selectedTile.Color = (byte)tileData.color;
                                selectedTile.IsActuated = tileData.isActuated;
                                selectedTile.HasActuator = tileData.hasActuator;
                                selectedTile.RedWire = tileData.hasRedWire;
                                selectedTile.BlueWire = tileData.hasBlueWire;
                                selectedTile.GreenWire = tileData.hasGreenWire;
                                selectedTile.YellowWire = tileData.hasYellowWire;
                                selectedTile.LiquidType = tileData.liquidType;
                                selectedTile.LiquidAmount = (byte)tileData.liquidAmount;
                                break;
                            }
                        case -1:
                            selectedTile.ClearEverything();
                            break;
                    }

                    if (tileData.wallType != -1) {
                        selectedTile.wall = (ushort)tileData.wallType;
                        selectedTile.WallColor = (byte)tileData.wallColor;
                        selectedTile.WallFrameNumber = (byte)tileData.wallFrame;
                        selectedTile.WallFrameX = tileData.wallFrameX;
                        selectedTile.WallFrameY = tileData.wallFrameY;
                    }
                }
            }
        }
    }
}