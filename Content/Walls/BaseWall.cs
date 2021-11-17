using Terraria.ModLoader;

namespace LivingWorldMod.Content.Walls {
    /// <summary>
    /// Base class for all LWM Walls that currently only has the functionality of overriding the
    /// Texture value to retrieve the wall's sprite from the Assets folder.
    /// </summary>
    public abstract class BaseWall : ModWall {
        public override string Texture => GetType()
                                          .Namespace?
                                          .Replace($"{nameof(LivingWorldMod)}.Content.", LivingWorldMod.LWMSpritePath)
                                          .Replace('.', '/')
                                          + $"/{Name}";
    }
}