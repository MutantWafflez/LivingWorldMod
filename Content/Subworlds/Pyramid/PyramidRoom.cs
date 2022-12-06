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
        /// Whether or not this room is currently active for updating purposes such as curses and enemy spawning.
        /// </summary>
        public bool IsActive {
            get {
                bool playerInside = false;

                for (int i = 0; i < Main.maxPlayers; i++) {
                    if (new Rectangle(region.X * 16, region.Y * 16, region.Width * 16, region.Height * 16).Contains(Main.player[i].Center.ToPoint())) {
                        playerInside = true;

                        break;
                    }
                }

                return playerInside || roomCleared;
            }
        }

        /// <summary>
        /// A list of all ACTIVE curses in this room. If this room is cleared by the player or otherwise not active, returns
        /// an empty list.
        /// </summary>
        public List<PyramidRoomCurseType> ActiveCurses => IsActive ? _roomCurses : new List<PyramidRoomCurseType>();

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
        /// Whether or not the room has been cleared, allowing usage of the doors.
        /// </summary>
        public bool roomCleared;

        /// <summary>
        /// A list of all curses that apply to this room.
        /// </summary>
        private readonly List<PyramidRoomCurseType> _roomCurses = new();

        /// <summary>
        /// Dictionary of integer values that pertain to timers of certain curses.
        /// </summary>
        private readonly Dictionary<PyramidRoomCurseType, int> _curseTimers;

        public PyramidRoom(Rectangle region, int gridTopLeftX, int gridTopLeftY, int gridWidth, int gridHeight) {
            this.region = region;
            this.gridTopLeftX = gridTopLeftX;
            this.gridTopLeftY = gridTopLeftY;
            this.gridWidth = gridWidth;
            this.gridHeight = gridHeight;

            roomType = LivingWorldMod.IsDebug && ModContent.GetInstance<DebugConfig>().allCursedRooms ? PyramidRoomType.Cursed : RoomSelector;
            _curseTimers = new Dictionary<PyramidRoomCurseType, int> {
                { PyramidRoomCurseType.Rotation, 0 }
            };
        }

        public override string ToString() => "{" + gridTopLeftX + ", " + gridTopLeftY + "} " + $"{gridWidth}x{gridHeight}";

        /// <summary>
        /// Called at the end of every update tick for each room in the subworld.
        /// </summary>
        public void Update() {
            foreach (PyramidRoomCurseType curse in ActiveCurses) {
                switch (curse) {
                    case PyramidRoomCurseType.Rotation:
                        if (Main.netMode != NetmodeID.MultiplayerClient && ++_curseTimers[PyramidRoomCurseType.Rotation] >= 60 * 15) {
                            _curseTimers[PyramidRoomCurseType.Rotation] = 0;

                            int removalCount = ActiveCurses.RemoveAll(innerCurse => innerCurse != PyramidRoomCurseType.Rotation);
                            for (int i = 0; i < removalCount - 1; i++) {
                                AddRandomCurse(false);
                            }

                            AddRandomCurse();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Adds a random, non-repeating curse to this room. Does nothing on MP clients. If on the server, syncs to all clients
        /// automatically, unless specified.
        /// </summary>
        public void AddRandomCurse(bool syncToClients = true) {
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                return;
            }

            _roomCurses.Add(WorldGen.genRand.Next(Enum.GetValues<PyramidRoomCurseType>().Where(curse => !ActiveCurses.Contains(curse)).ToList()));

            if (Main.netMode != NetmodeID.Server || !syncToClients) {
                return;
            }

            ModPacket packet = ModContent.GetInstance<PyramidDungeonPacketHandler>().GetPacket(PyramidDungeonPacketHandler.SyncRoomCurses);
            packet.Write(gridTopLeftX);
            packet.Write(gridTopLeftY);
            packet.Write(ActiveCurses.Count);
            foreach (PyramidRoomCurseType curse in ActiveCurses) {
                packet.Write((int)curse);
            }

            packet.Send();
        }

        /// <summary>
        /// Applies curse effects that are "one time" effects, such as generation effects
        /// of the Curse of Flooding.
        /// </summary>
        public void ApplyOneTimeCurseEffects() {
            foreach (PyramidRoomCurseType curse in ActiveCurses) {
                switch (curse) {
                    case PyramidRoomCurseType.Flooding:
                        int roomHalfY = region.Y + region.Height / 2;

                        for (int y = roomHalfY; y <= region.Bottom; y++) {
                            for (int x = region.X; x <= region.Right; x++) {
                                Tile tile = Main.tile[x, y];
                                tile.LiquidType = LiquidID.Water;
                                tile.LiquidAmount = byte.MaxValue;
                            }
                        }

                        if (Main.netMode == NetmodeID.Server) {
                            NetMessage.SendTileSquare(-1, region.X, roomHalfY, region.Width, region.Height / 2);
                        }
                        break;
                }
            }
        }
    }
}