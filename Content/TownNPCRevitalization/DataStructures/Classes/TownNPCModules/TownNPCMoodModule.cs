using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Hjson;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.ShopPersonalityTraits;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;
using LivingWorldMod.DataStructures.Structs;
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

    private static int BaseMoodValue {
        get {
            int baseValue = 50;
            if (Main.expertMode) {
                baseValue -= 5;
            }

            if (Main.masterMode) {
                baseValue -= 5;
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
        // JsonObject jsonMoodValues = LWMUtils.GetJSONFromFile("Assets/JSONData/TownNPCMoodValues.json").Qo();
        JsonObject jsonEventPreferenceValues = LWMUtils.GetJSONFromFile("Assets/JSONData/TownNPCEventPreferences.json").Qo();
        foreach ((string npcName, JsonValue eventData) in jsonEventPreferenceValues) {
            int npcType = NPCID.Search.GetId(npcName);

            foreach ((string eventName, JsonValue moodOffset) in eventData.Qo()) {
                Main.ShopHelper._database.Register(npcType, new EventPreferenceTrait(moodOffset, eventName));
            }
        }

        foreach ((int npcType, PersonalityProfile profile) in Main.ShopHelper._database._personalityProfiles) {
            ModNPC potentialModNPC = NPCLoader.GetNPC(npcType);
            string npcTypeName = LWMUtils.GetTypeNameOrIDName(npcType);
            string moodKeyPrefix = npcType >= NPCID.Count ? potentialModNPC.GetLocalizationKey("TownNPCMood") : $"TownNPCMood_{npcTypeName}";

            foreach (NPCPreferenceTrait trait in profile.ShopModifiers.OfType<NPCPreferenceTrait>().ToList()) {
                profile.ShopModifiers.Add(
                    new NumericNPCPreferenceTrait(
                        trait.Level switch {
                            AffectionLevel.Love => 20,
                            AffectionLevel.Like => 10,
                            AffectionLevel.Dislike => -10,
                            AffectionLevel.Hate => -20,
                            _ => 0
                        },
                        trait.NpcId
                    )
                );

                LocalizedText currentText = LanguageManager.Instance.GetText($"{moodKeyPrefix}.{trait.Level}NPC");
                string otherNPCTypeName = LWMUtils.GetTypeNameOrIDName(trait.NpcId);

                string newKey = $"TownNPCMood.{npcTypeName}.NPC_{otherNPCTypeName}".PrependModKey();
                LanguageManager.Instance._moddedKeys.Add(newKey);
                LanguageManager.Instance._localizedTexts[newKey] = currentText;
            }

            profile.ShopModifiers.RemoveAll(trait => trait is NPCPreferenceTrait);

            foreach (BiomePreferenceListTrait.BiomePreference preference in profile.ShopModifiers.OfType<BiomePreferenceListTrait>().ToList().SelectMany(trait => trait.Preferences)) {
                profile.ShopModifiers.Add(
                    new NumericBiomePreferenceTrait(
                        preference.Affection switch {
                            AffectionLevel.Love => 30,
                            AffectionLevel.Like => 15,
                            AffectionLevel.Dislike => -15,
                            AffectionLevel.Hate => -30,
                            _ => 0
                        },
                        preference.Biome
                    )
                );

                LocalizedText currentText = LanguageManager.Instance.GetText($"{moodKeyPrefix}.{preference.Affection}Biome");

                string newKey = $"TownNPCMood.{npcTypeName}.Biome_{preference.Biome.NameKey}".PrependModKey();
                LanguageManager.Instance._moddedKeys.Add(newKey);
                LanguageManager.Instance._localizedTexts[newKey] = currentText;
            }

            profile.ShopModifiers.RemoveAll(trait => trait is BiomePreferenceListTrait);
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

    public void AddModifier(SubstitutableLocalizedText descriptionText, SubstitutableLocalizedText flavorText, int moodOffset, int duration) {
        // if (flavorText.text.Key == flavorText.text.Value && _defaultFlavorTexts.TryGetValue(modifierKey, out LocalizedText defaultFlavorText)) {
        //     flavorText = defaultFlavorText;
        // }

        MoodModifierInstance modifierInstance = new (descriptionText, flavorText, moodOffset, duration);
        if (duration <= 0) {
            _currentStaticMoodModifiers.Add(modifierInstance);
            return;
        }

        _currentDynamicMoodModifiers.Add(modifierInstance);
    }

    // public void ConvertReportTextToStaticModifier(string townNPCLocalizationKey, string moodModifierKey, object flavorTextSubstituteObject = null) {
    //     if (TownNPCNameRegex.Match(townNPCLocalizationKey) is { } match && match != Match.Empty) {
    //         // We split moodModifierKey for scenarios such as LovesNPC_Princess, where we want the mood modifier to be "LovesNPC" as a catch-all
    //         AddModifier(moodModifierKey.Split('_')[0], Language.GetText($"{townNPCLocalizationKey}.{moodModifierKey}"), 0,  flavorTextSubstituteObject);
    //     }
    // }

    public void ResetStaticModifiers() {
        _currentStaticMoodModifiers.Clear();
    }
}