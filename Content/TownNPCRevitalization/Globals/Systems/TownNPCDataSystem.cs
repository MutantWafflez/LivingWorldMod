using System;
using System.Collections.Generic;
using System.Linq;
using Hjson;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.PersonalityTraits;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.DataStructures.Structs;
using LivingWorldMod.Globals.BaseTypes.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;
using Terraria.Localization;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;

/// <summary>
///     ModSystem that holds all of the static data needed for the Town NPC Revitalization to function.
/// </summary>
public class TownNPCDataSystem : BaseModSystem<TownNPCDataSystem> {
    private static readonly Point DefaultOverlayCornerOne = new (int.MaxValue, int.MaxValue);
    private static readonly TownNPCSpriteOverlay DefaultSpriteOverlay = new(Asset<Texture2D>.DefaultValue, Vector2.Zero);

    private static Dictionary<string, LocalizedText> _autoloadedFlavorTexts;
    
    private static readonly SleepThresholds DefaultSleepThresholds = new (LWMUtils.InGameHour * 17, LWMUtils.InGameHour * 13, LWMUtils.InGameHour * 5);
    private static readonly SleepSchedule DefaultSleepSchedule = new(new TimeOnly(19, 30, 0), new TimeOnly(4, 30, 0));

    public static Dictionary<int, TownNPCProfile> ProfileDatabase {
        get;
        private set;
    }

    public static LocalizedText GetAutoloadedFlavorTextOrDefault(string key) => !_autoloadedFlavorTexts.TryGetValue(key, out LocalizedText text) ? new LocalizedText(key, key) : text;

    private static TownNPCSpriteOverlay GenerateOverlayFromDifferenceBetweenFrames(
        in ArrayInterpreter<Color> rawTextureData,
        Rectangle frameOne,
        Rectangle frameTwo,
        string overlayName
    ) {
        Point overlayCornerOne = DefaultOverlayCornerOne;
        Point overlayCornerTwo = new (-1, -1);
        for (int i = 0; i < frameOne.Height; i++) {
            for (int j = 0; j < frameOne.Width; j++) {
                Color firstFramePixelColor = rawTextureData.GetAtPosition(frameOne.Y + i, frameOne.X + j);
                Color secondFramePixelColor = rawTextureData.GetAtPosition(frameTwo.Y + i, frameTwo.X + j);
                if (firstFramePixelColor == secondFramePixelColor) {
                    continue;
                }

                overlayCornerOne.X = Math.Min(overlayCornerOne.X, j);
                overlayCornerOne.Y = Math.Min(overlayCornerOne.Y, i);

                overlayCornerTwo.X = Math.Max(overlayCornerTwo.X, j);
                overlayCornerTwo.Y = Math.Max(overlayCornerTwo.Y, i);
            }
        }

        if (overlayCornerOne == DefaultOverlayCornerOne) {
            return DefaultSpriteOverlay;
        }

        Rectangle differenceRectangle = LWMUtils.NewRectFromCorners(overlayCornerOne, overlayCornerTwo + new Point(1, 1));
        Color[] colorDifference = new Color[differenceRectangle.Width * differenceRectangle.Height];
        for (int i = 0; i < differenceRectangle.Height; i++) {
            for (int j = 0; j < differenceRectangle.Width; j++) {
                colorDifference[i * differenceRectangle.Width + j] = rawTextureData.GetAtPosition(overlayCornerOne.Y + frameTwo.Y + i, overlayCornerOne.X + frameTwo.X + j);
            }
        }

        Texture2D overlayTexture = new (Main.graphics.GraphicsDevice, differenceRectangle.Width, differenceRectangle.Height);
        overlayTexture.SetData(colorDifference);
        overlayTexture.Name = overlayName;

        return new TownNPCSpriteOverlay(overlayTexture, differenceRectangle.TopLeft());
    }

