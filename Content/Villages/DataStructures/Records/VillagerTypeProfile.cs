using System.Collections.Generic;

namespace LivingWorldMod.Content.Villages.DataStructures.Records;

public readonly record struct VillagerTypeProfile(
    List<string> PossibleNames,
    ReputationThresholdData ReputationThresholds,
    List<DialogueData> Dialogues
);