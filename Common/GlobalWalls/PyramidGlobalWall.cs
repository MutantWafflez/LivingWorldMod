using LivingWorldMod.Common.Players;
using LivingWorldMod.Content.Subworlds.Pyramid;
using Microsoft.Xna.Framework.Graphics;
using SubworldLibrary;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.GlobalWalls {
    /// <summary>
    /// Global Wall used exclusively in the Pyramid Dungeon.
    /// </summary>
    public class PyramidGlobalWall : GlobalWall {
        public bool IsInPyramidSubworld => SubworldSystem.IsActive<PyramidSubworld>();

        public override void KillWall(int i, int j, int type, ref bool fail) {
            fail = IsInPyramidSubworld && !LivingWorldMod.IsDebug;
        }

        public override bool CanExplode(int i, int j, int type) => !IsInPyramidSubworld || LivingWorldMod.IsDebug;

        public override bool PreDraw(int i, int j, int type, SpriteBatch spriteBatch) => Main.LocalPlayer.GetModPlayer<PyramidDungeonPlayer>().currentRoom?.region.Contains(i, j) ?? true;
    }
}