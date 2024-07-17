using Terraria.Localization;

namespace LivingWorldMod.DataStructures.Structs;

/// <summary>
///     Simple struct that couples a <see cref="LocalizedText" /> object with an anonymous <see cref="object" /> that allows for subsitution of the given <see cref="object" />'s parameters into the
///     localized text, similar to how vanilla handles doing subsitutions with localizations.
/// </summary>
public readonly struct SubstitutableLocalizedText(LocalizedText text, object substitutionObject = null) {
    public readonly LocalizedText text = text;

    public string SubstitutedText => substitutionObject is null ? text.Value : text.FormatWith(substitutionObject);

    public static implicit operator SubstitutableLocalizedText(LocalizedText text) => new (text);

    public override string ToString() => SubstitutedText;
}