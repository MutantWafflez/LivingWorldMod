using System.Collections.Generic;
using System.IO;
using System.Linq;
using LivingWorldMod.Common.ModTypes;
using LivingWorldMod.Common.Systems;
using LivingWorldMod.Content.TileEntities.Interactables;
using LivingWorldMod.Custom.Interfaces;
using LivingWorldMod.Custom.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Core.PacketHandlers {
    /// <summary>
    /// PacketHandler that handles all packets relating to Waystones.
    /// </summary>
    public class WaystonePacketHandler : PacketHandler, IPlayerEnteredWorld {
        /// <summary>
        /// Sent/Received when a new player/client first enters a server's world. Syncs all Waystone
        /// Tile Entities from the server to said client.
        /// </summary>
        public const byte SyncNewPlayer = 0;

        /// <summary>
        /// Sent/Received when any player/client activates an unactive waystone, and Sync the activation
        /// with all other clients.
        /// </summary>
        public const byte InitiateWaystoneActivation = 1;

        public override void HandlePacket(BinaryReader reader, int fromWhomst) {
            byte packetType = reader.ReadByte();

            switch (packetType) {
                case SyncNewPlayer:
                    if (Main.netMode == NetmodeID.Server) {
                        List<WaystoneEntity> waystones = TileEntity.ByID.Values.OfType<WaystoneEntity>().ToList();

                        foreach (WaystoneEntity entity in waystones) {
                            NetMessage.SendData(MessageID.TileSection, fromWhomst, number: entity.Position.X - 1, number2: entity.Position.Y - 1, number3: 4, number4: 4);
                        }
                    }
                    break;
                case InitiateWaystoneActivation:
                    if (Main.netMode == NetmodeID.Server) {
                        int entityPosX = reader.ReadInt32();
                        int entityPosY = reader.ReadInt32();

                        if (!TileEntityUtils.TryFindModEntity(entityPosX, entityPosY, out WaystoneEntity entity)) {
                            return;
                        }
                        entity.ActivateWaystoneEntity();

                        ModPacket packet = GetPacket(InitiateWaystoneActivation);
                        packet.Write(entityPosX);
                        packet.Write(entityPosY);

                        packet.Send(ignoreClient: fromWhomst);
                    }
                    else if (Main.netMode == NetmodeID.MultiplayerClient) {
                        Point16 entityPos = new Point16(reader.ReadInt32(), reader.ReadInt32());

                        if (!TileEntityUtils.TryFindModEntity(entityPos.X, entityPos.Y, out WaystoneEntity entity)) {
                            return;
                        }

                        ModContent.GetInstance<WaystoneSystem>().AddNewActivationEntity(entityPos.ToWorldCoordinates(16, 16), entity.WaystoneColor);
                    }
                    break;
                default:
                    ModContent.GetInstance<LivingWorldMod>().Logger.Warn($"Invalid WaystonePacketHandler Packet Type of {packetType}");
                    break;
            }
        }

        public void OnPlayerEnterWorld() {
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                ModPacket packet = GetPacket(SyncNewPlayer);

                packet.Send();
            }
        }
    }
}