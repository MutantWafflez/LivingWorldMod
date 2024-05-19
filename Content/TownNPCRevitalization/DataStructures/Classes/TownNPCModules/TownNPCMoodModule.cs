using System.Collections.Generic;
using System.Linq;
using Terraria.Localization;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;

/// <summary>
/// Class that handles the new "mood" feature that replaces Town NPC happiness.
/// </summary>
public sealed class TownNPCMoodModule : TownNPCModule {
    public record struct MoodModifier(LocalizedText ModifierDesc, LocalizedText FlavorText, float MoodOffset);

    private struct MoodModifierInstance(MoodModifier modifierType, int duration) {
        public readonly MoodModifier modifierType = modifierType;
        public int duration = duration;
    }

    public const float MaxMoodValue = 100f;
    public const float MinMoodValue = 0f;

    public static IReadOnlyDictionary<string, MoodModifier> moodModifiers;

    private const float BaseMoodValue = 50;

    public float CurrentMood => Utils.Clamp(BaseMoodValue + _currentMoodModifiers.Sum(modifier => modifier.modifierType.MoodOffset), MinMoodValue, MaxMoodValue);

    private readonly List<MoodModifierInstance> _currentMoodModifiers;

    public TownNPCMoodModule(NPC npc) : base(npc) {
        _currentMoodModifiers = [];
    }

    public override void Update() {
        for (int i = 0; i < _currentMoodModifiers.Count; i++) {
            MoodModifierInstance instance = _currentMoodModifiers[i];
            if (--instance.duration <= 0) {
                _currentMoodModifiers.RemoveAt(i--);
            }
        }
    }

    public void AddModifier(string modifierKey, int duration) {
        if (!moodModifiers.TryGetValue(modifierKey, out MoodModifier moodModifier)) {
            return;
        }

        _currentMoodModifiers.Add(new MoodModifierInstance(moodModifier, duration));
    }
}