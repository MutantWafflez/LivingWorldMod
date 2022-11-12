using LivingWorldMod.Common.Players;
using Terraria;
using LivingWorldMod.Content.Subworlds.Pyramid;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SubworldLibrary;
using Terraria.GameContent;
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
    }
}