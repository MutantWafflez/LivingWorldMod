using System.IO;
using LivingWorldMod.Common.ModTypes;
using LivingWorldMod.Common.Players;
using LivingWorldMod.Common.Systems.SubworldSystems;
using LivingWorldMod.Content.Subworlds.Pyramid;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Core.PacketHandlers {
    public class PyramidDungeonPacketHandler : PacketHandler {
        /// <summary>
        /// Sent/Received under the influence of the curse of Gravitational Instability.
        /// Updates the sending player's forced gravity value to every other client.
        /// </summary>
        public const byte SyncPlayerGravitySwap = 0;

        /// <summary>
        /// Sent from a client when placing a torch in a room with the Dying Light curse.
        /// Received on the server and added to the torch list, where the rest of the
        /// process is handled.
        /// </summary>
        public const byte SyncDyingLightCurse = 1;

        /// <summary>
        /// Send exclusively from the server and received on all clients. Syncs all curses
        /// in the specified room.
        /// </summary>
        public const byte SyncRoomCurses = 2;

        public override void HandlePacket(BinaryReader reader, int fromWhomst) {
            byte packetType = reader.ReadByte();

            switch (packetType) {
                case SyncPlayerGravitySwap:
                    if (Main.netMode == NetmodeID.Server) {
                        float gravDir = reader.ReadSingle();

                        Main.player[fromWhomst].GetModPlayer<PyramidDungeonPlayer>().forcedGravityDirection = gravDir;
                        ModPacket packet = GetPacket(SyncPlayerGravitySwap);
                        packet.Write(gravDir);
                        packet.Write(fromWhomst);

                        packet.Send(ignoreClient: fromWhomst);
                    }
                    else {
                        float gravDir = reader.ReadSingle();
                        int whoWereThey = reader.ReadInt32();

                        Main.player[whoWereThey].GetModPlayer<PyramidDungeonPlayer>().forcedGravityDirection = gravDir;
                    }
                    break;
                case SyncDyingLightCurse:
                    if (Main.netMode == NetmodeID.Server) {
                        Point torchPos = new(reader.ReadInt32(), reader.ReadInt32());

                        PyramidDungeonSystem.Instance.AddNewDyingTorch(torchPos);
                    }

                    break;
                case SyncRoomCurses:
                    int roomGridX = reader.ReadInt32();
                    int roomGridY = reader.ReadInt32();
                    PyramidRoom room = ModContent.GetInstance<PyramidSubworld>().grid.GetRoom(roomGridX, roomGridY);
                    room.internalRoomCurses.Clear();

                    int curseCount = reader.ReadInt32();
                    for (int i = 0; i < curseCount; i++) {
                        PyramidRoomCurseType curse = (PyramidRoomCurseType)reader.ReadInt32();

                        room.internalRoomCurses.Add(curse);
                    }

                    break;
                default:
                    ModContent.GetInstance<LivingWorldMod>().Logger.Warn($"Invalid PyramidDungeonPacketHandler Packet Type of {packetType}");
                    break;
            }
        }
    }
}