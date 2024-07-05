namespace LivingWorldMod.Content.Villages.DataStructures.Structs;

/// <summary>
///     Struct that holds data for a given village type on their thresholds for reputation with the player.
/// </summary>
public struct ReputationThresholdData (int hateThreshold, int severeDislikeThreshold, int dislikeThreshold, int likeThreshold, int loveThreshold) {
    public int hateThreshold = hateThreshold;
    public int severeDislikeThreshold = severeDislikeThreshold;
    public int dislikeThreshold = dislikeThreshold;
    public int likeThreshold = likeThreshold;
    public int loveThreshold = loveThreshold;
}