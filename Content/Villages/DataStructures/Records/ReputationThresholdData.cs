namespace LivingWorldMod.Content.Villages.DataStructures.Records;

/// <summary>
///     Struct that holds data for a given village type on their thresholds for reputation with the player.
/// </summary>
public readonly record struct ReputationThresholdData(
    int Hate,
    int SevereDislike,
    int Dislike,
    int Like,
    int Love
);