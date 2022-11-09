using LivingWorldMod.Common.Players;
using LivingWorldMod.Content.Subworlds.Pyramid;
using LivingWorldMod.Core.PacketHandlers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Core.Patches {
    /// <summary>
    /// ILoadable class that holds patches for specifically the Update method for
    /// players and any tertiary related method.
    /// </summary>
    public class PlayerUpdatePatches : ILoadable {
        public void Load(Mod mod) {
            On.Terraria.Player.JumpMovement += PlayerGravityManipulation;
        }

        public void Unload() { }

        private void PlayerGravityManipulation(On.Terraria.Player.orig_JumpMovement orig, Player self) {
            //Due to the limitations of ModPlayer update hooks, we must mess with gravity here.
            PyramidDungeonPlayer player = self.GetModPlayer<PyramidDungeonPlayer>();
            foreach (PyramidRoomCurseType curse in player.CurrentCurses) {
                switch (curse) {
                    case PyramidRoomCurseType.GravitationalInstability:
                        self.gravDir = player.forcedGravityDirection;

                        if (Main.netMode != NetmodeID.Server && self.whoAmI == Main.myPlayer && --player.gravitySwapTimer <= 0) {
                            player.forcedGravityDirection *= -1;
                            player.gravitySwapTimer = Main.rand.Next(60 * 5, 60 * 10);

                            if (Main.netMode == NetmodeID.MultiplayerClient) {
                                ModPacket packet = ModContent.GetInstance<PyramidDungeonPacketHandler>().GetPacket(PyramidDungeonPacketHandler.SyncPlayerGravitySwap);
                                packet.Write(player.forcedGravityDirection);

                                packet.Send();
                            }
                        }

                        break;
                }
            }

            orig(self);
        }
    }
}