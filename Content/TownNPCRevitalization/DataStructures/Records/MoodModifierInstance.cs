using LivingWorldMod.DataStructures.Records;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;

public record struct MoodModifierInstance(DynamicLocalizedText DescriptionText, DynamicLocalizedText FlavorText, int MoodOffset);