namespace LivingWorldMod.Globals.BaseTypes.Projectiles;

/// <summary>
/// Base class for all LWM Projectiles that currently only has the functionality of overriding
/// the Texture value to retrieve the projectile's sprite from the Assets folder.
/// </summary>
public abstract class BaseProjectile : ModProjectile {
    public override string Texture => GetType()
                                      .Namespace?
                                      .Replace($"{nameof(LivingWorldMod)}.Content.", LWM.SpritePath)
                                      .Replace('.', '/')
                                      + $"/{Name}";
}