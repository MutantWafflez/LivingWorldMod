using Terraria.Localization;

namespace LivingWorldMod.Utilities;

// Utilities class which holds methods that deals with localization.
public static partial class LWMUtils {
    /// <summary>
    ///     Extension for strings that will turn the provided string into its <see cref="LocalizedText" /> equivalent, assuming
    ///     the provided string is the key for a <see cref="LWM" /> localization file text value.
    /// </summary>
    public static LocalizedText Localized(this string key) => ModContent.GetInstance<LWM>().GetLocalization(key);

    /// <summary>
    ///     Extension for strings that will prepend "Mods.LivingWorldMod." to the string, for use with generating localization keys
    ///     instead of <see cref="LocalizedText" /> objects directly.
    /// </summary>
    public static string PrependModKey(this string suffix) => $"Mods.{nameof(LivingWorldMod)}.{suffix}";
}