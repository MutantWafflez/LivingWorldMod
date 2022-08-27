using System;
using System.IO;
using LivingWorldMod.Common.ModTypes;
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
                default:
                    ModContent.GetInstance<LivingWorldMod>().Logger.Warn($"Invalid CutscenePacketHandler Packet Type of {packetType}");
                    break;
            }
        }
    }
}