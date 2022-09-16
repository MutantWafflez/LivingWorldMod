using System;
using System.IO;
using LivingWorldMod.Common.ModTypes;
using LivingWorldMod.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Core.PacketHandlers {
    public class CutscenePacketHandler : PacketHandler {
        /// <summary>
        /// Sent/Received when any player/client activates a cutscene, and syncs that cutscene with
        /// every other client.
        /// </summary>
        public const byte SyncCutsceneToAllClients = 0;

        /// <summary>
        /// Sent/Received when any player/client finishes the cutscene on their side, and syncs that with
        /// every other client.
        /// </summary>
        public const byte SyncCutsceneFinishToAllClients = 1;

        public override void HandlePacket(BinaryReader reader, int fromWhomst) {
            byte packetType = reader.ReadByte();

            switch (packetType) {
                case SyncCutsceneToAllClients:
                    byte cutsceneType = reader.ReadByte();
                    Cutscene newCutscene = Activator.CreateInstance(Cutscene.GetCutsceneFromType(cutsceneType).GetType(), true) as Cutscene;
                    newCutscene.HandleCutscenePacket(reader, fromWhomst);

                    if (Main.netMode == NetmodeID.Server) {
                        newCutscene.SendCutscenePacket(fromWhomst, fromWhomst);
                    }

                    break;
                case SyncCutsceneFinishToAllClients:
                    if (Main.netMode == NetmodeID.Server) {
                        Main.player[fromWhomst].GetModPlayer<CutscenePlayer>().EndCutscene();

                        ModPacket packet = GetPacket(SyncCutsceneFinishToAllClients);
                        packet.Send(ignoreClient: fromWhomst);
                    }
                    else {
                        int sendingPlayer = reader.ReadInt32();

                        Main.player[sendingPlayer].GetModPlayer<CutscenePlayer>().EndCutscene();
                    }

                    break;
                default:
                    ModContent.GetInstance<LivingWorldMod>().Logger.Warn($"Invalid CutscenePacketHandler Packet Type of {packetType}");
                    break;
            }
        }
    }
}