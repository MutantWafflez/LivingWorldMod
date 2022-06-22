using LivingWorldMod.Common.ModTypes;
using LivingWorldMod.Content.TileEntities.Interactables;
using LivingWorldMod.Custom.Interfaces;
using LivingWorldMod.Custom.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.DataStructures;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Core.PacketHandlers {
    /// <summary>
    /// PacketHandler that handles all packet functionality in relation to Village Shrines.
    /// </summary>
    public class ShrinePacketHandler : PacketHandler, IPlayerEnteredWorld {
        /// <summary>
        /// Sent/Received when a new player/client first enters a server's world. Syncs all Shrine
        /// Tile Entities from the server to said client.
        /// </summary>
        public const byte SyncNewPlayer = 0;

        /// <summary>
        /// Sent/Received when a player with a shrine UI open wants to add a new respawn item to the
        /// shrine's inventory.
        /// </summary>
        public const byte AddRespawnItem = 1;

        /// <summary>
        /// Sent/Recieved when a player with a shrine UI open wants to take a respawn item from the
        /// shrine's inventory.
        /// </summary>
        public const byte TakeRespawnItem = 2;

        public override void HandlePacket(BinaryReader reader, int fromWhomst) {
            byte packetType = reader.ReadByte();

            switch (packetType) {
                case SyncNewPlayer:
                    if (Main.netMode == NetmodeID.Server) {
                        List<VillageShrineEntity> shrines = TileEntityUtils.GetAllEntityOfType<VillageShrineEntity>().ToList();

                        foreach (VillageShrineEntity entity in shrines) {
                            NetMessage.SendData(MessageID.TileSection, fromWhomst, number: entity.Position.X - 1, number2: entity.Position.Y - 1, number3: 5, number4: 6);
                        }
                    }
                    break;
                case AddRespawnItem:
                    if (Main.netMode == NetmodeID.Server) {
                        Point16 entityPos = reader.ReadVector2().ToPoint16();

                        if (TileEntity.ByPosition.TryGetValue(entityPos, out TileEntity entity) && entity is VillageShrineEntity shrineEntity) {
                            if (shrineEntity.remainingRespawnItems < shrineEntity.CurrentValidHouses) {
                                shrineEntity.remainingRespawnItems++;

                                ModPacket packet = GetPacket(AddRespawnItem);
                                packet.Write(true);
                                packet.Send(fromWhomst);

                                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, shrineEntity.ID, shrineEntity.Position.X, shrineEntity.Position.Y);
                            }
                            else {
                                ModPacket packet = GetPacket(AddRespawnItem);
                                packet.Write(false);
                                packet.WriteVector2(entityPos.ToVector2());
                                packet.Send(fromWhomst);
                            }
                        }
                    }
                    else if (Main.netMode == NetmodeID.MultiplayerClient) {
                        if (!reader.ReadBoolean()) {
                            Point16 entityPos = reader.ReadVector2().ToPoint16();

                            if (TileEntity.ByPosition.TryGetValue(entityPos, out TileEntity entity) && entity is VillageShrineEntity shrineEntity) {
                                Main.LocalPlayer.QuickSpawnItem(new EntitySource_Sync(), NPCUtils.VillagerTypeToRespawnItemType(shrineEntity.shrineType));
                            }
                            else {
                                ModContent.GetInstance<LivingWorldMod>().Logger.Error($"Failed AddRespawnItem received, but got invalid/no entity at position: {entityPos}");
                            }
                        }
                    }
                    break;
                case TakeRespawnItem:
                    if (Main.netMode == NetmodeID.Server) {
                        Point16 entityPos = reader.ReadVector2().ToPoint16();

                        if (TileEntity.ByPosition.TryGetValue(entityPos, out TileEntity entity) && entity is VillageShrineEntity shrineEntity) {
                            ModPacket packet = GetPacket(TakeRespawnItem);
                            packet.WriteVector2(entityPos.ToVector2());

                            if (shrineEntity.remainingRespawnItems > 0) {
                                shrineEntity.remainingRespawnItems--;

                                packet.Write(true);
                                packet.Send(fromWhomst);

                                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, shrineEntity.ID, shrineEntity.Position.X, shrineEntity.Position.Y);
                            }
                            else {
                                packet.Write(false);
                                packet.Send(fromWhomst);
                            }
                        }
                    }
                    else if (Main.netMode == NetmodeID.MultiplayerClient) {
                        Point16 entityPos = reader.ReadVector2().ToPoint16();

                        if (!reader.ReadBoolean()) {
                            ModContent.GetInstance<LivingWorldMod>().Logger.Warn($"Client attempted TakeRespawnItem when Server had no respawn items left at position: {entityPos}");
                        }
                        else {
                            if (TileEntity.ByPosition.TryGetValue(entityPos, out TileEntity entity) && entity is VillageShrineEntity shrineEntity) {
                                Main.LocalPlayer.QuickSpawnItem(new EntitySource_Sync(), NPCUtils.VillagerTypeToRespawnItemType(shrineEntity.shrineType));
                            }
                            else {
                                ModContent.GetInstance<LivingWorldMod>().Logger.Error($"Successful TakeRespawnItem received, but got invalid/no entity at position: {entityPos}");
                            }
                        }
                    }
                    break;
                default:
                    ModContent.GetInstance<LivingWorldMod>().Logger.Warn($"Invalid ShrinePacketHandler Packet Type of {packetType}");
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