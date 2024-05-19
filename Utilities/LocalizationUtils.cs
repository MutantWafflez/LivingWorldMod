using Terraria.Localization;

namespace LivingWorldMod.Utilities;

// Utilities class which holds methods that deals with localization.
public static partial class LWMUtils {
    /// <summary>
    /// Extension for strings that will turn the provided string into its <see cref="LocalizedText"/> equivalent, assuming the
    /// provided string is the key for a <see cref="LWM"/> localization file text value.
    /// </summary>
    public static LocalizedText Localized(this string key, params object[] args) => ModContent.GetInstance<LWM>().GetLocalization(key).WithFormatArgs(args);
}