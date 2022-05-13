using LivingWorldMod.Common.PacketHandlers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Commands.DebugCommands {
    /// <summary>
    /// Command that syncs all Waystone Tile Entities to from the server to this client.
    /// </summary>
    public class WaystoneSyncCommand : DebugCommand {
        public override string Command => "syncway";

        public override string Usage => "/syncway";

        public override string Description => "Command that syncs all Waystone Tile Entities to from the server to this client.";

        public override CommandType Type => CommandType.Chat;

        public override void Action(CommandCaller caller, string input, string[] args) {
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                ModPacket packet = ModContent.GetInstance<WaystonePacketHandler>().GetPacket(WaystonePacketHandler.SyncNewPlayer);

                packet.Send();
                caller.Reply("Sync Message Sent.");
            }
        }
    }
}