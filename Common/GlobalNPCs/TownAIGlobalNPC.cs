using System;
using System.Collections.Generic;
using System.Linq;
using Hjson;
using LivingWorldMod.Common.ModTypes;
using LivingWorldMod.Content.TownNPCAIStates;
using LivingWorldMod.Custom.Classes;
using LivingWorldMod.Custom.Structs;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;

namespace LivingWorldMod.Common.GlobalNPCs;

/// <summary>
/// Global NPC that handles exclusively the AI overhaul for Town NPCs.
/// </summary>
public class TownAIGlobalNPC : GlobalNPC {
    /// <summary>
    /// How many activities are remembered by this NPC for the
    /// purpose of preventing activity repetition.
    /// </summary>
    private const int LastActivityMemoryLimit = 5;

    private static IReadOnlyDictionary<int, TownNPCAIState> _stateDict;
    private static IReadOnlyList<TownNPCActivity> _allActivities;

    private static IReadOnlyDictionary<int, (Texture2D, Texture2D)> _talkBlinkOverlays;

    public override bool InstancePerEntity => true;

    /// <summary>
    /// Instance of the module that handles all path-finding
    /// for this specific Town NPC.
    /// </summary>
    public TownNPCPathfinderModule PathfinderModule {
        get;
        private set;
    }

    /// <summary>
    /// Instance of the module that handles the mood for this
    /// specific Town NPC.
    /// </summary>
    public TownNPCMoodModule MoodModule {
        get;
        private set;
    }

    /// <summary>
    /// Instance of the module that handles attacks & general
    /// combat for this specific Town NPC.
    /// </summary>
    public TownNPCCombatModule CombatModule {
        get;
        private set;
    }

    /// <summary>
    /// Instance of the module that handles NPCs being chatted to
    /// by players or talking to other NPCs.
    /// </summary>
    public TownNPCChatModule ChatModule {
        get;
        private set;
    }

    /// <summary>
    /// Instance of the module that handles any miscellaneous drawing
    /// tasks.
    /// </summary>
    public TownNPCSpriteModule SpriteModule {
        get;
        private set;
    }

    /// <summary>
    /// Instance of the module that handles housing tasks and bounding.
    /// </summary>
    public TownNPCHousingModule HousingModule {
        get;
        private set;
    }

    private ForgetfulArray<TownNPCActivity> _lastActivities;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.aiStyle == NPCAIStyleID.Passive
                                                                                && entity.townNPC
                                                                                && !NPCID.Sets.IsTownPet[entity.type]
                                                                                && !NPCID.Sets.IsTownSlime[entity.type]
                                                                                && entity.type != NPCID.OldMan
                                                                                && entity.type != NPCID.TravellingMerchant;

    public override void Load() {
        JsonObject jsonAttackData = JsonUtils.GetJSONFromFile("Assets/JSONData/TownNPCAttackData.json").Qo();

        JsonObject projJSONAttackData = jsonAttackData["ProjNPCs"].Qo();
        JsonObject meleeJSONAttackData = jsonAttackData["MeleeNPCs"].Qo();

        Dictionary<int, TownNPCProjAttackData> projDict = new();
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
        TownNPCCombatModule.projAttackData = projDict;

        Dictionary<int, TownNPCMeleeAttackData> meleeDict = new();
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
        TownNPCCombatModule.meleeAttackData = meleeDict;

        JsonObject jsonMoodValues = JsonUtils.GetJSONFromFile("Assets/JSONData/TownNPCMoodValues.json").Qo();
        Dictionary<string, TownNPCMoodModule.MoodModifier> moodModifierDict = new();
        foreach ((string moodModifier, JsonValue jsonValue) in jsonMoodValues) {
            JsonObject jsonObject = jsonValue.Qo();
            moodModifierDict[moodModifier] = new TownNPCMoodModule.MoodModifier(jsonObject["MoodOffset"], jsonObject["MaxStacks"]);
        }
        TownNPCMoodModule.moodModifiers = moodModifierDict;
    }

    public override void Unload() {
        Main.QueueMainThreadAction(() => {
            foreach ((Texture2D talkTexture, Texture2D blinkTexture) in _talkBlinkOverlays.Values) {
                talkTexture.Dispose();
                blinkTexture.Dispose();
            }
        });
    }

