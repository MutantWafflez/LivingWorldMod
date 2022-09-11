using System;
using System.Collections.Generic;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Terraria.Utilities;

namespace LivingWorldMod.Custom.Classes {
    /// <summary>
    /// Class that handles a 2D grid of rooms for the Revamped Pyramid dungeon.
    /// </summary>
    public class PyramidRoomGrid {
        private int _gridSideLength;
        private int _roomSideLength;
        private int _roomPadding;
        private PyramidRoom[][] _fixedGrid;
        private List<PyramidRoom>[] _roomList;
        private UnifiedRandom _randomNumberGenerator;

        public PyramidRoomGrid(int gridSideLength, int roomSideLength, int roomPadding, UnifiedRandom randomNumberGenerator) {
            _gridSideLength = gridSideLength;
            _roomSideLength = roomSideLength;
            _roomPadding = roomPadding;
            _randomNumberGenerator = randomNumberGenerator;
            _roomList = new List<PyramidRoom>[gridSideLength];
            _fixedGrid = new PyramidRoom[_gridSideLength][];
        }

        /// <summary>
        /// Generates and overrides any previous grid on this object with the specifications passed in the constructor.
        /// </summary>
        public void GenerateGrid() {
            Array.Clear(_fixedGrid);
            bool[][] takenGridSpots = new bool[_gridSideLength][];
            for (int i = 0; i < _gridSideLength; i++) {
                _fixedGrid[i] = new PyramidRoom[_gridSideLength];
                takenGridSpots[i] = new bool[_gridSideLength];
            }

            Array.Clear(_roomList);
            for (int i = 0; i < _gridSideLength; i++) {
                _roomList[i] = new List<PyramidRoom>();
                //The top room will always be 1x1
                PyramidRoom topRoom = new PyramidRoom(new Rectangle(_roomPadding + i * _roomSideLength + 1, _roomPadding + 1, _roomSideLength, _roomSideLength), i, 0, 1, 1);
                _roomList[i].Add(topRoom);
                _fixedGrid[i][0] = topRoom;
                takenGridSpots[i][0] = true;

                int currentHeight = 1;
                while (currentHeight < _gridSideLength) {
                    if (takenGridSpots[i][currentHeight]) {
                        currentHeight++;
                        continue;
                    }

                    List<Tuple<Tuple<int, int>, double>> roomChoices = new List<Tuple<Tuple<int, int>, double>>();
                    roomChoices.Add(new Tuple<Tuple<int, int>, double>(new Tuple<int, int>(1, 1), 75)); // 1x1 room
                    roomChoices.AddConditionally(new Tuple<Tuple<int, int>, double>(new Tuple<int, int>(1, 2), 8.34), currentHeight < _gridSideLength - 1 && !takenGridSpots[i][currentHeight + 1]); //1x2 room
                    roomChoices.AddConditionally(new Tuple<Tuple<int, int>, double>(new Tuple<int, int>(2, 1), 8.34), i < _gridSideLength - 1); //2x1 room
                    roomChoices.AddConditionally(new Tuple<Tuple<int, int>, double>(new Tuple<int, int>(2, 2), 8.34), i < _gridSideLength - 1 && currentHeight < _gridSideLength - 1 && !takenGridSpots[i][currentHeight + 1]); //2x2 room

                    (int roomWidth, int roomHeight) = new WeightedRandom<Tuple<int, int>>(_randomNumberGenerator, roomChoices.ToArray()).Get();
                    PyramidRoom newRoom = new PyramidRoom(
                        new Rectangle(_roomPadding + i * _roomSideLength + 1, _roomPadding + currentHeight * _roomSideLength + 1, _roomSideLength * roomWidth, _roomSideLength * roomHeight),
                        i,
                        currentHeight,
                        roomWidth,
                        roomHeight);

                    _roomList[i].Add(newRoom);

                    for (int x = 0; x < roomWidth; x++) {
                        for (int y = 0; y < roomHeight; y++) {
                            takenGridSpots[i + x][currentHeight + y] = true;
                            _fixedGrid[i + x][currentHeight + y] = newRoom;
                        }
                    }

                    currentHeight += roomHeight;
                }
            }
        }

        /// <summary>
        /// Gets the room below the passed in room, from the bottom-left tile of this room.
        /// Returns null if going below is out of bounds.
        /// </summary>
        public PyramidRoom GetRoomBelow(PyramidRoom room) => GetRoom(room.gridTopLeftX, room.gridTopLeftY + room.gridHeight);

        /// <summary>
        /// Gets the room to the left of the passed in room, from the bottom-left tile of this room.
        /// Returns null if going to the left is out of bounds.
        /// </summary>
        public PyramidRoom GetRoomToLeft(PyramidRoom room) => GetRoom(room.gridTopLeftX - 1, room.gridTopLeftY + room.gridHeight - 1);

        /// <summary>
        /// Gets the room to the right of the passed in room, from the bottom-left tile of this room.
        /// Returns null if going to the right is out of bounds.
        /// </summary>
        public PyramidRoom GetRoomToRight(PyramidRoom room) => GetRoom(room.gridTopLeftX + room.gridWidth, room.gridTopLeftY + room.gridHeight - 1);

        /// <summary>
        /// Returns the room that exists at the specified coordinates within the grid. If the grid location is taken by
        /// a piece of a multi-tile room, this will return the room object that is found at the top left.
        /// Returns null if out of bounds.
        /// </summary>
        public PyramidRoom GetRoom(int i, int j) => i >= 0 && i < _gridSideLength && j >= 0 && j < _gridSideLength ? _fixedGrid[i][j] : null;

        /// <summary>
        /// Returns the entire list of rooms that pertains to the passed in column number.
        /// </summary>
        public List<PyramidRoom> GetRoomColumn(int i) => _roomList[i];
    }
}