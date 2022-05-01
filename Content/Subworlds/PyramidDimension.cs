using System.Collections.Generic;
using LivingWorldMod.Content.Walls.WorldGen;
using SubworldLibrary;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Content.Subworlds {
    public class PyramidDimension : Subworld {
        public override int Width => 1000;

        public override int Height => 1000;

        public override bool ShouldSave => true;

        public override List<GenPass> Tasks => new List<GenPass>() {
            new PassLegacy("Fill World", (progress, configuration) => {
                progress.Message = "Slabbing up";
                for (int i = 0; i < Main.maxTilesX; i++) {
                    for (int j = 0; j < Main.maxTilesY; j++) {
                        progress.Set((j + i * Main.maxTilesY) / (float)(Main.maxTilesX * Main.maxTilesY));
                        Tile tileInQuestion = Framing.GetTileSafely(i, j);
                        tileInQuestion.HasTile = true;
                        tileInQuestion.TileType = TileID.SandStoneSlab;
                        tileInQuestion.WallType = (ushort)ModContent.WallType<PyramidBrickWall>();
                    }
                }
            })
        };

        public override void OnLoad() { }
    }
}