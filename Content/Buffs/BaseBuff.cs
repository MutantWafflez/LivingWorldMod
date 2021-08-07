using LivingWorldMod.Custom.Utilities;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Buffs {
    /// <summary>
    /// Base class for all LWM Buffs that currently only has the functionality of overriding the
    /// Texture value to retrieve the buff's sprite from the Assets folder.
    /// </summary>
    public abstract class BaseBuff : ModBuff {

        public override string Texture => GetType().Namespace?
            .Replace($"{nameof(LivingWorldMod)}.Content", IOUtilities.LWMSpritePath)
            .Replace('.', '/')
            + $"/{Name}";
    }
}