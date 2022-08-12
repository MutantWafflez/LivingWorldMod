using System;
using System.Collections.Generic;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Utilities;

namespace LivingWorldMod.Custom.Classes {
    /// <summary>
    /// Class that handles a 2D grid of rooms for the Revamped Pyramid dungeon.
    /// </summary>
    public class RoomGrid {

        private int _gridSideLength;
        private int _roomSideLength;
        private int _roomPadding;
        private List<Rectangle>[] _roomGrid;

        public Rectangle this[int i, int j] => _roomGrid[i][j];

        public RoomGrid(int gridSideLength, int roomSideLength, int roomPadding) {
            _gridSideLength = gridSideLength;
            _roomSideLength = roomSideLength;
            _roomPadding = roomPadding;
            _roomGrid = new List<Rectangle>[gridSideLength];
        }

        public void GenerateGrid() {
            bool[][] takenGridSpots = new bool[_gridSideLength][];
            for (int i = 0; i < _gridSideLength; i++) {
                takenGridSpots[i] = new bool[_gridSideLength];
            }

            Array.Clear(_roomGrid);
            for (int i = 0; i < _gridSideLength; i++) {
                _roomGrid[i] = new List<Rectangle>();
                //The top room will always be 1x1
                _roomGrid[i].Add(new Rectangle(_roomPadding + i * _roomSideLength, _roomPadding, _roomSideLength, _roomSideLength));
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
                    roomChoices.AddConditionally(new Tuple<Tuple<int, int>, double>(new Tuple<int, int>(2, 2), 8.34),  i < _gridSideLength - 1 && currentHeight < _gridSideLength - 1 && !takenGridSpots[i][currentHeight + 1]); //2x2 room
                    
                    (int roomWidth, int roomHeight) = new WeightedRandom<Tuple<int, int>>(WorldGen._genRand, roomChoices.ToArray()).Get();
                    for (int x = 0; x < roomWidth; x++) {
                        for (int y = 0; y < roomHeight; y++) {
                            takenGridSpots[i + x][currentHeight + y] = true;
                        }
                    }

                    _roomGrid[i].Add(new Rectangle(_roomPadding + i * _roomSideLength, _roomPadding + currentHeight * _roomSideLength, _roomSideLength * roomWidth, _roomSideLength * roomHeight));
                    currentHeight += roomHeight;
                }
            }
        }

        /// <summary>
        /// Returns the length/height of the specified column number of the grid.
        /// </summary>
        /// <param name="i"> The column to retrieve the length/height of. </param>
        public int ColumnLength(int i) => _roomGrid[i].Count;
    }
}
