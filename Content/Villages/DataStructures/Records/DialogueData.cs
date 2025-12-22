namespace LivingWorldMod.Content.Villages.DataStructures.Records;

/// <summary>
///     Data Structure that holds data on a specific line of dialogue, including its translation key, weight (if applicable), and if
///     any events are required for it to appear.
/// </summary>
/// <param name="DialogueKey">The actual key of the dialogue.</param>
/// <param name="Weight">The weight of this dialogue, taken into account when selecting a dialogue line from a list.</param>
/// <param name="Priority">The priority of this dialogue. The highest priority dialogues will be chosen.</param>
/// <param name="RequiredEvents">If any events are required for this line, this array holds them. Null if no events are required.</param>
public readonly record struct DialogueData (string DialogueKey, double Weight, int Priority, string[] RequiredEvents) {
    public override string ToString() =>
        $"Key: {DialogueKey.Replace("Mods.LivingWorldMod.VillagerDialogue.", "...")} Weight: {Weight} Events: {(RequiredEvents is null ? "None" : string.Join(", ", RequiredEvents))}";
}