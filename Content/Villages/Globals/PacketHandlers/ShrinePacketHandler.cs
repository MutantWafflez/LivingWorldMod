using System.IO;
using LivingWorldMod.Content.Villages.Globals.Systems.UI;
using LivingWorldMod.Content.Villages.HarpyVillage.Tiles.Furniture;
using LivingWorldMod.Globals.ModTypes;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace LivingWorldMod.Content.Villages.Globals.PacketHandlers;

/// <summary>
///     PacketHandler that handles all packet functionality in relation to Village Shrines.
/// </summary>
public class ShrinePacketHandler : PacketHandler {
    /// <summary>
    ///     Sent/Received when a player with a shrine UI open wants to add a new respawn item to the
    ///     shrine's inventory.
    /// </summary>
    public const byte AddRespawnItem = 0;

    /// <summary>
    ///     Sent/Received when a player with a shrine UI open wants to take a respawn item from the
    ///     shrine's inventory.
    /// </summary>
    public const byte TakeRespawnItem = 1;

    /// <summary>
    ///     Sent by the client when a player with a shrine UI open wants the Server to resync the
    ///     housing for said shrine.
    /// </summary>
    public const byte TriggerForceSync = 2;

    /// <summary>
    ///     Sent/Recieved when a player with a shrine UI open clicks the button to toggle villager respawning.
    /// </summary>
    public const byte ToggleVillagerRespawning = 3;

    public override void HandlePacket(BinaryReader reader, int fromWhomst) {
        byte packetType = reader.ReadByte();

        switch (packetType) {
            case AddRespawnItem:
                if (Main.netMode == NetmodeID.Server) {
                    Point16 entityPos = reader.ReadVector2().ToPoint16();

                    if (TileEntity.ByPosition.TryGetValue(entityPos, out TileEntity entity) && entity is VillageShrineEntity shrineEntity) {
                        if (shrineEntity.remainingRespawnItems < shrineEntity.CurrentValidHouses) {
                            shrineEntity.remainingRespawnItems++;

                            ModPacket packet = GetPacket();
                            packet.Write(true);
                            packet.Send(fromWhomst);

                            NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, shrineEntity.ID, shrineEntity.Position.X, shrineEntity.Position.Y);
                        }
                        else {
                            ModPacket packet = GetPacket();
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
                            Main.LocalPlayer.QuickSpawnItem(new EntitySource_Sync(), LWMUtils.VillagerTypeToRespawnItemType(shrineEntity.shrineType));

                            ModContent.GetInstance<VillageShrineUISystem>().OpenOrRegenShrineState(entityPos);
                        }
                        else {
                            LWM.Instance.Logger.Error($"Failed AddRespawnItem received, but got invalid/no entity at position: {entityPos}");
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
                        LWM.Instance.Logger.Warn($"Client attempted TakeRespawnItem when Server had no respawn items left at position: {entityPos}");
                    }
                    else {
                        if (TileEntity.ByPosition.TryGetValue(entityPos, out TileEntity entity) && entity is VillageShrineEntity shrineEntity) {
                            Main.LocalPlayer.QuickSpawnItem(new EntitySource_Sync(), LWMUtils.VillagerTypeToRespawnItemType(shrineEntity.shrineType));

                            ModContent.GetInstance<VillageShrineUISystem>().OpenOrRegenShrineState(entityPos);
                        }
                        else {
                            LWM.Instance.Logger.Error($"Successful TakeRespawnItem received, but got invalid/no entity at position: {entityPos}");
                        }
                    }
                }

                break;
            case TriggerForceSync:
                if (Main.netMode == NetmodeID.Server) {
                    Point16 entityPos = reader.ReadVector2().ToPoint16();

                    if (TileEntity.ByPosition.TryGetValue(entityPos, out TileEntity entity) && entity is VillageShrineEntity shrineEntity) {
                        shrineEntity.ForceRecalculateAndSync();
                    }
                    else {
                        LWM.Instance.Logger.Error($"TriggerForceSync received, but got invalid/no entity at position: {entityPos}");
                    }
                }

                break;
            case ToggleVillagerRespawning:
                if (Main.netMode == NetmodeID.Server) {
                    Vector2 entityPos = reader.ReadVector2();

                    if (TileEntity.ByPosition.TryGetValue(entityPos.ToPoint16(), out TileEntity entity) && entity is VillageShrineEntity shrineEntity) {
                        shrineEntity.pausedRespawns = !shrineEntity.pausedRespawns;

                        // Syncing all the fields with a normal MessageID.TileEntitySharing feels a bit excessive for such a minor change, so manually setting will do. See TriggerForceSync for full
                        // syncing instead.
                        ModPacket returnPacket = GetPacket(ToggleVillagerRespawning);
                        returnPacket.WriteVector2(entityPos);
                        returnPacket.Write(shrineEntity.pausedRespawns);
                        returnPacket.Send();
                    }
                    else {
                        LWM.Instance.Logger.Error($"ToggleVillagerRespawning received, but got invalid/no entity at position: {entityPos}");
                    }
                }
                else if (Main.netMode == NetmodeID.MultiplayerClient) {
                    Vector2 entityPos = reader.ReadVector2();
                    bool shouldBePaused = reader.ReadBoolean();

                    if (TileEntity.ByPosition.TryGetValue(entityPos.ToPoint16(), out TileEntity entity) && entity is VillageShrineEntity shrineEntity) {
                        shrineEntity.pausedRespawns = shouldBePaused;

                        VillageShrineUISystem.Instance.UIState.SetVillagerPauseStatus();
                    }
                    else {
                        LWM.Instance.Logger.Error($"ToggleVillagerRespawning received, but got invalid/no entity at position: {entityPos}");
                    }
                }

                break;
            default:
                LWM.Instance.Logger.Warn($"Invalid ShrinePacketHandler Packet Type of {packetType}");
                break;
        }
    }
}