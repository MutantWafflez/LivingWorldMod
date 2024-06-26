using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using Terraria.Localization;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;

public struct MoodModifierInstance(MoodModifier modifierType, LocalizedText flavorText, int duration, params string[] flavorTextParameters) {
    public readonly MoodModifier modifierType = modifierType;
    public readonly LocalizedText flavorText = flavorText;
    public readonly string[] flavorTextParameters = flavorTextParameters;
    public int duration = duration;
}