using System.Collections.Generic;
using System.Linq;
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
public sealed class TownNPCMoodModule : TownNPCModule {
    public const float MaxMoodValue = 100f;
    public const float MinMoodValue = 0f;

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

        // Princess does not use the profile system, using a hardcoded system instead. Thus, we need to instantiate her profile ourselves since that hardcoded system has been removed 
        PersonalityProfile princessProfile = new ();
        NPCPreferenceTrait princessPreferenceTrait = new()  { Level = AffectionLevel.Like, NpcId = NPCID.Princess };
        List<BiomePreferenceListTrait.BiomePreference> evilBiomePreferences = new List<IShoppingBiome>([new CorruptionBiome(), new CrimsonBiome(), new DungeonBiome()])
            .Select(biome => new BiomePreferenceListTrait.BiomePreference(AffectionLevel.Hate, biome))
            .ToList();
        foreach ((int npcType, PersonalityProfile profile) in Main.ShopHelper._database._personalityProfiles) {
            ModNPC potentialModNPC = NPCLoader.GetNPC(npcType);
            string npcTypeName = LWMUtils.GetNPCTypeNameOrIDName(npcType);
            string moodKeyPrefix = npcType >= NPCID.Count ? potentialModNPC.GetLocalizationKey("TownNPCMood") : $"TownNPCMood_{npcTypeName}";

            // All Town NPCs liking the Princess is not handled through NPCPreferenceTrait (and is instead hard-coded), as such we add a fake preference trait that will be translated numerically below
            List<NPCPreferenceTrait> npcPreferences = profile.ShopModifiers.OfType<NPCPreferenceTrait>().ToList();
            npcPreferences.Add(princessPreferenceTrait);

            foreach (NPCPreferenceTrait trait in npcPreferences) {
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

                string currentFlavorTextKey = $"{moodKeyPrefix}.{trait.Level}NPC";
                string otherNPCTypeName = LWMUtils.GetNPCTypeNameOrIDName(trait.NpcId);
                if (!LanguageManager.Instance._localizedTexts.TryGetValue($"{currentFlavorTextKey}_{otherNPCTypeName}", out LocalizedText currentFlavorText)) {
                    currentFlavorText = LanguageManager.Instance.GetText(currentFlavorTextKey);
                }

                string newKey = $"TownNPCMood.{npcTypeName}.NPC_{otherNPCTypeName}".PrependModKey();
                LanguageManager.Instance._moddedKeys.Add(newKey);
                LanguageManager.Instance._localizedTexts[newKey] = currentFlavorText;
            }

            profile.ShopModifiers.RemoveAll(trait => trait is NPCPreferenceTrait);

            // Evil biomes are also not handles through a BiomePreference similar to the NPC situation with the princess, thus we add some fake preferences traits for them to auto translate
            foreach (BiomePreferenceListTrait.BiomePreference preference in profile.ShopModifiers.OfType<BiomePreferenceListTrait>()
                .SelectMany(trait => trait.Preferences)
                .Concat(evilBiomePreferences)
                .ToList()) {
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

                LocalizedText currentText = Language.GetText($"{moodKeyPrefix}.{preference.Affection}Biome");

                string newKey = $"TownNPCMood.{npcTypeName}.Biome_{preference.Biome.NameKey}".PrependModKey();
                LanguageManager.Instance._moddedKeys.Add(newKey);
                LanguageManager.Instance._localizedTexts[newKey] = currentText;
            }

            profile.ShopModifiers.RemoveAll(trait => trait is BiomePreferenceListTrait);
            profile.ShopModifiers.AddRange([new CrowdingTrait(), new HomelessTrait(), new HomeProximityTrait(), new SpaciousTrait()]);

            string princessLoveFlavorTextKey = npcType >= NPCID.Count ? NPCLoader.GetNPC(npcType).GetLocalizationKey("TownNPCMood.Princess_LovesNPC") : $"TownNPCMood_Princess.LoveNPC_{npcTypeName}";
            string newPrincessKey = $"TownNPCMood.Princess.NPC_{npcTypeName}".PrependModKey();
            LanguageManager.Instance._moddedKeys.Add(newPrincessKey);
            LanguageManager.Instance._localizedTexts[newPrincessKey] = Language.GetText(princessLoveFlavorTextKey);

            princessProfile.ShopModifiers.Add(new NumericNPCPreferenceTrait(20, npcType));
        }

        princessProfile.ShopModifiers.AddRange([new HomelessTrait(), new HomeProximityTrait(), new LonelyTrait()]);
        Main.ShopHelper._database._personalityProfiles[NPCID.Princess] = princessProfile;
    }

    /// <summary>
    ///     Returns the flavor text localization key prefix for the given npc, accounting for if the npc is modded or not.
    /// </summary>
    public static string GetFlavorTextKeyPrefix(NPC npc) => npc.ModNPC is not null ? npc.ModNPC.GetLocalizationKey("TownNPCMood") : $"TownNPCMood_{NPCID.Search.GetName(npc.type)}";

    /// <summary>
    ///     Returns the flavor text localization key prefix for the given npc type, accounting for if the npc is modded or not.
    /// </summary>
    public static string GetFlavorTextKeyPrefix(int npcType) => npcType >= NPCID.Count ? NPCLoader.GetNPC(npcType).GetLocalizationKey("TownNPCMood") : $"TownNPCMood_{NPCID.Search.GetName(npcType)}";

    public override void Update() {
        for (int i = 0; i < _currentDynamicMoodModifiers.Count; i++) {
            MoodModifierInstance instance = _currentDynamicMoodModifiers[i];
            if (--instance.duration <= 0) {
                _currentDynamicMoodModifiers.RemoveAt(i--);
            }
        }
    }

    public void AddModifier(SubstitutableLocalizedText descriptionText, SubstitutableLocalizedText flavorText, int moodOffset, int duration) {
        MoodModifierInstance modifierInstance = new (descriptionText, flavorText, moodOffset, duration);
        if (duration <= 0) {
            _currentStaticMoodModifiers.Add(modifierInstance);
            return;
        }

        _currentDynamicMoodModifiers.Add(modifierInstance);
    }

    public void ResetStaticModifiers() {
        _currentStaticMoodModifiers.Clear();
    }
}