    private static TownNPCSpriteOverlay[] GenerateTownNPCSpriteOverlays(string npcAssetName, Texture2D npcTexture, int npcType) {
        int npcFrameCount = Main.npcFrameCount[npcType];
        int totalPixelArea = npcTexture.Width * npcTexture.Height;
        int nonAttackFrameCount = npcFrameCount - NPCID.Sets.AttackFrameCount[npcType];

        Color[] rawTextureData = new Color[totalPixelArea];
        ArrayInterpreter<Color> rawTextureInterpreter = new (rawTextureData, npcTexture.Height, npcTexture.Width);
        npcTexture.GetData(rawTextureData);

        Rectangle defaultFrameRectangle = npcTexture.Frame(verticalFrames: npcFrameCount);

        /*
         * A quick explanation of what (and more importantly, WHY) this triple for loop is necessary:
         * In general, vanilla Town NPC sprites follow a few guidelines, with some being stricter than others. The frame order is always the same (minus the attacking frames, but those are the same
         * amongst NPCs of the same attack type), and that's basically where the similarities end and the exceptions begin. Namely, the exceptions come in the form of head offset; since we extract
         * both the eyelid texture and mouth texture, which depend on head position, we need a way to determine if the NPC's head has moved. There's a lot of ways you could do it, but I took this
         * method as it is simple and is generally applicable to all vanilla Town NPCs. The short of it is that we go through each of the NPC's frames, and note down the Y position of the top most
         * pixel of that frame, and take the difference between that and the very first frame's top most Y pixel. The first frame is the "default" frame, and thus its the base-line by which we can
         * calculate if a frame causes the NPC's head to move up or down. These differences then passed to our overlay structs down below, where our Sprite Module handles ofsetting them, where
         * applicable. Not fool-proof by any means, but it appears to do the job for vanilla Town NPC's. Modded Town NPCs definitely have the capacity to absolutely destroy this, but so be it. Not
         * going to do computer vision image registration/recognition to optimally calculate where the eyes and mouth move for every frame.
         */
        Dictionary<int, Vector2> additionalFrameOffsets = new ();
        int? defaultFrameYPos = null;
        for (int frameNumber = 0; frameNumber < npcFrameCount; frameNumber++) {
            for (int i = 0; i < defaultFrameRectangle.Height; i++) {
                for (int j = 0; j < defaultFrameRectangle.Width; j++) {
                    if (rawTextureInterpreter.GetAtPosition(defaultFrameRectangle.Height * frameNumber + i, j) == default(Color)) {
                        continue;
                    }

                    // If this check succeeds, this means this is the first frame and we need to record the default y offset here
                    if (defaultFrameYPos is not { } defaultYPos) {
                        defaultFrameYPos = i;
                        goto ContinueOutermostLoop;
                    }

                    if (defaultYPos != i) {
                        additionalFrameOffsets[frameNumber] = new Vector2(0, i - defaultYPos);
                    }

                    goto ContinueOutermostLoop;
                }
            }

            ContinueOutermostLoop: ;
        }

        int talkingFrame = nonAttackFrameCount - 2;
        int blinkFrame = nonAttackFrameCount - 1;
        if (talkingFrame <= 0 || blinkFrame <= 0) {
            return [DefaultSpriteOverlay, DefaultSpriteOverlay];
        }

        Rectangle talkingFrameRectangle = npcTexture.Frame(verticalFrames: npcFrameCount, frameY: talkingFrame);
        Rectangle blinkingFrameRectangle = npcTexture.Frame(verticalFrames: npcFrameCount, frameY: blinkFrame);

        string textureNamePrefix = $"{npcAssetName[(npcAssetName.LastIndexOf("\\") + 1)..]}";
        (Rectangle, string)[] frameNameDifferenceArray = [(talkingFrameRectangle, "Talking"), (blinkingFrameRectangle, "Blinking")];
        List<TownNPCSpriteOverlay> returnList = [];
        foreach ((Rectangle secondFrame, string resultingOverlaySuffix) in frameNameDifferenceArray) {
            returnList.Add(
                GenerateOverlayFromDifferenceBetweenFrames(
                    rawTextureInterpreter,
                    defaultFrameRectangle,
                    secondFrame,
                    $"{textureNamePrefix}_{resultingOverlaySuffix}"
                ) with {
                    AdditionalFrameOffsets = additionalFrameOffsets
                }
            );
        }

        return returnList.ToArray();
    }

    private static void LoadPersonalities() {
        _autoloadedFlavorTexts = [];

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
            List<IPersonalityTrait> newPersonalityTraits = [];

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
                .ToList()
            ) {
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

            newPersonalityTraits.AddRange([new CrowdingTrait(), new HomelessTrait(), new HomeProximityTrait(), new SpaciousTrait(), new SleepTrait(), new TaxesTrait()]);

            string princessLoveFlavorTextKey = npcType >= NPCID.Count ? NPCLoader.GetNPC(npcType).GetLocalizationKey("TownNPCMood.Princess_LovesNPC") : $"TownNPCMood_Princess.LoveNPC_{npcTypeName}";
            string newPrincessKey = $"Princess.NPC_{npcTypeName}";
            _autoloadedFlavorTexts[newPrincessKey] = Language.GetText(princessLoveFlavorTextKey);

            princessProfile.Add(new NumericNPCPreferenceTrait(20, npcType));
            ProfileDatabase[npcType].Traits.AddRange(newPersonalityTraits);
        }

