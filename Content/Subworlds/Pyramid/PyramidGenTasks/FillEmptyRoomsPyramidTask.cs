using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Content.Subworlds.Pyramid.PyramidGenTasks {
    public class FillEmptyRoomsPyramidTask : PyramidSubworld.PyramidGenerationTask {
        public override string StepName => "Fill Empty Rooms";

        public override void DoTask(GenerationProgress progress, GameConfiguration config) {
            const int gridSideLength = PyramidSubworld.GridSideLength;

            progress.Message = "Filling the Empty Space";

            for (int i = 0; i < gridSideLength; i++) {
                progress.Set(i / (float)gridSideLength);
                List<PyramidRoom> column = PyramidSubworld.grid.GetRoomColumn(i);

                foreach (PyramidRoom room in column) {
                    Rectangle roomRegion = room.region;

                    for (int x = roomRegion.X; x <= roomRegion.X + roomRegion.Width; x++) {
                        for (int y = roomRegion.Y; y <= roomRegion.Y + roomRegion.Height; y++) {
                            Tile tile = Framing.GetTileSafely(x, y);
                            if (!room.pathSearched) {
                                tile.HasTile = true;
                                tile.TileType = TileID.SandStoneSlab;
                            }
                        }
                    }
                }
            }
        }
    }
}