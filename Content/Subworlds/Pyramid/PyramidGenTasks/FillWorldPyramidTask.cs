using LivingWorldMod.Content.Walls.WorldGen;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Content.Subworlds.Pyramid.PyramidGenTasks {
    public class FillWorldPyramidTask : PyramidSubworld.PyramidGenerationTask {
        public override string StepName => "Fill World";

        public override void DoTask(GenerationProgress progress, GameConfiguration config) {
            const int worldBorderPadding = PyramidSubworld.WorldBorderPadding;

            progress.Message = "Wall Slabs";
            for (int i = 0; i < Main.maxTilesX; i++) {
                for (int j = 0; j < Main.maxTilesY; j++) {
                    progress.Set((j + i * Main.maxTilesY) / (float)(Main.maxTilesX * Main.maxTilesY));
                    Tile tile = Framing.GetTileSafely(i, j);
                    tile.WallType = (ushort)ModContent.WallType<PyramidBrickWall>();
                    if (i <= worldBorderPadding || i > PyramidSubworld.Width - worldBorderPadding || j <= worldBorderPadding || j > PyramidSubworld.Height - PyramidSubworld.BossRoomPadding - worldBorderPadding) {
                        tile.HasTile = true;
                        tile.TileType = TileID.SandStoneSlab;
                    }
                }
            }
        }
    }
}