    public override void SetStaticDefaults() {
        List<TownNPCAIState> states = ModContent.GetContent<TownNPCAIState>().ToList();

        if (states.Count != states.DistinctBy(state => state.ReservedStateInteger).Count()) {
            throw new Exception("Multiple TownNPCAIState instances with the same ReservedStateInteger");
        }

        _stateDict = states.ToDictionary(state => state.ReservedStateInteger);
        //_allActivities = states.OfType<TownNPCActivity>().ToList();

        Main.QueueMainThreadAction(GenerateTalkBlinkTextures);
    }

    public override GlobalNPC NewInstance(NPC target) {
        TownAIGlobalNPC instance = (TownAIGlobalNPC)base.NewInstance(target);

        instance.PathfinderModule = new TownNPCPathfinderModule(target);
        instance.MoodModule = new TownNPCMoodModule(target);
        instance.CombatModule = new TownNPCCombatModule(target);

        if (_talkBlinkOverlays is not null) {
            instance.ChatModule = new TownNPCChatModule(target, _talkBlinkOverlays[target.type].Item1);
            instance.SpriteModule = new TownNPCSpriteModule(target, _talkBlinkOverlays[target.type].Item2);
        }

        instance.HousingModule = new TownNPCHousingModule(target);
        instance._lastActivities = new ForgetfulArray<TownNPCActivity>(LastActivityMemoryLimit);

        return instance;
    }

    public override bool PreAI(NPC npc) {
        npc.aiStyle = -1;
        return true;
    }

    public override void AI(NPC npc) {
        SetMiscNPCFields(npc);

        SpriteModule.Update();
        ChatModule.Update();
        CombatModule.Update();
        HousingModule.Update();

        if (_stateDict.TryGetValue((int)npc.ai[0], out TownNPCAIState state)) {
            state.DoState(this, npc);

            /*
            // New activities can only be selected when the npc is in the default state
            if (state is DefaultAIState && Main.rand.Next(_allActivities.SkipWhile(_lastActivities.Contains).ToList()) is { } activity && activity.CanDoActivity(this, npc)) {
                _lastActivities.Add(activity);
                activity.InitializeActivity(npc);

                return;
            }*/
        }
        else {
            RefreshToState<DefaultAIState>(npc);
        }

        MoodModule.Update();
        PathfinderModule.Update();
    }

    public override void PostAI(NPC npc) {
        // To make vanilla still draw extras properly
        npc.aiStyle = NPCAIStyleID.Passive;
    }

    public override bool? CanFallThroughPlatforms(NPC npc) => PathfinderModule.canFallThroughPlatforms ? true : null;

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (_stateDict.TryGetValue((int)npc.ai[0], out TownNPCAIState state)) {
            state.PreDrawNPC(this, npc, spriteBatch, screenPos, drawColor);
        }

        return true;
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (_stateDict.TryGetValue((int)npc.ai[0], out TownNPCAIState state)) {
            state.PostDrawNPC(this, npc, spriteBatch, screenPos, drawColor);
        }
        SpriteModule.DrawOntoNPC(spriteBatch, screenPos, drawColor);
        ChatModule.DoChatDrawing(spriteBatch, screenPos, drawColor);

        if (!LWM.IsDebug) {
            return;
        }

