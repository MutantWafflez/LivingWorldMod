using System;
using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Common.Configs;
using LivingWorldMod.Common.Systems.SubworldSystems;
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

        //TODO: Uncomment this once all curses are implemented
        /*
        /// <summary>
        /// List of curses that cannot be added after the room is initially generated.
        /// </summary>
        public static readonly IReadOnlyList<PyramidRoomCurseType> CannotBeAddedPostGeneration = new List<PyramidRoomCurseType> {
            PyramidRoomCurseType.Battle,
            PyramidRoomCurseType.Siege,
            PyramidRoomCurseType.Flooding,
            PyramidRoomCurseType.UnsteadyFooting
        };
        */

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
        public List<PyramidRoomCurseType> ActiveCurses => IsActive ? internalRoomCurses : new List<PyramidRoomCurseType>();

        /// <summary>
        /// A list of all curses that apply to this room, regardless if the room is
        /// active or not.
        /// </summary>
        public readonly List<PyramidRoomCurseType> internalRoomCurses = new();

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

                            int removalCount = internalRoomCurses.RemoveAll(innerCurse => innerCurse != PyramidRoomCurseType.Rotation);
                            PyramidDungeonSystem.Instance.PurgeTorchList();

                            for (int i = 0; i < removalCount - 1; i++) {
                                AddRandomCurse(false);
                            }

                            AddRandomCurse();
                            ApplyOneTimeCurseEffects();
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

            internalRoomCurses.Add(WorldGen.genRand.Next(Enum.GetValues<PyramidRoomCurseType>().Where(curse => !internalRoomCurses.Contains(curse)).ToList()));

            if (Main.netMode != NetmodeID.Server || !syncToClients) {
                return;
            }

            ModPacket packet = ModContent.GetInstance<PyramidDungeonPacketHandler>().GetPacket(PyramidDungeonPacketHandler.SyncRoomCurses);
            packet.Write(gridTopLeftX);
            packet.Write(gridTopLeftY);
            packet.Write(internalRoomCurses.Count);
            foreach (PyramidRoomCurseType curse in internalRoomCurses) {
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

                        break;
                    case PyramidRoomCurseType.DyingLight:
                        // When Dying Light is added, any torches that existed before-hand need to get added to the death list
                        for (int y = region.Y; y <= region.Bottom; y++) {
                            for (int x = region.X; x <= region.Right; x++) {
                                if (Main.tile[x, y].TileType == TileID.Torches) {
                                    PyramidDungeonSystem.Instance.AddNewDyingTorch(new Point(x, y));
                                }
                            }
                        }

                        break;
                }
            }
        }
    }
}