using LivingWorldMod.Common.Systems.BaseSystems;
using LivingWorldMod.Custom.Interfaces;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Common.ILoadables;
using LivingWorldMod.Common.Players;
using LivingWorldMod.Content.Subworlds.Pyramid;
using SubworldLibrary;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Systems {
    /// <summary>
    /// System that handles client-side changes to the screen, whether that be zooming, transforming,
    /// changing lighting, or applying screen-wide shaders.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class ScreenModificationSystem : BaseModSystem<ScreenModificationSystem> {
        public List<IModifyLightingBrightness> LightingBrightnessEffects {
            get;
            private set;
        }

        public List<IModifySunlightColor> SunlightColorEffects {
            get;
            private set;
        }

        public override void PostSetupContent() {
            LightingBrightnessEffects = ModContent.GetContent<IModifyLightingBrightness>().ToList();
            SunlightColorEffects = ModContent.GetContent<IModifySunlightColor>().ToList();
        }

        public override void ModifyLightingBrightness(ref float scale) {
            foreach (IModifyLightingBrightness lightingEffect in LightingBrightnessEffects.Where(effect => effect.LightingEffectActive)) {
                lightingEffect.LightingEffect(ref scale);
            }
        }

        public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor) {
            foreach (IModifySunlightColor sunlightEffect in SunlightColorEffects.Where(effect => effect.SunlightEffectActive)) {
                sunlightEffect.SunlightEffect(ref tileColor, ref backgroundColor);
            }
        }

        public override void ModifyTransformMatrix(ref SpriteViewMatrix Transform) {
            if (!SubworldSystem.IsActive<PyramidSubworld>()) {
                return;
            }

            foreach (PyramidRoomCurseType curse in Main.LocalPlayer.GetModPlayer<PyramidDungeonPlayer>().CurrentCurses) {
                switch (curse) {
                    case PyramidRoomCurseType.Spotlight:
                        Transform.Zoom = new Vector2(Main.ForcedMinimumZoom * 4f);
                        break;
                }
            }
        }
    }
}