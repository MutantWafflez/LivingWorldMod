using System.IO;
using LivingWorldMod.Content.Waystones.Globals.Systems;
using LivingWorldMod.Content.Waystones.Tiles;
using LivingWorldMod.Globals.ModTypes;
using LivingWorldMod.Utilities;
using Terraria.DataStructures;

namespace LivingWorldMod.Content.Waystones.Globals.PacketHandlers;

/// <summary>
///     PacketHandler that handles all packets relating to Waystones.
/// </summary>
public class WaystonePacketHandler : PacketHandler {
    /// <summary>
    ///     Sent/Received when any player/client activates an inactive waystone, and Sync the activation
    ///     with all other clients.
    /// </summary>
    public const byte InitiateWaystoneActivation = 0;

    public override void HandlePacket(BinaryReader reader, int fromWhomst) {
        byte packetType = reader.ReadByte();

        switch (packetType) {
            case InitiateWaystoneActivation:
                if (Main.netMode == NetmodeID.Server) {
                    int entityPosX = reader.ReadInt32();
                    int entityPosY = reader.ReadInt32();

                    if (!LWMUtils.TryFindModEntity(entityPosX, entityPosY, out WaystoneEntity entity) || entity.DoingActivationVFX) {
                        return;
                    }

                    entity.ActivateWaystoneEntity();

                    ModPacket packet = GetPacket();
                    packet.Write(entityPosX);
                    packet.Write(entityPosY);

                    packet.Send(ignoreClient: fromWhomst);
                }
                else if (Main.netMode == NetmodeID.MultiplayerClient) {
                    Point16 entityPos = new(reader.ReadInt32(), reader.ReadInt32());

                    if (!LWMUtils.TryFindModEntity(entityPos.X, entityPos.Y, out WaystoneEntity entity)) {
                        return;
                    }

                    WaystoneSystem.Instance.AddNewActivationEntity(entityPos.ToWorldCoordinates(16, 16), entity.WaystoneColor);
                }

                break;
            default:
                ModContent.GetInstance<LWM>().Logger.Warn($"Invalid WaystonePacketHandler Packet Type of {packetType}");
                break;
        }
    }
}