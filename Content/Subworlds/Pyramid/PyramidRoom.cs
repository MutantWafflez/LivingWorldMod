﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Utilities;

namespace LivingWorldMod.Content.Subworlds.Pyramid {
    /// <summary>
    /// Small class that exists for data storage on a given pyramid room.
    /// </summary>
    public sealed class PyramidRoom {
        /// <summary>
        /// The rectangle that denotes the actual tile position and width/length.
        /// </summary>
        public Rectangle region;

        /// <summary>
        /// Dictionary that holds data on each door in this room. If a key doesn't exist for one of the values, that door does not
        /// exist in this room.
        /// </summary>
        public Dictionary<PyramidDoorDirection, PyramidDoorData> doorData = new Dictionary<PyramidDoorDirection, PyramidDoorData>() { };

        /// <summary>
        /// The top left position's X value of this room in terms of the GRID, not tiles.
        /// </summary>
        public int gridTopLeftX;

        /// <summary>
        /// The top left position's Y value of this room in terms of the GRID, not tiles.
        /// </summary>
        public int gridTopLeftY;

        /// <summary>
        /// How wide this room is in terms of the GRID, not tiles.
        /// </summary>
        public int gridWidth;

        /// <summary>
        /// How tall this room is in terms of the GRID, not tiles.
        /// </summary>
        public int gridHeight;

        /// <summary>
        /// Whether or not this room has been searched in the process of creating the correct path for the dungeon.
        /// </summary>
        public bool pathSearched;

        /// <summary>
        /// The current progress of this specific room in the generation process.
        /// </summary>
        public PyramidRoomGenerationStep generationStep;

        /// <summary>
        /// This room's type.
        /// </summary>
        public PyramidRoomType roomType;

        public PyramidRoom(Rectangle region, int gridTopLeftX, int gridTopLeftY, int gridWidth, int gridHeight) {
            this.region = region;
            this.gridTopLeftX = gridTopLeftX;
            this.gridTopLeftY = gridTopLeftY;
            this.gridWidth = gridWidth;
            this.gridHeight = gridHeight;

            roomType = new WeightedRandom<PyramidRoomType>(WorldGen.genRand, new[] {
                new Tuple<PyramidRoomType, double>(PyramidRoomType.Normal, 50),
                new Tuple<PyramidRoomType, double>(PyramidRoomType.Cursed, 50 / 3f),
                new Tuple<PyramidRoomType, double>(PyramidRoomType.Puzzle, 50 / 3f),
                new Tuple<PyramidRoomType, double>(PyramidRoomType.Treasure, 50 / 3f)
            });
        }

        public override string ToString() => "{" + gridTopLeftX + ", " + gridTopLeftY + "} " + $"{gridWidth}x{gridHeight}";
    }
}