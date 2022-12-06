using System;
using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.IO;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Content.Subworlds.Pyramid.PyramidGenTasks {
    public class InitializePyramidTask : PyramidSubworld.PyramidGenerationTask {
        public override string StepName => "Initialize";

        public override void DoTask(GenerationProgress progress, GameConfiguration config) {
            progress.Message = "Initializing";
            ref PyramidRoomGrid grid = ref PyramidSubworld.grid;
            ref List<PyramidRoom> correctPath = ref PyramidSubworld.correctPath;
            ref int spawnTileX = ref PyramidSubworld.spawnTileX;
            ref int spawnTileY = ref PyramidSubworld.spawnTileY;
            ref List<List<PyramidRoom>> fakePaths = ref PyramidSubworld.fakePaths;

            //Default spawn spot, in case generation goes awry
            spawnTileX = 225;
            spawnTileY = 245;

            //Generate grid
            grid = new PyramidRoomGrid(PyramidSubworld.GridSideLength, PyramidSubworld.RoomSideLength, PyramidSubworld.WorldBorderPadding);
            grid.GenerateGrid();

            //Generate correct path first
            correctPath = new List<PyramidRoom> { grid.GetRoom(WorldGen.genRand.Next(PyramidSubworld.GridSideLength), 0) };
            PyramidRoom currentRoom = correctPath.First();
            //Starter room will always be a normal room
            currentRoom.roomType = PyramidRoomType.Normal;
            currentRoom.pathSearched = true;

            while (currentRoom is not null) {
                PyramidRoom roomBelow = grid.GetRoomBelow(currentRoom);
                PyramidRoom roomLeft = grid.GetRoomToLeft(currentRoom);
                PyramidRoom roomRight = grid.GetRoomToRight(currentRoom);

                List<Tuple<PyramidRoom, double>> movementChoices = new();
                movementChoices.Add(new Tuple<PyramidRoom, double>(roomBelow, 34)); //Down
                movementChoices.AddConditionally(new Tuple<PyramidRoom, double>(roomLeft, 33), roomLeft is { pathSearched: false } && !currentRoom.doorData.ContainsKey(PyramidDoorDirection.Left)); //Left
                movementChoices.AddConditionally(new Tuple<PyramidRoom, double>(roomRight, 33), roomRight is { pathSearched: false } && !currentRoom.doorData.ContainsKey(PyramidDoorDirection.Right)); //Right

                PyramidRoom selectedRoom = new WeightedRandom<PyramidRoom>(WorldGen.genRand, movementChoices.ToArray()).Get();
                if (selectedRoom is not null) {
                    correctPath.Add(selectedRoom);
                    selectedRoom.pathSearched = true;

                    //Link doors accordingly
                    PyramidDoorDirection currentDoorDirection = PyramidDoorDirection.Top;
                    if (selectedRoom == roomBelow) {
                        currentDoorDirection = PyramidDoorDirection.Down;
                    }
                    else if (selectedRoom == roomLeft) {
                        currentDoorDirection = PyramidDoorDirection.Left;
                    }
                    else if (selectedRoom == roomRight) {
                        currentDoorDirection = PyramidDoorDirection.Right;
                    }

                    PyramidSubworld.ConnectDoors(currentRoom, selectedRoom, currentDoorDirection, PyramidSubworld.GetOppositeDirection(currentDoorDirection));
                }

                currentRoom = selectedRoom;
            }

            //Move spawn point accordingly
            PyramidRoom starterRoom = correctPath.First();
            spawnTileX = starterRoom.region.Center.X;
            spawnTileY = starterRoom.region.Center.Y;

            //Generate Fake paths along the correct path
            fakePaths = new List<List<PyramidRoom>>();
            GenerateFakePath(correctPath, 5, 50);
            int originalCount = fakePaths.Count;
            for (int i = 0; i < originalCount; i++) {
                GenerateFakePath(fakePaths[i], 8, 30);
            }
        }

        /// <summary>
        /// Scans the passed in path and randomly generates more fake paths branching off of it.
        /// Make sure the fake path list is properly initialized before calling this.
        /// </summary>
        /// <param name="pathToSearchAlong"> The path this method will search along to potentially branch off of. </param>
        /// <param name="branchOccurrenceDenominator">
        /// The starting value of 1/value, determining whether or not a new path will be created. Every time the RNG fails,
        /// this value will decrease by 1 until success, when it is reset to this value.
        /// </param>
        /// <param name="branchEndChanceDenominator">
        /// The starting value of 1/value, determining whether or not a path generating will end. Every time the RNG fails,
        /// this value will decrease by 5 until success, when it is reset to this value.
        /// </param>
        private void GenerateFakePath(List<PyramidRoom> pathToSearchAlong, int branchOccurrenceDenominator, int branchEndChanceDenominator) {
            ref PyramidRoomGrid grid = ref PyramidSubworld.grid;
            ref List<List<PyramidRoom>> fakePaths = ref PyramidSubworld.fakePaths;

            int branchChanceDenominator = branchOccurrenceDenominator;

            foreach (PyramidRoom originalPathRoom in pathToSearchAlong) {
                if (originalPathRoom is null || grid.GetRoomBelow(originalPathRoom) is { pathSearched: true } && grid.GetRoomToLeft(originalPathRoom) is { pathSearched: true } && grid.GetRoomToRight(originalPathRoom) is { pathSearched: true }) {
                    continue;
                }

                if (WorldGen.genRand.NextBool(branchChanceDenominator)) {
                    branchChanceDenominator = branchOccurrenceDenominator;
                    int endChanceDenominator = branchEndChanceDenominator;

                    List<PyramidRoom> newPath = new() { originalPathRoom };
                    PyramidRoom currentFakeRoom = newPath[0];

                    while (true) {
                        PyramidRoom roomBelow = grid.GetRoomBelow(currentFakeRoom);
                        PyramidRoom roomLeft = grid.GetRoomToLeft(currentFakeRoom);
                        PyramidRoom roomRight = grid.GetRoomToRight(currentFakeRoom);

                        List<Tuple<PyramidRoom, double>> movementChoices = new();
                        movementChoices.AddConditionally(new Tuple<PyramidRoom, double>(roomBelow, 25), roomBelow is { pathSearched: false }); //Down
                        movementChoices.AddConditionally(new Tuple<PyramidRoom, double>(roomLeft, 37.5), roomLeft is { pathSearched: false } && !currentFakeRoom.doorData.ContainsKey(PyramidDoorDirection.Left)); //Left
                        movementChoices.AddConditionally(new Tuple<PyramidRoom, double>(roomRight, 37.5), roomRight is { pathSearched: false } && !currentFakeRoom.doorData.ContainsKey(PyramidDoorDirection.Right)); //Right
                        if (!movementChoices.Any()) {
                            break;
                        }

                        PyramidRoom selectedRoom = new WeightedRandom<PyramidRoom>(WorldGen.genRand, movementChoices.ToArray()).Get();
                        if (WorldGen.genRand.NextBool(endChanceDenominator) || selectedRoom is null || selectedRoom.pathSearched) {
                            break;
                        }

                        //Link doors accordingly
                        PyramidDoorDirection currentDoorDirection = PyramidDoorDirection.Top;
                        if (selectedRoom == roomBelow) {
                            if (currentFakeRoom.doorData.ContainsKey(PyramidDoorDirection.Down)) {
                                break;
                            }

                            currentDoorDirection = PyramidDoorDirection.Down;
                        }
                        else if (selectedRoom == roomLeft) {
                            if (currentFakeRoom.doorData.ContainsKey(PyramidDoorDirection.Left)) {
                                break;
                            }

                            currentDoorDirection = PyramidDoorDirection.Left;
                        }
                        else if (selectedRoom == roomRight) {
                            if (currentFakeRoom.doorData.ContainsKey(PyramidDoorDirection.Right)) {
                                break;
                            }

                            currentDoorDirection = PyramidDoorDirection.Right;
                        }
                        PyramidSubworld.ConnectDoors(currentFakeRoom, selectedRoom, currentDoorDirection, PyramidSubworld.GetOppositeDirection(currentDoorDirection));

                        newPath.Add(selectedRoom);
                        selectedRoom.pathSearched = true;

                        endChanceDenominator = (int)MathHelper.Clamp(endChanceDenominator - 5, 1, branchEndChanceDenominator);
                        currentFakeRoom = selectedRoom;
                    }

                    if (newPath.Count > 1) {
                        fakePaths.Add(newPath);
                    }
                }
                else {
                    branchChanceDenominator = (int)MathHelper.Clamp(branchChanceDenominator - 1, 1, branchChanceDenominator);
                }
            }
        }
    }
}