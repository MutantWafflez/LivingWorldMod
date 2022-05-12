using LivingWorldMod.Custom.Enums;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Systems {
    /// <summary>
    /// ModSystem that helps with miscellaneous Waystone functionality. Right now, all it does is
    /// sync multiplayer clients with the server's Waystone tile entities.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class WaystoneSystem : ModSystem {
        public override void OnWorldLoad() {
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                ModPacket packet = Mod.GetPacket();
                packet.Write((int)PacketType.SyncWaystones);

                packet.Send();
            }
        }
    }
}