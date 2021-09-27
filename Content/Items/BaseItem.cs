using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items {

    /// <summary>
    /// Base class for all LWM Items that currently only has the functionality of overriding the
    /// Texture value to retrieve the item's sprite from the Assets folder.
    /// </summary>
    public abstract class BaseItem : ModItem {

        public override string Texture => GetType().Namespace?
            .Replace($"{nameof(LivingWorldMod)}.Content", LivingWorldMod.LWMSpritePath)
            .Replace('.', '/')
            + $"/{Name}";
    }
}