using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;

public struct MoodModifierInstance(MoodModifier modifierType, string flavorText, int duration, params string[] flavorTextParameters) {
    public readonly MoodModifier modifierType = modifierType;
    public readonly string flavorText = flavorText;
    public readonly string[] flavorTextParameters = flavorTextParameters;
    public int duration = duration;
}