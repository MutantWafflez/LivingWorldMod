using System.Collections.Generic;
using System.Linq;

namespace LivingWorldMod.Custom.Classes;

/// <summary>
/// Class that handles the new "mood" feature that replaces Town NPC happiness.
/// </summary>
public sealed class TownNPCMoodModule : TownNPCModule {
    // TODO: Add stack functionality
    public sealed record MoodModifier(float MoodOffset, int MaxStacks);

    private sealed class MoodModifierInstance {
        public readonly MoodModifier modifier;
        public readonly string flavorText;
        public int duration;

        public MoodModifierInstance(MoodModifier modifier, string flavorText, int duration) {
            this.modifier = modifier;
            this.flavorText = flavorText;
            this.duration = duration;
        }
    }

    public const float MaxMoodValue = 100f;
    public const float MinMoodValue = 0f;

    public static IReadOnlyDictionary<string, MoodModifier> moodModifiers;

    private const float BaseMoodValue = 50;

    public float CurrentMood => Utils.Clamp(BaseMoodValue + _currentMoodModifiers.Sum(modifier => modifier.modifier.MoodOffset), MinMoodValue, MaxMoodValue);

    private readonly List<MoodModifierInstance> _currentMoodModifiers;

    public TownNPCMoodModule(NPC npc) : base(npc) {
        _currentMoodModifiers = new List<MoodModifierInstance>();
    }

    public override void Update() {
        for (int i = 0; i < _currentMoodModifiers.Count; i++) {
            MoodModifierInstance instance = _currentMoodModifiers[i];
            if (--instance.duration <= 0) {
                _currentMoodModifiers.RemoveAt(i--);
            }
        }
    }

    public void AddModifier(string modifierKey, string flavorText, int duration) {
        if (!moodModifiers.TryGetValue(modifierKey, out MoodModifier modifier)) {
            return;
        }

        _currentMoodModifiers.Add(new MoodModifierInstance(modifier, flavorText, duration));
    }

    public List<(string, float)> GetFlavorTextAndModifiers() => _currentMoodModifiers.OrderByDescending(modifier => modifier.modifier.MoodOffset).Select(modifier => (modifier.flavorText, modifier.modifier.MoodOffset)).ToList();
}