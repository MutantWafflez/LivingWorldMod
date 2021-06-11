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
            for (int y = 0; y < cache.currentTileArray.GetLength(0); y++) {
                progress.Set((float)y / cache.currentTileArray.GetLength(0));
                for (int x = 0; x < cache.currentTileArray.GetLength(1); x++) {
                    Tile selectedTile = Framing.GetTileSafely(xLocation + x, yLocation + y);
                    if (cache.currentTileArray[y, x] != -1) {
                        selectedTile.type = (ushort)cache.currentTileArray[y, x];
                        selectedTile.IsActive = true;
                    }

                    selectedTile.Slope = (SlopeType)cache.currentSlopeArray[y, x];
                    selectedTile.wall = (ushort)cache.currentWallArray[y, x];
                    selectedTile.LiquidAmount = (byte)cache.currentLiquidArray[y, x];
                    selectedTile.frameX = (short)cache.currentXFrameArray[y, x];
                    selectedTile.frameY = (short)cache.currentYFrameArray[y, x];
                }
            }
        }
    }
}