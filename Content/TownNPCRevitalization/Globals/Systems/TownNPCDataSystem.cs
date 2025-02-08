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
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;
using Terraria.Localization;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;

/// <summary>
///     ModSystem that holds all of the static data needed for the Town NPC Revitalization to function.
/// </summary>
public class TownNPCDataSystem : BaseModSystem<TownNPCDataSystem> {
    public static Dictionary<int, SleepSchedule> sleepSchedules;

    public static Dictionary<int, TownNPCProjAttackData> projectileAttackDatas;
    public static Dictionary<int, TownNPCMeleeAttackData> meleeAttackDatas;

    public static IReadOnlyDictionary<int, TownNPCOverlayProfile> spriteOverlayProfiles;

    private static readonly Point DefaultOverlayTopLeft = new (-1, -1);

    private static Dictionary<string, LocalizedText> _autoloadedFlavorTexts;

    public static Dictionary<int, List<IPersonalityTrait>> PersonalityDatabase {
        get;
        private set;
    }

    public static LocalizedText GetAutoloadedFlavorTextOrDefault(string key) => !_autoloadedFlavorTexts.TryGetValue(key, out LocalizedText text) ? new LocalizedText(key, key) : text;

    private static TownNPCSpriteOverlay GenerateOverlayFromDifferenceBetweenFrames(
        Color[] rawTextureData,
        int textureWidth,
        int textureHeight,
        Rectangle frameOne,
        Rectangle frameTwo,
        string overlayName
    ) {
        // Color[] colorDifference = new Color[frameOne.Width * frameTwo.Height];
        Point overlayTopLeft = DefaultOverlayTopLeft;
        Point overlayBottomRight = DefaultOverlayTopLeft;
        for (int i = 0; i < frameOne.Height; i++) {
            for (int j = 0; j < frameOne.Width; j++) {
                Color firstFramePixelColor = rawTextureData.GetValueAsNDimensionalArray(
                    new ArrayDimensionData(frameOne.Y + i, textureHeight),
                    new ArrayDimensionData(frameOne.X + j, textureWidth)
                );
                Color secondFramePixelColor = rawTextureData.GetValueAsNDimensionalArray(
                    new ArrayDimensionData(frameTwo.Y + i, textureHeight),
                    new ArrayDimensionData(frameTwo.X + j, textureWidth)
                );
                if (firstFramePixelColor == secondFramePixelColor) {
                    continue;
                }

                if (overlayTopLeft == DefaultOverlayTopLeft) {
                    overlayTopLeft = overlayBottomRight = new Point(j, i);
                    continue;
                }

                overlayBottomRight.X = Math.Max(overlayBottomRight.X, j);
                overlayBottomRight.Y = Math.Max(overlayBottomRight.Y, i);
                // colorDifference[i * frameOne.Width + j] = secondFramePixelColor;
            }
        }

        if (overlayTopLeft == DefaultOverlayTopLeft) {
            return new TownNPCSpriteOverlay(new Texture2D(Main.graphics.GraphicsDevice, 1, 1), Vector2.Zero);
        }

        Rectangle differenceRectangle = LWMUtils.NewRectFromCorners(overlayTopLeft, overlayBottomRight + new Point(1, 1));
        Color[] colorDifference = new Color[differenceRectangle.Width * differenceRectangle.Height];
        for (int i = 0; i < differenceRectangle.Height; i++) {
            for (int j = 0; j < differenceRectangle.Width; j++) {
                colorDifference[i * differenceRectangle.Width + j] = rawTextureData.GetValueAsNDimensionalArray(
                    new ArrayDimensionData(overlayTopLeft.Y + frameTwo.Y + i, textureHeight),
                    new ArrayDimensionData(overlayTopLeft.X + frameTwo.X + j, textureWidth)
                );
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
        npcTexture.GetData(rawTextureData);

        Rectangle defaultFrameRectangle = npcTexture.Frame(verticalFrames: npcFrameCount);
        Rectangle talkingFrameRectangle = npcTexture.Frame(verticalFrames: npcFrameCount, frameY: nonAttackFrameCount - 2);
        Rectangle blinkingFrameRectangle = npcTexture.Frame(verticalFrames: npcFrameCount, frameY: nonAttackFrameCount - 1);

        string textureNamePrefix = $"{npcAssetName[(npcAssetName.LastIndexOf("\\") + 1)..]}";
        (Rectangle, string)[] frameNameDifferenceArray = [(talkingFrameRectangle, "Talking"), (blinkingFrameRectangle, "Blinking")];
        List<TownNPCSpriteOverlay> returnList = [];
        // TODO: Add frame offsets where necessary
        foreach ((Rectangle secondFrame, string resultingOverlaySuffix) in frameNameDifferenceArray) {
            returnList.Add(
                GenerateOverlayFromDifferenceBetweenFrames(
                    rawTextureData,
                    npcTexture.Width,
                    npcTexture.Height,
                    defaultFrameRectangle,
                    secondFrame,
                    $"{textureNamePrefix}_{resultingOverlaySuffix}"
                )
            );
        }

        return returnList.ToArray();
    }

    private static void LoadPersonalities() {
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

    public override void Unload() {
        if (spriteOverlayProfiles is not null) {
            Main.QueueMainThreadAction(
                () => {
                    foreach (TownNPCOverlayProfile spriteProfile in spriteOverlayProfiles.Values) {
                        spriteProfile.Dispose();
                    }
                }
            );
        }
    }

    public override void Load() {
        // TODO: Combine JSON into one file(?)
        sleepSchedules = new Dictionary<int, SleepSchedule>();
        JsonObject sleepSchedulesJSON = LWMUtils.GetJSONFromFile("Assets/JSONData/TownNPCSleepSchedules.json").Qo();
        foreach ((string npcName, JsonValue sleepSchedule) in sleepSchedulesJSON) {
            int npcType = NPCID.Search.GetId(npcName);

            sleepSchedules[npcType] = new SleepSchedule(TimeOnly.Parse(sleepSchedule["Start"]), TimeOnly.Parse(sleepSchedule["End"]));
        }

        JsonObject jsonAttackData = LWMUtils.GetJSONFromFile("Assets/JSONData/TownNPCAttackData.json").Qo();

        JsonObject projJSONAttackData = jsonAttackData["ProjNPCs"].Qo();
        JsonObject meleeJSONAttackData = jsonAttackData["MeleeNPCs"].Qo();

        Dictionary<int, TownNPCProjAttackData> projDict = [];
        foreach ((string npcName, JsonValue jsonValue) in projJSONAttackData) {
            JsonObject jsonObject = jsonValue.Qo();
            int npcType = NPCID.Search.GetId(npcName);

            projDict[npcType] = new TownNPCProjAttackData(
                jsonObject.Qi("projType"),
                jsonObject.Qi("projDamage"),
                (float)jsonObject.Qd("knockBack"),
                (float)jsonObject.Qd("speedMult"),
                jsonObject.Qi("attackDelay"),
                jsonObject.Qi("attackCooldown"),
                jsonObject.Qi("maxValue"),
                jsonObject.Qi("gravityCorrection"),
                NPCID.Sets.DangerDetectRange[npcType],
                (float)jsonObject.Qd("randomOffset")
            );
        }

        projectileAttackDatas = projDict;

        Dictionary<int, TownNPCMeleeAttackData> meleeDict = [];
        foreach ((string npcName, JsonValue jsonValue) in meleeJSONAttackData) {
            JsonObject jsonObject = jsonValue.Qo();
            int npcType = NPCID.Search.GetId(npcName);

            meleeDict[npcType] = new TownNPCMeleeAttackData(
                jsonObject.Qi("attackCooldown"),
                jsonObject.Qi("maxValue"),
                jsonObject.Qi("damage"),
                (float)jsonObject.Qd("knockBack"),
                jsonObject.Qi("itemWidth"),
                jsonObject.Qi("itemHeight")
            );
        }

        meleeAttackDatas = meleeDict;
    }

    public override void PostSetupContent() {
        if (Main.netMode != NetmodeID.Server) {
            Main.QueueMainThreadAction(GenerateTownNPCSpriteProfiles);
        }

        LoadPersonalities();
    }

    private void GenerateTownNPCSpriteProfiles() {
        Dictionary<int, TownNPCOverlayProfile> overlayTextures = [];
        TownGlobalNPC townSingletonNPC = ModContent.GetInstance<TownGlobalNPC>();
        NPC npc = new();
        for (int i = 0; i < NPCLoader.NPCCount; i++) {
            npc.SetDefaults(i);
            if (!townSingletonNPC.AppliesToEntity(npc, true)) {
                continue;
            }

            string modName = i >= NPCID.Count ? NPCLoader.GetNPC(i).Mod.Name : "Terraria";
            Asset<Texture2D> npcAsset;
            if (!TownNPCProfiles.Instance.GetProfile(npc, out ITownNPCProfile profile)) {
                npcAsset = TextureAssets.Npc[i];
                overlayTextures[i] = new TownNPCOverlayProfile(GenerateTownNPCSpriteOverlays(npcAsset.Name, npcAsset.ForceLoadAsset(modName), i));
                continue;
            }

            int npcVariationCount = profile is Profiles.StackedNPCProfile stackedNPCProfile ? stackedNPCProfile._profiles.Length : 1;
            List<TownNPCSpriteOverlay[]> spriteOverlays = [];
            for (int j = 0; j < npcVariationCount; npc.townNpcVariationIndex = ++j) {
                npcAsset = profile.GetTextureNPCShouldUse(npc);
                spriteOverlays.Add(GenerateTownNPCSpriteOverlays(npcAsset.Name, npcAsset.ForceLoadAsset(modName), i));
            }

            overlayTextures[i] = new TownNPCOverlayProfile(spriteOverlays.ToArray());
        }

        spriteOverlayProfiles = overlayTextures;
    }
}