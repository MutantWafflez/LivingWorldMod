using LivingWorldMod.DataStructures.Structs;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;

public struct MoodModifierInstance(SubstitutableLocalizedText descriptionText, SubstitutableLocalizedText flavorText, int moodOffset, int duration) {
    public readonly SubstitutableLocalizedText descriptionText = descriptionText;
    public readonly SubstitutableLocalizedText flavorText = flavorText;
    public readonly int moodOffset = moodOffset;
    public int duration = duration;
}