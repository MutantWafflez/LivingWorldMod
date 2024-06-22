using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;

public struct MoodModifierInstance(MoodModifier modifierType, int duration) {
    public readonly MoodModifier modifierType = modifierType;
    public int duration = duration;
}