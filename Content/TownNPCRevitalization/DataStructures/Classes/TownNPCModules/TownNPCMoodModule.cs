using System.Collections.Generic;
using System.Linq;
using Hjson;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;
using LivingWorldMod.Utilities;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;

/// <summary>
///     Class that handles the new "mood" feature that replaces Town NPC happiness.
/// </summary>
public sealed class TownNPCMoodModule : TownNPCModule {
    public const float MaxMoodValue = 100f;
    public const float MinMoodValue = 0f;

    private const float BaseMoodValue = 50;

    private static IReadOnlyDictionary<string, MoodModifier> _moodModifiers;

    private readonly List<MoodModifierInstance> _currentMoodModifiers;

    public float CurrentMood => Utils.Clamp(BaseMoodValue + _currentMoodModifiers.Sum(modifier => modifier.modifierType.MoodOffset), MinMoodValue, MaxMoodValue);

    public TownNPCMoodModule(NPC npc) : base(npc) {
        _currentMoodModifiers = [];
    }

    public override void Load() {
        JsonObject jsonMoodValues = LWMUtils.GetJSONFromFile("Assets/JSONData/TownNPCMoodValues.json").Qo();
        Dictionary<string, MoodModifier> moodModifierDict = [];
        //TODO: Populate vanilla flavor text
        foreach ((string moodModifierKey, float moodOffset) in jsonMoodValues) {
            moodModifierDict[moodModifierKey] = new MoodModifier($"TowNPCMoodDescription.{moodModifierKey}".Localized(), $"TownNPCMoodFlavorText.{moodModifierKey}".Localized(), moodOffset);
        }
        _moodModifiers = moodModifierDict;
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
        if (!_moodModifiers.TryGetValue(modifierKey, out MoodModifier moodModifier)) {
            return;
        }

        _currentMoodModifiers.Add(new MoodModifierInstance(moodModifier, duration));
    }
}