        PathfinderModule.DebugDrawPath(spriteBatch, screenPos);
        HousingModule.DebugDraw(spriteBatch);
    }

    public override void FindFrame(NPC npc, int frameHeight) {
        if (_stateDict.TryGetValue((int)npc.ai[0], out TownNPCAIState state)) {
            state.FrameNPC(this, npc, frameHeight);
        }
    }

    public static void RefreshToState<T>(NPC npc)
        where T : TownNPCAIState => RefreshToState(npc, TownNPCAIState.GetStateInteger<T>());

    public static void RefreshToState(NPC npc, int stateValue) {
        npc.ai[0] = stateValue;
        npc.ai[1] = npc.ai[2] = 0f;
        npc.netUpdate = true;
    }

    private void SetMiscNPCFields(NPC npc) {
        npc.dontTakeDamage = false;
        npc.rotation = 0f;
        NPC.ShimmeredTownNPCs[npc.type] = npc.IsShimmerVariant;

        if (npc.type == NPCID.SantaClaus && Main.netMode != NetmodeID.MultiplayerClient && !Main.xMas) {
            npc.StrikeInstantKill();
            if (Main.netMode == NetmodeID.Server) {
                NetMessage.SendData(MessageID.DamageNPC, -1, -1, null, npc.whoAmI, 9999f);
            }
        }

        switch (npc.type) {
            case NPCID.Golfer:
                NPC.savedGolfer = true;
                break;
            case NPCID.TaxCollector:
                NPC.savedTaxCollector = true;
                break;
            case NPCID.GoblinTinkerer:
                NPC.savedGoblin = true;
                break;
            case NPCID.Wizard:
                NPC.savedWizard = true;
                break;
            case NPCID.Mechanic:
                NPC.savedMech = true;
                break;
            case NPCID.Stylist:
                NPC.savedStylist = true;
                break;
            case NPCID.Angler:
                NPC.savedAngler = true;
                break;
            case NPCID.DD2Bartender:
                NPC.savedBartender = true;
                break;
        }

        if (npc.type == NPCID.TaxCollector) {
            NPC.taxCollector = true;
        }

        npc.directionY = -1;
        if (npc.direction == 0) {
            npc.direction = 1;
        }

        if (npc.velocity.Y == 0f && !PathfinderModule.IsPathfinding) {
            npc.velocity *= 0.75f;
        }

        if (npc.type != NPCID.Mechanic) {
            return;
        }

        int wrenchWhoAmI = NPC.lazyNPCOwnedProjectileSearchArray[npc.whoAmI];
        bool wrenchFound = false;

        if (Main.projectile.IndexInRange(wrenchWhoAmI)) {
            Projectile projectile = Main.projectile[wrenchWhoAmI];
            if (projectile.active && projectile.type == 582 && projectile.ai[1] == npc.whoAmI) {
                wrenchFound = true;
            }
        }

        npc.localAI[0] = wrenchFound.ToInt();
    }

    private void GenerateTalkBlinkTextures() {
        Dictionary<int, (Texture2D, Texture2D)> talkBlinkOverlays = new();
        NPC npc = new();
        for (int i = 0; i < NPCLoader.NPCCount; i++) {
            npc.SetDefaults(i);
            if (!AppliesToEntity(npc, true) || !TownNPCProfiles.Instance.GetProfile(npc, out ITownNPCProfile profile)) {
                continue;
            }

            Asset<Texture2D> npcAsset = profile.GetTextureNPCShouldUse(npc);
            string modName = "Terraria";
            if (i >= NPCID.Count) {
                modName = NPCLoader.GetNPC(i).Mod.Name;
            }
            Texture2D npcTexture = ModContent.Request<Texture2D>($"{modName}/{npcAsset.Name}".Replace("\\", "/"), AssetRequestMode.ImmediateLoad).Value;

            int npcFrameHeight = npcTexture.Height / Main.npcFrameCount[i];
            int totalPixelArea = npcTexture.Width * npcFrameHeight;
            int nonAttackFrameCount = Main.npcFrameCount[i] - NPCID.Sets.AttackFrameCount[i];

            Color[] firstFrameData = new Color[totalPixelArea];
            Color[] secondFrameData = new Color[totalPixelArea];
            Rectangle frameRectangle = new(0, 0, npcTexture.Width, npcFrameHeight);

            npcTexture.GetData(0, frameRectangle, firstFrameData, 0, totalPixelArea);
            npcTexture.GetData(0, frameRectangle with { Y = npcFrameHeight * (nonAttackFrameCount - 2) }, secondFrameData, 0, totalPixelArea);

            Color[] colorDifference = new Color[totalPixelArea];
            for (int j = 0; j < totalPixelArea; j++) {
                if (firstFrameData[j] != secondFrameData[j]) {
                    colorDifference[j] = secondFrameData[j];
                }
            }

            Texture2D talkingFrameTexture = new(Main.graphics.GraphicsDevice, npcTexture.Width, npcFrameHeight);
            talkingFrameTexture.SetData(colorDifference);
            talkingFrameTexture.Name = $"{npcAsset.Name[(npcAsset.Name.LastIndexOf("\\") + 1)..]}_Talking";

            npcTexture.GetData(0, frameRectangle with { Y = npcFrameHeight * (nonAttackFrameCount - 1) }, secondFrameData, 0, totalPixelArea);
            for (int j = 0; j < totalPixelArea; j++) {
                colorDifference[j] = default(Color);
                if (firstFrameData[j] != secondFrameData[j]) {
                    colorDifference[j] = secondFrameData[j];
                }
            }

            Texture2D blinkingFrameTexture = new(Main.graphics.GraphicsDevice, npcTexture.Width, npcFrameHeight);
            blinkingFrameTexture.SetData(colorDifference);
            blinkingFrameTexture.Name = $"{npcAsset.Name[(npcAsset.Name.LastIndexOf("\\") + 1)..]}_Blinking";

            talkBlinkOverlays[i] = (talkingFrameTexture, blinkingFrameTexture);
        }

        _talkBlinkOverlays = talkBlinkOverlays;
    }
}