        princessProfile.AddRange([new HomelessTrait(), new HomeProximityTrait(), new LonelyTrait(), new SleepTrait()]);
        ProfileDatabase[NPCID.Princess].Traits.AddRange(princessProfile);
    }

    public override void Unload() {
        if (ProfileDatabase is not null) {
            Main.QueueMainThreadAction(() => {
                    foreach (OverlayProfile spriteProfile in ProfileDatabase.Select(pair => pair.Value.OverlayProfile)) {
                        spriteProfile?.Dispose();
                    }
                }
            );
        }
    }

    private static SleepSchedule ParseSleepSchedule(JsonObject sleepScheduleJson) {
        return new SleepSchedule(TimeOnly.Parse(sleepScheduleJson["Start"]), TimeOnly.Parse(sleepScheduleJson["End"]));
    }

    private static List<EventPreferencesTrait.EventPreference> ParseEventPreferences(JsonObject eventPrefsJson) {
        List<EventPreferencesTrait.EventPreference> eventPreferences = [];
        
        foreach ((string eventName, JsonValue eventValue) in eventPrefsJson) {
            eventPreferences.Add(new EventPreferencesTrait.EventPreference(eventName, eventValue.Qi()));
        }

        return eventPreferences;
    }

    public override void PostSetupContent() {
        ProfileDatabase = []; 
        
        NPC npc = new();
        for (int i = 0; i < NPCLoader.NPCCount; i++) {
            npc.SetDefaults(i);
            if (!TownGlobalNPC.IsAnyValidTownNPC(npc, true)) {
                continue;
            }

            ProfileDatabase[i] = new TownNPCProfile(new TownNPCAttackData(), [], DefaultSleepSchedule, DefaultSleepThresholds, null);
        }

        JsonObject townNPCData = LWMUtils.GetJsonFromHjsonFile("Assets/Json/TownNPCData.t_hjson").Qo();
        foreach ((string npcName, JsonValue npcData) in townNPCData) {
            int npcType = NPCID.Search.GetId(npcName);
            TownNPCProfile currentProfile = ProfileDatabase[npcType];

            ProfileDatabase[npcType] = currentProfile with {
                AttackData = JsonConvert.DeserializeObject<TownNPCAttackData>(npcData["AttackData"].ToString()),
                Traits = [new EventPreferencesTrait(ParseEventPreferences(npcData["EventPrefs"].Qo()).ToArray())],
                SleepSchedule = npcData.ContainsKey("SleepSchedule") ? ParseSleepSchedule(npcData["SleepSchedule"].Qo()) : currentProfile.SleepSchedule
            };
        }
        
        LoadPersonalities();
        
        if (Main.netMode != NetmodeID.Server) {
            Main.QueueMainThreadAction(GenerateTownNPCSpriteProfiles);
        }
    }

    private static void GenerateTownNPCSpriteProfiles() {
        NPC npc = new();
        foreach (int npcType in ProfileDatabase.Keys) {
            npc.SetDefaults(npcType);
            if (!TownGlobalNPC.IsValidFullTownNPC(npc, true)) {
                continue;
            }
            
            TownNPCProfile currentProfile = ProfileDatabase[npcType];
            AssetRepository assetRepo = npcType >= NPCID.Count ? NPCLoader.GetNPC(npcType).Mod.Assets : (AssetRepository)Main.Assets;
            Asset<Texture2D> npcAsset;
            if (!TownNPCProfiles.Instance.GetProfile(npc, out ITownNPCProfile profile)) {
                npcAsset = TextureAssets.Npc[npcType];
                ProfileDatabase[npcType] = currentProfile with {
                    OverlayProfile = new OverlayProfile(GenerateTownNPCSpriteOverlays(npcAsset.Name, npcAsset.EnsureAssetLoaded(assetRepo).Value, npcType))
                };
                continue;
            }

            int npcVariationCount = profile switch {
                Profiles.StackedNPCProfile stackedProfile => stackedProfile._profiles.Length,
                Profiles.VariantNPCProfile variantProfile => variantProfile._variants.Length,
                _ => 1
            };

            List<TownNPCSpriteOverlay[]> spriteOverlays = [];
            for (int j = 0; j < npcVariationCount; npc.townNpcVariationIndex = ++j) {
                npcAsset = profile.GetTextureNPCShouldUse(npc);
                spriteOverlays.Add(GenerateTownNPCSpriteOverlays(npcAsset.Name, npcAsset.EnsureAssetLoaded(assetRepo).Value, npcType));
            }

            ProfileDatabase[npcType] = currentProfile with { OverlayProfile = new OverlayProfile(spriteOverlays.ToArray()) };
        }
    }
}