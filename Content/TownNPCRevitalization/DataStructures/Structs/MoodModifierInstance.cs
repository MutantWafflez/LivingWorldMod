using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using Terraria.Localization;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;

public struct MoodModifierInstance(MoodModifier modifierType, LocalizedText flavorText, int duration, object flavorTextSubstitutes = null) {
    public readonly MoodModifier modifierType = modifierType;
    public readonly LocalizedText flavorText = flavorText;
    public readonly object flavorTextSubstitutes = flavorTextSubstitutes;
    public int duration = duration;
}