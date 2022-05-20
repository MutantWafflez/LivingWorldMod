using System.Collections.Generic;
using LivingWorldMod.Common.VanillaOverrides.WorldGen.GenShapes;
using LivingWorldMod.Content.Tiles.Interactables;
using LivingWorldMod.Content.Walls.WorldGen;
using Microsoft.Xna.Framework;
using SubworldLibrary;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Content.Subworlds {
    public class PyramidDimension : Subworld {
        public override int Width => 1000;

        public override int Height => 1000;

        public override bool ShouldSave => !LivingWorldMod.IsDebug;

        public override List<GenPass> Tasks => new List<GenPass>() {
            new PassLegacy("Fill World", FillWorld),
            new PassLegacy("Set Spawn", SetSpawn),
            new PassLegacy("Starter Room", GenStarterRoom),
            new PassLegacy("Initial Tunnel", GenInitialTunnels)
        };

        private void FillWorld(GenerationProgress progress, GameConfiguration config) {
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
        }

        private void SetSpawn(GenerationProgress progress, GameConfiguration config) {
            progress.Message = "Spawning up";
            Main.spawnTileX = Width / 2;
            Main.spawnTileY = 82;
        }

        private void GenStarterRoom(GenerationProgress progress, GameConfiguration config) {
            progress.Message = "Starter Room";

            //Generate initial triangular room
            WorldUtils.Gen(new Point(Width / 2 - 1, 72), new EqualTriangle(20), new Actions.ClearTile(true));

            //Generate exit door
            WorldGen.PlaceObject(Width / 2 - 2, 78, ModContent.TileType<PyramidDoorTile>(), style: 0, direction: 1);

            //Generate Torches
            WorldGen.PlaceTile(Width / 2 - 3, 77, TileID.Torches, forced: true, style: 16);
            WorldGen.PlaceTile(Width / 2 + 2, 77, TileID.Torches, forced: true, style: 16);
        }

        private void GenInitialTunnels(GenerationProgress progress, GameConfiguration config) {
            progress.Message = "Tunnelin'";
            WorldUtils.Gen(new Point(Width / 2 - 7, 81), new StraightLine(5, new Vector2(-10, 10)), new Actions.ClearTile(true));
        }
    }
}