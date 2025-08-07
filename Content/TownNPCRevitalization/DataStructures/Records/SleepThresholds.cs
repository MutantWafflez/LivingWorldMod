using LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;

/// <summary>
///     Data that determines the <see cref="TownNPCSleepModule.awakeTicks" /> value thresholds; i.e. at what values a given NPC is "very well rested", "well rested", "tired", or "sleep deprived."
/// </summary>
/// <param name="TiredLimit">Max amount of awake ticks to be considered "tired."</param>
/// <param name="WellRestedLimit">Max amount of awake ticks to be considered "well rested."</param>
/// <param name="BestRestLimit">Max amount of awake ticks to be considered "very well rested."</param>
public readonly record struct SleepThresholds(int TiredLimit, int WellRestedLimit, int BestRestLimit);