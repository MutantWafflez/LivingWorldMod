using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.Tiles.Interactables;
using LivingWorldMod.Custom.Structs;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Content.Subworlds.Pyramid.PyramidGenTasks {
    public class RoomLayoutsPyramidTask : PyramidSubworld.PyramidGenerationTask {
        public override string StepName => "Room Layouts";

        public override void DoTask(GenerationProgress progress, GameConfiguration config) {
            progress.Message = "Shifting the Rooms";

            //Manually generate starter room
            PyramidRoom startRoom = PyramidSubworld.correctPath.First();
            RoomData startRoomData = IOUtils.GetTagFromFile<RoomData>(LivingWorldMod.LWMStructurePath + "PyramidRooms/StartRoom.pyrroom");
            Rectangle roomRegion = startRoom.region;

            WorldGenUtils.GenerateStructure(startRoomData.roomLayout, roomRegion.X, roomRegion.Y);
            WorldGen.PlaceObject(roomRegion.X + startRoomData.doorData[PyramidDoorDirection.Top].X, roomRegion.Y + startRoomData.doorData[PyramidDoorDirection.Top].Y, ModContent.TileType<PyramidDoorTile>());

            foreach ((PyramidDoorDirection key, PyramidDoorData value) in startRoom.doorData) {
                Point16 doorPos = new(startRoom.region.X + startRoomData.doorData[key].X, startRoom.region.Y + startRoomData.doorData[key].Y);

                value.doorPos = doorPos;
                WorldGen.PlaceObject(doorPos.X, doorPos.Y, ModContent.TileType<InnerPyramidDoorTile>());
            }

            startRoom.generationStep = PyramidSubworld.PyramidRoomGenerationStep.LayoutGenerated;

            //Generate ALL the rooms
            List<List<PyramidRoom>> allPaths = PyramidSubworld.fakePaths.Prepend(PyramidSubworld.correctPath).ToList();
            for (int i = 0; i < allPaths.Count; i++) {
                progress.Set(i / (allPaths.Count - 1f));

                GenerateRoomLayoutOnPath(allPaths[i]);
            }
        }

        /// <summary>
        /// Generates all the room layouts in the passed in path.
        /// </summary>
        private void GenerateRoomLayoutOnPath(List<PyramidRoom> path) {
            for (int i = 0; i < path.Count; i++) {
                PyramidRoom room = path[i];

                Rectangle roomRegion = room.region;
                if (room.generationStep >= PyramidSubworld.PyramidRoomGenerationStep.LayoutGenerated) {
                    continue;
                }
                room.generationStep = PyramidSubworld.PyramidRoomGenerationStep.LayoutGenerated;

                string roomDimensions = $"{room.gridWidth}x{room.gridHeight}";
                RoomData roomData = WorldGen.genRand.Next(PyramidSubworld.allRooms[roomDimensions]);

                //Generate Layout
                WorldGenUtils.GenerateStructure(roomData.roomLayout, roomRegion.X, roomRegion.Y, false);

                //Initialize doors and place them
                foreach ((PyramidDoorDirection key, PyramidDoorData value) in room.doorData) {
                    Point16 doorPos = new(room.region.X + roomData.doorData[key].X, room.region.Y + roomData.doorData[key].Y);

                    value.doorPos = doorPos;
                    WorldGen.PlaceObject(doorPos.X, doorPos.Y, ModContent.TileType<InnerPyramidDoorTile>());
                }
            }
        }
    }
}