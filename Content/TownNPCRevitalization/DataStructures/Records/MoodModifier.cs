using Terraria.Localization;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;

public record struct MoodModifier(LocalizedText ModifierDesc, LocalizedText FlavorText, float MoodOffset);