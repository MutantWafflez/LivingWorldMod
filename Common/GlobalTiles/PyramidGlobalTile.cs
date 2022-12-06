using LivingWorldMod.Common.Players;
using LivingWorldMod.Common.Systems.SubworldSystems;
using LivingWorldMod.Content.Subworlds.Pyramid;
using LivingWorldMod.Core.PacketHandlers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.GlobalTiles {
    /// <summary>
    /// Global Tile used exclusively in the Pyramid Dungeon.
    /// </summary>
    public class PyramidGlobalTile : GlobalTile {
        public bool IsInPyramidSubworld => SubworldSystem.IsActive<PyramidSubworld>();

        public override bool CanKillTile(int i, int j, int type, ref bool blockDamaged) => !IsInPyramidSubworld || LivingWorldMod.IsDebug;

        public override bool CanExplode(int i, int j, int type) => !IsInPyramidSubworld || LivingWorldMod.IsDebug;

        public override bool PreDraw(int i, int j, int type, SpriteBatch spriteBatch) {
            if (!(Main.LocalPlayer.GetModPlayer<PyramidDungeonPlayer>().currentRoom?.region.Contains(i, j) ?? true)) {
                return false;
            }

            return true;
        }

        public override void PlaceInWorld(int i, int j, int type, Item item) {
            if (type != TileID.Torches || !IsInPyramidSubworld) {
                return;
            }
            if (ModContent.GetInstance<PyramidSubworld>().grid.GetRoomFromTilePosition(new Point(i, j)) is not { } room || !room.ActiveCurses.Contains(PyramidRoomCurseType.DyingLight)) {
                return;
            }

            if (Main.netMode == NetmodeID.MultiplayerClient) {
                ModPacket packet = ModContent.GetInstance<PyramidDungeonPacketHandler>().GetPacket(PyramidDungeonPacketHandler.SyncDyingLightCurse);
                packet.Write(i);
                packet.Write(j);

                packet.Send();
            }
            else {
                PyramidDungeonSystem.Instance.AddNewDyingTorch(new Point(i, j));
            }
        }
    }
}