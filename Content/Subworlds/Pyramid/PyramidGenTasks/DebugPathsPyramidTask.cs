using System.Collections.Generic;
using LivingWorldMod.Common.VanillaOverrides.WorldGen.GenShapes;
using LivingWorldMod.Content.Tiles.Interactables;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Content.Subworlds.Pyramid.PyramidGenTasks {
    public class DebugPathsPyramidTask : PyramidSubworld.PyramidGenerationTask {
        public override string StepName => "Debugging Paths";

        public override void DoTask(GenerationProgress progress, GameConfiguration config) {
            if (!LivingWorldMod.IsDebug) {
                return;
            }
            List<PyramidRoom> correctPath = PyramidSubworld.correctPath;

            progress.Message = "Visualizing Paths";

            for (int i = 0; i < correctPath.Count - 1; i++) {
                if (correctPath[i + 1] is null) {
                    break;
                }
                Point firstCenter = correctPath[i].region.Center;
                Point secondCenter = correctPath[i + 1].region.Center;

                WorldUtils.Gen(firstCenter, new StraightLine(2f, secondCenter), Actions.Chain(
                    new Modifiers.SkipTiles((ushort)ModContent.TileType<InnerPyramidDoorTile>(), (ushort)ModContent.TileType<PyramidDoorTile>()),
                    new Modifiers.Offset(0, -1),
                    new Modifiers.SkipTiles((ushort)ModContent.TileType<InnerPyramidDoorTile>(), (ushort)ModContent.TileType<PyramidDoorTile>()),
                    new Modifiers.Offset(0, 1),
                    new Actions.PlaceTile(TileID.LivingCursedFire)));
            }

            foreach (List<PyramidRoom> fakePath in PyramidSubworld.fakePaths) {
                for (int i = 0; i < fakePath.Count - 1; i++) {
                    Point firstCenter = fakePath[i].region.Center;
                    Point secondCenter = fakePath[i + 1].region.Center;

                    WorldUtils.Gen(firstCenter, new StraightLine(2f, secondCenter), Actions.Chain(
                        new Modifiers.SkipTiles((ushort)ModContent.TileType<InnerPyramidDoorTile>(), (ushort)ModContent.TileType<PyramidDoorTile>()),
                        new Modifiers.Offset(0, -1),
                        new Modifiers.SkipTiles((ushort)ModContent.TileType<InnerPyramidDoorTile>(), (ushort)ModContent.TileType<PyramidDoorTile>()),
                        new Modifiers.Offset(0, 1),
                        new Actions.PlaceTile(TileID.LivingFire)));
                }
            }
        }
    }
}