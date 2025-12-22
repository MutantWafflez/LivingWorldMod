using Terraria.Localization;

namespace LivingWorldMod.DataStructures.Records;

/// <summary>
///     Simple record that couples a <see cref="LocalizedText" /> object with an <see cref="object" /> array that allows for formatting of the given <see cref="LocalizedText" />'s parameters into the
///     localized text, similar to how vanilla handles doing formatting with localizations. Also allows for registering a fallback in case the provided text doesn't have a valid localization entry,
///     which functions recursively, allowing for multiple fallbacks (if necessary).
/// </summary>
public record DynamicLocalizedText(LocalizedText OriginalText, object FormatObject = null, DynamicLocalizedText FallbackText = null) {
    public string FormattedString {
        get {
            if (!OriginalText.HasValidLocalizationValue()) {
                return FallbackString;
            }

            return FormatObject switch {
                null => OriginalText.Value,
                object[] formatArray => OriginalText.Format(formatArray),
                _ => OriginalText.CanFormatWith(FormatObject) ? OriginalText.FormatWith(FormatObject) : LocalizedText.Empty.Value
            };
        }
    }

    private string FallbackString => FallbackText?.FormattedString ?? "";

    public static implicit operator DynamicLocalizedText(LocalizedText text) => new (text);

    public override string ToString() => FormattedString;
}