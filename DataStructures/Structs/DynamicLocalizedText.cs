using LivingWorldMod.Utilities;
using Terraria.Localization;

namespace LivingWorldMod.DataStructures.Structs;

/// <summary>
///     Simple struct that couples a <see cref="LocalizedText" /> object with an anonymous <see cref="object" /> that allows for subsitution of the given <see cref="object" />'s parameters into the
///     localized text, similar to how vanilla handles doing subsitutions with localizations. Also allows for registering a fallback in case the provided text doesn't have a valid localization entry.
/// </summary>
public readonly struct DynamicLocalizedText(LocalizedText text, object[] formatArray = null, LocalizedText fallbackText = null) {
    public readonly LocalizedText text = text;

    public string SubstitutedText   {
        get {
            if (!text.HasValidLocalizationValue()) {
                return FallbackText;
            }

            return formatArray is not null ? text.Format(formatArray) : text.Value;
        }
    }

    public string FallbackText => fallbackText?.Value ?? "";

    public static implicit operator DynamicLocalizedText(LocalizedText text) => new (text);

    public override string ToString() => SubstitutedText;
}