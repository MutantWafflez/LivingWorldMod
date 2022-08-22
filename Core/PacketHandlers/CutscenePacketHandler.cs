using System.IO;
using LivingWorldMod.Common.ModTypes;
using LivingWorldMod.Common.Players;
using LivingWorldMod.Content.Tiles.Interactables;
using LivingWorldMod.Custom.Classes.Cutscenes;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Core.PacketHandlers {
    public class CutscenePacketHandler : PacketHandler {
        /// <summary>
        /// Sent/Received when any player/client activates a cutscene, and syncs that cutscene with
        /// every other client.
        /// </summary>
        public const byte SyncPyramidEnterCutscene = 0;

        public override void HandlePacket(BinaryReader reader, int fromWhomst) {
            byte packetType = reader.ReadByte();

            switch (packetType) {
                case SyncPyramidEnterCutscene:
                    if (Main.netMode == NetmodeID.Server) {
                        Player player = Main.player[fromWhomst];
                        Point16 doorPos = new Point16(reader.ReadInt16(), reader.ReadInt16());

                        if (Framing.GetTileSafely(doorPos).TileType != ModContent.TileType<PyramidDoorTile>() || !player.active) {
                            break;
                        }
                        player.GetModPlayer<CutscenePlayer>().StartCutscene(new EnterPyramidCutscene(doorPos));

                        ModPacket packet = GetPacket(SyncPyramidEnterCutscene);
                        packet.Write(fromWhomst);
                        packet.Write(doorPos.X);
                        packet.Write(doorPos.Y);

                        packet.Send(ignoreClient: fromWhomst);
                    }
                    else if (Main.netMode == NetmodeID.MultiplayerClient) {
                        Player player = Main.player[reader.ReadInt32()];
                        Point16 doorPos = new Point16(reader.ReadInt16(), reader.ReadInt16());

                        if (!player.active) {
                            break;
                        }

                        player.GetModPlayer<CutscenePlayer>().StartCutscene(new EnterPyramidCutscene(doorPos));
                    }
                    break;
                default:
                    ModContent.GetInstance<LivingWorldMod>().Logger.Warn($"Invalid CutscenePacketHandler Packet Type of {packetType}");
                    break;
            }
        }
    }
}