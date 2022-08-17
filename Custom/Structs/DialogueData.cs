using System.Linq;
using Terraria.ModLoader;

namespace LivingWorldMod.Custom.Structs {
    /// <summary>
    /// Struct that holds data on a specific line of dialogue, including its translation key, weight (if applicable), and if any events are
    /// required for it to appear.
    /// </summary>
    public readonly struct DialogueData {
        /// <summary>
        /// The actual text of the dialogue.
        /// </summary>
        public readonly ModTranslation dialogue;

        /// <summary>
        /// The weight of this dialogue, taken into account when selecting a dialogue line from a list.
        /// </summary>
        public readonly double weight;

        /// <summary>
        /// The priority of this dialogue. The highest priority dialogues will be chosen.
        /// </summary>
        public readonly int priority;

        /// <summary>
        /// If any events are required for this line, this array holds them. Null if no events are required.
        /// </summary>
        public readonly string[] requiredEvents;

        public DialogueData(ModTranslation dialogue, double weight, int priority, string[] requiredEvents) {
            this.dialogue = dialogue;
            this.weight = weight;
            this.priority = priority;
            this.requiredEvents = requiredEvents;
        }

        public override string ToString() => $"Key: {dialogue.Key.Replace("Mods.LivingWorldMod.VillagerDialogue.", "...")} Weight: {weight} Events: {(requiredEvents is null ? "None" : string.Join(", ", requiredEvents))}";
    }
}