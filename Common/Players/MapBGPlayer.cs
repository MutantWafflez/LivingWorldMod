using LivingWorldMod.Content.Biomes;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Players {
    /// <summary>
    /// ModPlayer that handles map backgrounds.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class MapBGPlayer : ModPlayer {
        private Asset<Texture2D> _pyramidBG;

        public override void Load() {
            _pyramidBG = ModContent.Request<Texture2D>($"{LivingWorldMod.LWMSpritePath}Backgrounds/Loading/PyramidBG");
        }

        public override Texture2D GetMapBackgroundImage() => ModContent.GetInstance<RevampedPyramidBiome>().IsBiomeActive(Player) ? _pyramidBG.Value : null;
    }
}