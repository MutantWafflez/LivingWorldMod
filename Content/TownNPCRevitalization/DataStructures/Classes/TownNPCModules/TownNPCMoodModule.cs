using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Hjson;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.ShopPersonalityTraits;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;
using LivingWorldMod.Utilities;
using Terraria.GameContent.Personalities;
using Terraria.Localization;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;

/// <summary>
///     Class that handles the new "mood" feature that replaces Town NPC happiness.
/// </summary>
public sealed partial class TownNPCMoodModule : TownNPCModule {
    public const float MaxMoodValue = 100f;
    public const float MinMoodValue = 0f;

    private static readonly Regex TownNPCNameRegex = LoadNPCNameRegex();

    private static Dictionary<string, LocalizedText> _defaultFlavorTexts;

    private readonly List<MoodModifierInstance> _currentStaticMoodModifiers;
    private readonly List<MoodModifierInstance> _currentDynamicMoodModifiers;

    public IReadOnlyList<MoodModifierInstance> CurrentDynamicMoodModifiers => _currentDynamicMoodModifiers;

    public IReadOnlyList<MoodModifierInstance> CurrentStaticMoodModifiers => _currentStaticMoodModifiers;

    public float CurrentMood => Utils.Clamp(
        BaseMoodValue + _currentDynamicMoodModifiers.Concat(_currentStaticMoodModifiers).Sum(modifier => modifier.moodOffset),
        MinMoodValue,
        MaxMoodValue
    );

    private static float BaseMoodValue {
        get {
            float baseValue = 50f;
            if (Main.expertMode) {
                baseValue -= 5f;
            }

            if (Main.masterMode) {
                baseValue -= 5f;
            }

            return baseValue;
        }
    }

    public TownNPCMoodModule(NPC npc) : base(npc) {
        _currentStaticMoodModifiers = [];
        _currentDynamicMoodModifiers = [];
        _defaultFlavorTexts = [];
    }

    public static void Load() {
        JsonObject jsonMoodValues = LWMUtils.GetJSONFromFile("Assets/JSONData/TownNPCMoodValues.json").Qo();

        JsonObject jsonEventPreferenceValues = LWMUtils.GetJSONFromFile("Assets/JSONData/TownNPCEventPreferences.json").Qo();
        foreach ((string npcName, JsonValue eventData) in jsonEventPreferenceValues) {
            int npcType = NPCID.Search.GetId(npcName);

            foreach ((string eventName, JsonValue affectionLevel) in eventData.Qo()) {
                Main.ShopHelper._database.Register(npcType, new EventPreferenceTrait(Enum.Parse<AffectionLevel>(affectionLevel), eventName));
            }
        }

        _defaultFlavorTexts = Language.FindAll(Lang.CreateDialogFilter("TownNPCMood.")).ToDictionary(text => text.Key);
    }

    [GeneratedRegex(@"(.+\.(?<Name>.+)\.TownNPCMood|TownNPCMood_(?<Name>.+))")]
    private static partial Regex LoadNPCNameRegex();

    public override void Update() {
        for (int i = 0; i < _currentDynamicMoodModifiers.Count; i++) {
            MoodModifierInstance instance = _currentDynamicMoodModifiers[i];
            if (--instance.duration <= 0) {
                _currentDynamicMoodModifiers.RemoveAt(i--);
            }
        }
    }

    public void AddModifier(string modifierKey, LocalizedText flavorText, int duration, object flavorTextSubstitutes = null) {
        if (flavorText.Key == flavorText.Value && _defaultFlavorTexts.TryGetValue(modifierKey, out LocalizedText defaultFlavorText)) {
            flavorText = defaultFlavorText;
        }

        MoodModifierInstance modifierInstance =  new MoodModifierInstance(moodModifier, flavorText, 0, flavorTextSubstitutes);
        if (duration <= 0) {
            _currentStaticMoodModifiers.Add(modifierInstance);
            return;
        }

        _currentDynamicMoodModifiers.Add(modifierInstance);
    }

    public void ConvertReportTextToStaticModifier(string townNPCLocalizationKey, string moodModifierKey, object flavorTextSubstituteObject = null) {
        if (TownNPCNameRegex.Match(townNPCLocalizationKey) is { } match && match != Match.Empty) {
            // We split moodModifierKey for scenarios such as LovesNPC_Princess, where we want the mood modifier to be "LovesNPC" as a catch-all
            AddModifier(moodModifierKey.Split('_')[0], Language.GetText($"{townNPCLocalizationKey}.{moodModifierKey}"), 0,  flavorTextSubstituteObject);
        }
    }

    public void ResetStaticModifiers() {
        _currentStaticMoodModifiers.Clear();
    }
}