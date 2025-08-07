using System.Text.RegularExpressions;
using Terraria.Localization;

namespace LivingWorldMod.Utilities;

// Utilities class which holds methods that deals with localization.
public static partial class LWMUtils {
    /// <summary>
    ///     Extension for strings that will turn the provided string into its <see cref="LocalizedText" /> equivalent, assuming
    ///     the provided string is the key for a <see cref="LWM" /> localization file text value.
    /// </summary>
    public static LocalizedText Localized(this string key) => LWM.Instance.GetLocalization(key);

    /// <summary>
    ///     Converts this <see cref="LocalizedText" /> instance into its <see cref="NetworkText" /> equivalent.
    /// </summary>
    public static NetworkText ToNetworkText(this LocalizedText text) => NetworkText.FromKey(text.Key);

    /// <summary>
    ///     Extension for strings that will prepend "Mods.LivingWorldMod." to the string, for use with generating localization keys
    ///     instead of <see cref="LocalizedText" /> objects directly.
    /// </summary>
    public static string PrependModKey(this string suffix) => $"Mods.{nameof(LivingWorldMod)}.{suffix}";

    /// <summary>
    ///     Takes the input string and places a space in-between each english capital letter, returning the result.
    /// </summary>
    public static string SplitBetweenCapitalLetters(string input) => SpaceBetweenCapitalsRegex().Replace(input, " $1");

    /// <summary>
    ///     Returns whether this localized text actually has a proper localization entry.
    /// </summary>
    public static bool HasValidLocalizationValue(this LocalizedText text) => text.Key != text.Value;

    /// <summary>
    ///     Regex used to
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex("([A-Z])")]
    private static partial Regex SpaceBetweenCapitalsRegex();
}