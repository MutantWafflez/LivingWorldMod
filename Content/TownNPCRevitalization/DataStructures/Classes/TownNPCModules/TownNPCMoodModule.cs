﻿using System.Collections.Generic;
using System.Linq;
using Hjson;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.PersonalityTraits;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;
using LivingWorldMod.DataStructures.Structs;
using LivingWorldMod.Utilities;
using Terraria.GameContent.Personalities;
using Terraria.Localization;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;

/// <summary>
///     Class that handles the new "mood" feature that replaces Town NPC happiness.
/// </summary>
public sealed class TownNPCMoodModule (NPC npc, TownGlobalNPC globalNPC) : TownNPCModule(npc, globalNPC) {
    public const float MaxMoodValue = 100f;
    public const float MinMoodValue = 0f;

    private static Dictionary<string, LocalizedText> _autoloadedFlavorTexts;

    private readonly List<MoodModifierInstance> _currentMoodModifiers = [];

    public static Dictionary<int, List<IPersonalityTrait>> PersonalityDatabase {
        get;
        private set;
    }

    public IReadOnlyList<MoodModifierInstance> CurrentMoodModifiers => _currentMoodModifiers;

    public float CurrentMood => Utils.Clamp(
        BaseMoodValue + CurrentMoodModifiers.Sum(instance => instance.moodOffset),
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

    public static LocalizedText GetAutoloadedFlavorTextOrDefault(string key) => !_autoloadedFlavorTexts.TryGetValue(key, out LocalizedText text) ? new LocalizedText(key, key) : text;

    public static void Load() {
        _autoloadedFlavorTexts = [];
        PersonalityDatabase = new Dictionary<int, List<IPersonalityTrait>>();

        // Princess does not use the profile system, using a hardcoded system instead. Thus, we need to instantiate her profile ourselves since that hardcoded system has been removed 
        List<IPersonalityTrait> princessProfile = [];
        NPCPreferenceTrait princessPreferenceTrait = new()  { Level = AffectionLevel.Like, NpcId = NPCID.Princess };
        List<BiomePreferenceListTrait.BiomePreference> evilBiomePreferences = new List<IShoppingBiome>([new CorruptionBiome(), new CrimsonBiome(), new DungeonBiome()])
            .Select(biome => new BiomePreferenceListTrait.BiomePreference(AffectionLevel.Hate, biome))
            .ToList();
        foreach ((int npcType, PersonalityProfile oldProfile) in Main.ShopHelper._database._personalityProfiles) {
            ModNPC potentialModNPC = NPCLoader.GetNPC(npcType);
            string npcTypeName = LWMUtils.GetNPCTypeNameOrIDName(npcType);
            string moodKeyPrefix = npcType >= NPCID.Count ? potentialModNPC.GetLocalizationKey("TownNPCMood") : $"TownNPCMood_{npcTypeName}";
            List<IPersonalityTrait> newPersonalityTraits = PersonalityDatabase[npcType] = [];

            // All Town NPCs liking the Princess is not handled through NPCPreferenceTrait (and is instead hard-coded), as such we add a fake preference trait that will be translated numerically 
            foreach (NPCPreferenceTrait trait in oldProfile.ShopModifiers.OfType<NPCPreferenceTrait>().Append(princessPreferenceTrait).ToList()) {
                newPersonalityTraits.Add(
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

                string newKey = $"{npcTypeName}.NPC_{otherNPCTypeName}";
                _autoloadedFlavorTexts[newKey] = currentFlavorText;
            }

            // Evil biomes are also not handles through a BiomePreference similar to the NPC situation with the princess, thus we add some fake preferences traits for them to auto translate
            foreach (BiomePreferenceListTrait.BiomePreference preference in oldProfile.ShopModifiers.OfType<BiomePreferenceListTrait>()
                .SelectMany(trait => trait.Preferences)
                .Concat(evilBiomePreferences)
                .ToList()) {
                newPersonalityTraits.Add(
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

                string newKey = $"{npcTypeName}.Biome_{preference.Biome.NameKey}";
                _autoloadedFlavorTexts[newKey] = new LocalizedText(
                    newKey,
                    currentText.FormatWith(new { BiomeName = Language.GetText($"TownNPCMoodBiomes.{preference.Biome.NameKey}") })
                );
            }

            newPersonalityTraits.AddRange([new CrowdingTrait(), new HomelessTrait(), new HomeProximityTrait(), new SpaciousTrait(), new SleepTrait()]);

            string princessLoveFlavorTextKey = npcType >= NPCID.Count ? NPCLoader.GetNPC(npcType).GetLocalizationKey("TownNPCMood.Princess_LovesNPC") : $"TownNPCMood_Princess.LoveNPC_{npcTypeName}";
            string newPrincessKey = $"Princess.NPC_{npcTypeName}";
            _autoloadedFlavorTexts[newPrincessKey] = Language.GetText(princessLoveFlavorTextKey);

            princessProfile.Add(new NumericNPCPreferenceTrait(20, npcType));
        }

        princessProfile.AddRange([new HomelessTrait(), new HomeProximityTrait(), new LonelyTrait(), new SleepTrait()]);
        PersonalityDatabase[NPCID.Princess] = princessProfile;

        JsonObject jsonEventPreferenceValues = LWMUtils.GetJSONFromFile("Assets/JSONData/TownNPCEventPreferences.json").Qo();
        foreach ((string npcName, JsonValue eventData) in jsonEventPreferenceValues) {
            int npcType = NPCID.Search.GetId(npcName);

            PersonalityDatabase[npcType].Add(new EventPreferencesTrait(eventData.Qo().Select(pair => new EventPreferencesTrait.EventPreference(pair.Key, pair.Value)).ToArray()));
        }
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
        if (Main.LocalPlayer.talkNPC == npc.whoAmI && Main.npcShop == 0)  {
            HappinessPatches.ProcessMoodOverride(Main.ShopHelper, Main.LocalPlayer, npc);
        }

        for (int i = 0; i < _currentMoodModifiers.Count; i++) {
            MoodModifierInstance instance = _currentMoodModifiers[i];
            if (instance.duration-- <= 0) {
                _currentMoodModifiers.RemoveAt(i--);
            }
            else {
                _currentMoodModifiers[i] = instance;
            }
        }
    }

    public void AddModifier(SubstitutableLocalizedText descriptionText, SubstitutableLocalizedText flavorText, int moodOffset, int duration = 1) {
        _currentMoodModifiers.Add(new MoodModifierInstance (descriptionText, flavorText, moodOffset, duration));
    }
}