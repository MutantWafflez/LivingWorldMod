using System.Collections.Generic;
using System.IO;
using System.Linq;
using LivingWorldMod.Common.ModTypes;
using LivingWorldMod.Content.TileEntities.Interactables;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.PacketHandlers {
    /// <summary>
    /// PacketHandler that handles the syncing of Waystones between server and client
    /// when a new client joins the server.
    /// </summary>
    public class WaystoneSyncHandler : PacketHandler {
        public override void HandlePacket(BinaryReader reader, int fromWhomst) {
            if (Main.netMode == NetmodeID.Server) {
                List<WaystoneEntity> waystones = TileEntity.ByID.Values.OfType<WaystoneEntity>().ToList();

                foreach (WaystoneEntity entity in waystones) {
                    NetMessage.SendData(MessageID.TileSection, fromWhomst, number: entity.Position.X - 1, number2: entity.Position.Y - 1, number3: 4, number4: 4);
                }
            }
        }

        // There is no "types" of packets for Waystone handling (for now?), thus we don't need to write packetType.
        protected override void HijackGetPacket(ref ModPacket packet, byte packetType) {
            return;
        }
    }
}