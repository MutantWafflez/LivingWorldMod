namespace LivingWorldMod.Globals.BaseTypes.StatusEffects;

/// <summary>
///     Base class for all LWM Status Effects (buffs, debuffs, etc.) that currently only has the functionality of overriding
///     the
///     Texture value to retrieve the buff's sprite from the Assets folder.
/// </summary>
public abstract class BaseStatusEffect : ModBuff {
    public override string Texture => GetType()
            .Namespace?
            .Replace($"{nameof(LivingWorldMod)}.Content.", LWM.SpritePath)
            .Replace('.', '/')
        + $"/{Name}";
}