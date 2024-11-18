using LivingWorldMod.DataStructures.Structs;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;

public struct MoodModifierInstance(DynamicLocalizedText descriptionText, DynamicLocalizedText flavorText, int moodOffset, int duration) {
    public readonly DynamicLocalizedText descriptionText = descriptionText;
    public readonly DynamicLocalizedText flavorText = flavorText;
    public readonly int moodOffset = moodOffset;
    public int duration = duration;
}