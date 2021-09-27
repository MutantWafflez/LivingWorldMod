using System.Reflection;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Structs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Custom.Utilities {

    /// <summary>
    /// Class that holds methods and properties that assist with world generation or world related matters.
    /// </summary>
    public static class WorldGenUtilities {

        /// <summary>
        /// Returns world size of the current world being played.
        /// </summary>
        public static WorldSize CurrentWorldSize {
            get {
                switch (Main.maxTilesX) {
                    case 4200: //Small
                        return WorldSize.Small;

                    case 6300: //Medium
                        return WorldSize.Medium;

                    case 8400: //Large
                        return WorldSize.Large;

                    default: //Non-vanilla world size
                        return WorldSize.Custom;
                }
            }
        }

        /// <summary>
        /// Generates a given Structure into the world using a StructureData struct.
        /// </summary>
        /// <param name="data"> The struct containing data for the structure. </param>
        /// <param name="startingX"> Far left location of where the structure will begin to generate. </param>
        /// <param name="startingY"> Top-most location of where the structure will begin to generate. </param>
        /// <param name="autoFrame">
        /// Whether or not the entire structure should be framed, in terms of both walls and tiles,
        /// when finished being generated.
        /// </param>
        public static void GenerateStructure(StructureData data, int startingX, int startingY, bool autoFrame = true) {
            for (int y = 0; y < data.structureHeight; y++) {
                for (int x = 0; x < data.structureWidth; x++) {
                    Tile selectedTile = Framing.GetTileSafely(startingX + x + data.structureDisplacement.X, startingY + y + data.structureDisplacement.Y);
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
                            selectedTile.HasActuator = tileData.hasActuator;
                            selectedTile.RedWire = tileData.hasRedWire;
                            selectedTile.BlueWire = tileData.hasBlueWire;
                            selectedTile.GreenWire = tileData.hasGreenWire;
                            selectedTile.YellowWire = tileData.hasYellowWire;
                            selectedTile.LiquidType = tileData.liquidType;
                            selectedTile.LiquidAmount = (byte)tileData.liquidAmount;
                            break;
                    }

                    if (tileData.wallType != -1) {
                        if (ModContent.TryFind(tileData.modName, tileData.modWallName, out ModWall modWall)) {
                            selectedTile.wall = modWall.Type;
                        }
                        else {
                            selectedTile.wall = (ushort)tileData.wallType;
                        }
                        selectedTile.WallColor = (byte)tileData.wallColor;
                        selectedTile.WallFrameNumber = (byte)tileData.wallFrame;
                        selectedTile.WallFrameX = tileData.wallFrameX;
                        selectedTile.WallFrameY = tileData.wallFrameY;
                    }
                }
            }

            if (!autoFrame) {
                return;
            }

            for (int y = 0; y < data.structureHeight; y++) {
                for (int x = 0; x < data.structureWidth; x++) {
                    WorldGen.TileFrame(startingX + x + data.structureDisplacement.X, startingY + y + data.structureDisplacement.Y, true, true);
                    Framing.WallFrame(startingX + x + data.structureDisplacement.X, startingY + y + data.structureDisplacement.Y, true);
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
        /// <param name="autoFrame">
        /// Whether or not the entire structure should be framed, in terms of both walls and tiles,
        /// when finished being generated.
        /// </param>
        public static void GenerateStructure(StructureData data, int startingX, int startingY, ref GenerationProgress progress, bool autoFrame = true) {
            for (int y = 0; y < data.structureHeight; y++) {
                progress.Set((float)y / data.structureHeight);
                for (int x = 0; x < data.structureWidth; x++) {
                    Tile selectedTile = Framing.GetTileSafely(startingX + x + data.structureDisplacement.X, startingY + y + data.structureDisplacement.Y);
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
                            selectedTile.HasActuator = tileData.hasActuator;
                            selectedTile.RedWire = tileData.hasRedWire;
                            selectedTile.BlueWire = tileData.hasBlueWire;
                            selectedTile.GreenWire = tileData.hasGreenWire;
                            selectedTile.YellowWire = tileData.hasYellowWire;
                            selectedTile.LiquidType = tileData.liquidType;
                            selectedTile.LiquidAmount = (byte)tileData.liquidAmount;
                            break;
                    }

                    if (tileData.wallType != -1) {
                        if (ModContent.TryFind(tileData.modName, tileData.modWallName, out ModWall modWall)) {
                            selectedTile.wall = modWall.Type;
                        }
                        else {
                            selectedTile.wall = (ushort)tileData.wallType;
                        }
                        selectedTile.WallColor = (byte)tileData.wallColor;
                        selectedTile.WallFrameNumber = (byte)tileData.wallFrame;
                        selectedTile.WallFrameX = tileData.wallFrameX;
                        selectedTile.WallFrameY = tileData.wallFrameY;
                    }
                }
            }

            if (!autoFrame) {
                return;
            }

            for (int y = 0; y < data.structureHeight; y++) {
                for (int x = 0; x < data.structureWidth; x++) {
                    WorldGen.TileFrame(startingX + x + data.structureDisplacement.X, startingY + y + data.structureDisplacement.Y, true, true);
                    Framing.WallFrame(startingX + x + data.structureDisplacement.X, startingY + y + data.structureDisplacement.Y, true);
                }
            }
        }
    }
}