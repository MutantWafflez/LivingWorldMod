using Newtonsoft.Json;

namespace LivingWorldMod.Content.Villages.DataStructures.Records;

/// <summary>
///     Struct that holds data for a given village type on their thresholds for reputation with the player.
/// </summary>
public readonly record struct ReputationThresholdData(
    [JsonProperty(PropertyName = "Hate")] int HateThreshold,
    [JsonProperty(PropertyName = "SevereDislike")]
    int SevereDislikeThreshold,
    [JsonProperty(PropertyName = "Dislike")]
    int DislikeThreshold,
    [JsonProperty(PropertyName = "Like")] int LikeThreshold,
    [JsonProperty(PropertyName = "Love")] int LoveThreshold
);