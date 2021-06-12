using LivingWorldMod.Custom.Classes.StructureCaches;
using Terraria;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Custom.Utilities {

    public static class WorldGenUtilities {

        /// <summary>
        /// Generates a given structure cache's data into the world.
        /// </summary>
        /// <param name="cache"> Cache to gather data from. </param>
        /// <param name="xLocation"> Far left location of where the structure will begin to generate. </param>
        /// <param name="yLocation"> Top-most location of where the structure will begin to generate. </param>
        /// <param name="progress">
        /// Progress of the loops to show the player how far along the generation is.
        /// </param>
        public static void GenerateWithStructureCache(StructureCache cache, int xLocation, int yLocation, GenerationProgress progress) {
            for (int y = 0; y < cache.TileTypes.GetLength(0); y++) {
                progress.Set((float)y / cache.TileTypes.GetLength(0));
                for (int x = 0; x < cache.TileTypes.GetLength(1); x++) {
                    Tile selectedTile = Framing.GetTileSafely(xLocation + x, yLocation + y);
                    if (cache.TileTypes[y, x] != -1) {
                        selectedTile.type = (ushort)cache.TileTypes[y, x];
                        selectedTile.IsActive = true;
                    }

                    selectedTile.IsHalfBlock = cache.AreTilesHalfBlocks[y, x];
                    selectedTile.FrameNumber = (byte)cache.TileFrameNumbers[y, x];
                    selectedTile.frameX = (short)cache.TileFrameXs[y, x];
                    selectedTile.frameY = (short)cache.TileFrameYs[y, x];
                    selectedTile.Slope = (SlopeType)cache.TileSlopeTypes[y, x];
                    selectedTile.Color = (byte)cache.TileColors[y, x];
                    selectedTile.IsActuated = cache.AreTilesActuated[y, x];
                    selectedTile.HasActuator = cache.HaveActuators[y, x];
                    selectedTile.RedWire = cache.HaveRedWires[y, x];
                    selectedTile.BlueWire = cache.HaveBlueWires[y, x];
                    selectedTile.GreenWire = cache.HaveGreenWires[y, x];
                    selectedTile.YellowWire = cache.HaveYellowWires[y, x];
                    selectedTile.LiquidType = cache.LiquidTypes[y, x];
                    selectedTile.LiquidAmount = (byte)cache.LiquidAmounts[y, x];
                    selectedTile.wall = (ushort)cache.WallTypes[y, x];
                    selectedTile.WallColor = (byte)cache.WallColors[y, x];
                    selectedTile.WallFrameNumber = (byte)cache.WallFrameNumbers[y, x];
                    selectedTile.WallFrameX = cache.WallFrameXs[y, x];
                    selectedTile.WallFrameY = cache.WallFrameYs[y, x];
                }
            }
        }
    }
}