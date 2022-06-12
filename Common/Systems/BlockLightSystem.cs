using LivingWorldMod.Common.Systems.BaseSystems;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Systems {
    /// <summary>
    /// Small ModSystem that handles color changing for blocks, or potentially other applications.
    /// Has a couple of static fields that pertain to specific block colors.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class BlockLightSystem : BaseModSystem<BlockLightSystem> {
        /// <summary>
        /// The current glow color of the Starshard Cloud blocks.
        /// </summary>
        public Color starCloudColor;

        /// <summary>
        /// The glow color the Starshard Cloud blocks are transitioning towards.
        /// </summary>
        public Color targetStarCloudColor;

        /// <summary>
        /// The glow color of the star torches.
        /// </summary>
        public Color starTorchColor;

        private int starCloudTimer;

        public override void Load() {
            starCloudColor = targetStarCloudColor = Color.Yellow;
            starCloudTimer = 5 * 60;

            starTorchColor = new Color(0.97f, 0.84f, 0.41f);
        }

        public override void PostUpdateEverything() {
            TransitionStarshardColor();
        }

        private void TransitionStarshardColor() {
            //Lerp towards target color
            starCloudColor = Color.Lerp(starCloudColor, targetStarCloudColor, 0.34f / 60f);

            if (--starCloudTimer <= 0) {
                //Pick random color after the timer runs out
                targetStarCloudColor = Main.rand.Next(3) switch {
                    0 => Color.Yellow,
                    1 => Color.DarkCyan,
                    2 => Color.Magenta,
                    _ => targetStarCloudColor
                };
                //Timer set to 5 seconds
                starCloudTimer = 5 * 60;
            }
        }
    }
}