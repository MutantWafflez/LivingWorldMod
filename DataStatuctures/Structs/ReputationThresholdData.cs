namespace LivingWorldMod.DataStatuctures.Structs;

/// <summary>
/// Struct that holds data for a given village type on their thresholds for reputation with the player.
/// </summary>
public struct ReputationThresholdData {
    public int hateThreshold;
    public int severeDislikeThreshold;
    public int dislikeThreshold;
    public int likeThreshold;
    public int loveThreshold;

    public ReputationThresholdData(int hateThreshold, int severeDislikeThreshold, int dislikeThreshold, int likeThreshold, int loveThreshold) {
        this.hateThreshold = hateThreshold;
        this.severeDislikeThreshold = severeDislikeThreshold;
        this.dislikeThreshold = dislikeThreshold;
        this.likeThreshold = likeThreshold;
        this.loveThreshold = loveThreshold;
    }
}