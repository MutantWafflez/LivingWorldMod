using System;
using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Common.Configs;
using LivingWorldMod.Core.PacketHandlers;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace LivingWorldMod.Content.Subworlds.Pyramid {
    /// <summary>
    /// Small class that exists for data storage on a given pyramid room.
    /// </summary>
    public sealed class PyramidRoom {
        public static readonly WeightedRandom<PyramidRoomType> RoomSelector = new(WorldGen.genRand, new Tuple<PyramidRoomType, double>(PyramidRoomType.Normal, 50), new Tuple<PyramidRoomType, double>(PyramidRoomType.Cursed, 50 / 3f),
            new Tuple<PyramidRoomType, double>(PyramidRoomType.Puzzle, 50 / 3f), new Tuple<PyramidRoomType, double>(PyramidRoomType.Treasure, 50 / 3f));

        /// <summary>
        /// The rectangle that denotes the actual tile position and width/length.
        /// </summary>
        public Rectangle region;

        /// <summary>
        /// Dictionary that holds data on each door in this room. If a key doesn't exist for one of the values, that door does not
        /// exist in this room.
        /// </summary>
        public Dictionary<PyramidDoorDirection, PyramidDoorData> doorData = new();

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
        public PyramidSubworld.PyramidRoomGenerationStep generationStep;

        /// <summary>
        /// This room's type.
        /// </summary>
        public PyramidRoomType roomType;

        /// <summary>
        /// A list of all curses affecting this room.
        /// </summary>
        public List<PyramidRoomCurseType> roomCurses = new();

        public PyramidRoom(Rectangle region, int gridTopLeftX, int gridTopLeftY, int gridWidth, int gridHeight) {
            this.region = region;
            this.gridTopLeftX = gridTopLeftX;
            this.gridTopLeftY = gridTopLeftY;
            this.gridWidth = gridWidth;
            this.gridHeight = gridHeight;

            roomType = LivingWorldMod.IsDebug && ModContent.GetInstance<DebugConfig>().allCursedRooms ? PyramidRoomType.Cursed : RoomSelector;
        }

        public override string ToString() => "{" + gridTopLeftX + ", " + gridTopLeftY + "} " + $"{gridWidth}x{gridHeight}";

        /// <summary>
        /// Adds a random, non-repeating curse to this room. Does nothing on MP clients. If on the server, syncs to all clients
        /// automatically, unless specified.
        /// </summary>
        public void AddRandomCurse(bool syncToClients = true) {
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                return;
            }

            roomCurses.Add(WorldGen.genRand.Next(Enum.GetValues<PyramidRoomCurseType>().Where(curse => !roomCurses.Contains(curse)).ToList()));

            if (Main.netMode != NetmodeID.Server || !syncToClients) {
                return;
            }

            ModPacket packet = ModContent.GetInstance<PyramidDungeonPacketHandler>().GetPacket(PyramidDungeonPacketHandler.SyncRoomCurses);
            packet.Write(gridTopLeftX);
            packet.Write(gridTopLeftY);
            packet.Write(roomCurses.Count);
            foreach (PyramidRoomCurseType curse in roomCurses) {
                packet.Write((int)curse);
            }

            packet.Send();
        }
    }
}