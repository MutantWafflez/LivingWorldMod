using LivingWorldMod.Common.Players;
using LivingWorldMod.Content.Subworlds.Pyramid;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.GlobalWalls {
    /// <summary>
    /// Global Wall used exclusively in the Pyramid Dungeon.
    /// </summary>
    public class PyramidGlobalWall : GlobalWall {
        public override void KillWall(int i, int j, int type, ref bool fail) {
            fail = PyramidSubworld.IsInSubworld && !LivingWorldMod.IsDebug;
        }

        public override bool CanExplode(int i, int j, int type) => !PyramidSubworld.IsInSubworld || LivingWorldMod.IsDebug;

        public override bool PreDraw(int i, int j, int type, SpriteBatch spriteBatch) => Main.LocalPlayer.GetModPlayer<PyramidDungeonPlayer>().currentRoom?.region.Contains(i, j) ?? true;
    }
}