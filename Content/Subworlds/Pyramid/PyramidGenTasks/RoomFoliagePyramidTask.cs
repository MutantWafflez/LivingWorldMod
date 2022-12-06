using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Common.VanillaOverrides.WorldGen.GenConditions;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ObjectData;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Content.Subworlds.Pyramid.PyramidGenTasks {
    public class RoomFoliagePyramidTask : PyramidSubworld.PyramidGenerationTask {
        public override string StepName => "Room Foliage";

        public override void DoTask(GenerationProgress progress, GameConfiguration config) {
            progress.Message = "Uncovering Lost Remains";

            //Generate torches in all rooms
            List<List<PyramidRoom>> allPaths = PyramidSubworld.fakePaths.Prepend(PyramidSubworld.correctPath).ToList();
            for (int i = 0; i < allPaths.Count; i++) {
                progress.Set(i / (allPaths.Count - 1f));

                GenerateRoomFoliageOnPath(allPaths[i]);
            }
        }

        /// <summary>
        /// Generates "foliage" in the rooms on the passed in path.
        /// </summary>
        private void GenerateRoomFoliageOnPath(List<PyramidRoom> path) {
            for (int i = 0; i < path.Count; i++) {
                PyramidRoom room = path[i];

                Rectangle roomRegion = room.region;
                if (room.generationStep >= PyramidSubworld.PyramidRoomGenerationStep.FoliageGenerated) {
                    continue;
                }
                room.generationStep = PyramidSubworld.PyramidRoomGenerationStep.FoliageGenerated;

                TileObjectData pileData = TileObjectData.GetTileData(TileID.LargePiles, 0);
                List<Point> pileLocations = new();

                //Search through entire room, looking for valid pile placements
                for (int x = roomRegion.X; x < roomRegion.X + roomRegion.Width; x++) {
                    for (int y = roomRegion.Y; y <= roomRegion.Y + roomRegion.Height; y++) {
                        Point searchPoint = new(x, y);

                        if (TileObject.CanPlace(x, y, TileID.LargePiles, 0, -1, out _) && WorldUtils.Find(searchPoint, Searches.Chain(new Searches.Down(1), new IsDry().AreaAnd(pileData.Width, pileData.Height)), out _)) {
                            pileLocations.Add(searchPoint);
                        }
                    }
                }

                foreach (Point pileLocation in pileLocations) {
                    //20% chance to place by default
                    if (!WorldGen.genRand.NextBool(5)) {
                        continue;
                    }
                    Dictionary<ushort, int> tileData = new();

                    //Check if there is already any piles nearby; if not, place!
                    WorldUtils.Gen(pileLocation + new Point(1, 0), new Shapes.Circle(28), new Actions.TileScanner(TileID.LargePiles).Output(tileData));
                    if (tileData[TileID.LargePiles] == 0) {
                        WorldGen.PlaceObject(pileLocation.X, pileLocation.Y, TileID.LargePiles, style: WorldGen.genRand.Next(6));
                    }
                }
            }
        }
    }
}