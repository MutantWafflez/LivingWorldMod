using LivingWorldMod.Content.Walls.WorldGen;
using LivingWorldMod.Custom.Interfaces;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Biomes {
    /// <summary>
    /// "Biome" for the Revamped Pyramid dungeon.
    /// </summary>
    public class RevampedPyramidBiome : ModBiome, IModifyLightingBrightness, IModifySunlightColor {
        public override bool IsPrimaryBiome => true;

        public bool SunlightEffectActive => IsBiomeActive(Main.LocalPlayer);

        public bool LightingEffectActive => SunlightEffectActive;

        public override bool IsBiomeActive(Player player) => Framing.GetTileSafely((int)(player.Center.X / 16f), (int)(player.Center.Y / 16f)).WallType == ModContent.WallType<PyramidBrickWall>();

        public void SunlightEffect(ref Color tileColor, ref Color backgroundColor) {
            tileColor = tileColor.MultiplyRGB(Color.Black);
            backgroundColor = backgroundColor.MultiplyRGB(Color.Black);
        }

        public void LightingEffect(ref float scale) {
            scale *= 0.25f;
        }
    }
}