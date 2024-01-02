using LivingWorldMod.Common.Systems.BaseSystems;
using LivingWorldMod.Content.StatusEffects.Debuffs.Consumables;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Systems;

/// <summary>
/// System that handles client-side changes to the screen, whether that be zooming, transforming,
/// changing lighting, or applying screen-wide shaders.
/// </summary>
[Autoload(Side = ModSide.Client)]
public class ScreenModificationSystem : BaseModSystem<ScreenModificationSystem> {
    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor) {
        if (Main.LocalPlayer.HasBuff<SugarSuperfluity>()) {
            tileColor = backgroundColor = Main.DiscoColor;
        }
    }
}