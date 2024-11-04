using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LivingWorldMod.Content.TownNPCRevitalization.AIStates;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using LivingWorldMod.Content.TownNPCRevitalization.UI.Bestiary;
using LivingWorldMod.DataStructures.Classes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;

/// <summary>
///     Global NPC that handles exclusively the AI overhaul for Town NPCs.
/// </summary>
public class TownGlobalNPC : GlobalNPC {
    /// <summary>
    ///     How many activities are remembered by this NPC for the
    ///     purpose of preventing activity repetition.
    /// </summary>
    private const int LastActivityMemoryLimit = 5;

    private static IReadOnlyDictionary<int, TownNPCAIState> _stateDict;
    private static IReadOnlyList<TownNPCActivity> _allActivities;

    private ForgetfulArray<TownNPCActivity> _lastActivities;

    public override bool InstancePerEntity => true;

    /// <summary>
    ///     Instance of the module that handles all path-finding
    ///     for this specific Town NPC.
    /// </summary>
    public TownNPCPathfinderModule PathfinderModule {
        get;
        private set;
    }

    /// <summary>
    ///     Instance of the module that handles the mood for this
    ///     specific Town NPC.
    /// </summary>
    public TownNPCMoodModule MoodModule {
        get;
        private set;
    }

    /// <summary>
    ///     Instance of the module that handles attacks & general
    ///     combat for this specific Town NPC.
    /// </summary>
    public TownNPCCombatModule CombatModule {
        get;
        private set;
    }

    /// <summary>
    ///     Instance of the module that handles NPCs being chatted to
    ///     by players or talking to other NPCs.
    /// </summary>
    public TownNPCChatModule ChatModule {
        get;
        private set;
    }

    /// <summary>
    ///     Instance of the module that handles all sprite-related tasks.
    /// </summary>
    public TownNPCSpriteModule SpriteModule {
        get;
        private set;
    }

    /// <summary>
    ///     Instance of the module that handles housing tasks and bounding.
    /// </summary>
    public TownNPCHousingModule HousingModule {
        get;
        private set;
    }

    /// <summary>
    ///     Instance of the module that handles special collision logic.
    /// </summary>
    public TownNPCCollisionModule CollisionModule {
        get;
        private set;
    }

    /// <summary>
    ///     Instance of the module that handles the Town NPC sleeping.
    /// </summary>
    public TownNPCSleepModule SleepModule {
        get;
        private set;
    }

    public static void RefreshToState<T>(NPC npc) where T : TownNPCAIState => RefreshToState(npc, TownNPCAIState.GetStateInteger<T>());

    public static bool IsValidStandingPosition(NPC npc, Point tilePos) {
        bool foundTileToStandOn = false;
        int npcTileWidth = (int)Math.Ceiling(npc.width / 16f);
        for (int i = 0; i < npcTileWidth; i++) {
            Tile floorTile = Main.tile[tilePos + new Point(i, 1)];
            if (!floorTile.HasUnactuatedTile || floorTile.IsHalfBlock || (!Main.tileSolidTop[floorTile.TileType] && !Main.tileSolid[floorTile.TileType])) {
                continue;
            }

            foundTileToStandOn = true;
            break;
        }

        if (!foundTileToStandOn) {
            return false;
        }

        int npcTileHeight = (int)Math.Ceiling(npc.height / 16f);
        for (int i = 0; i < npcTileHeight; i++) {
            for (int j = 0; j < npcTileWidth; j++) {
                Tile upTile = Main.tile[tilePos.X + j, tilePos.Y - i];

                if (upTile.HasUnactuatedTile && Main.tileSolid[upTile.TileType]) {
                    return false;
                }
            }
        }

        return true;
    }

    public static void RefreshToState(NPC npc, int stateValue) {
        npc.ai[0] = stateValue;
        npc.ai[1] = npc.ai[2] = npc.ai[3] = 0;
        npc.netUpdate = true;
    }

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => lateInstantiation
        && entity.aiStyle == NPCAIStyleID.Passive
        && entity.townNPC
        && !NPCID.Sets.IsTownPet[entity.type]
        && !NPCID.Sets.IsTownSlime[entity.type]
        && entity.type != NPCID.OldMan
        && entity.type != NPCID.TravellingMerchant;

    public override void Unload() {
        if (TownNPCDataSystem.spriteOverlayProfiles is not null) {
            Main.QueueMainThreadAction(
                () => {
                    foreach (TownNPCSpriteProfile spriteProfile in TownNPCDataSystem.spriteOverlayProfiles.Values) {
                        spriteProfile.Dispose();
                    }
                }
            );
        }
    }

    public override void SetStaticDefaults() {
        List<TownNPCAIState> states = ModContent.GetContent<TownNPCAIState>().ToList();

        if (states.Count != states.DistinctBy(state => state.ReservedStateInteger).Count()) {
            throw new Exception("Multiple TownNPCAIState instances with the same ReservedStateInteger");
        }

        _stateDict = states.ToDictionary(state => state.ReservedStateInteger);
        //_allActivities = states.OfType<TownNPCActivity>().ToList();
    }

    public override GlobalNPC NewInstance(NPC target) {
        TownGlobalNPC instance = (TownGlobalNPC)base.NewInstance(target)!;

        instance.PathfinderModule = new TownNPCPathfinderModule(target, instance);
        instance.MoodModule = new TownNPCMoodModule(target, instance);
        instance.CombatModule = new TownNPCCombatModule(target, instance);
        instance.ChatModule = new TownNPCChatModule(target, instance);
        instance.SpriteModule = new TownNPCSpriteModule(target, instance);
        instance.HousingModule = new TownNPCHousingModule(target, instance);
        instance.CollisionModule = new TownNPCCollisionModule(target, instance);
        instance.SleepModule = new TownNPCSleepModule(target, instance);
        instance._lastActivities = new ForgetfulArray<TownNPCActivity>(LastActivityMemoryLimit);

        return instance;
    }

    public override void SaveData(NPC npc, TagCompound tag) {
        OnSave?.Invoke(tag);
    }

    public override void LoadData(NPC npc, TagCompound tag) {
        OnLoad?.Invoke(tag);
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

        SleepModule.Update();
        MoodModule.Update();
        PathfinderModule.Update();
    }

    public override void PostAI(NPC npc) {
        // To make vanilla still draw extras properly
        npc.aiStyle = NPCAIStyleID.Passive;
    }

    public override void SetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.Add(new TownNPCPreferredSleepTimeSpanElement(npc.type));
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter) {
        PathfinderModule.SendNetworkData(bitWriter, binaryWriter);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader) {
        PathfinderModule.ReceiveNetworkData(bitReader, binaryReader);
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        SpriteModule.DrawNPC(spriteBatch, screenPos, drawColor);
        return false;
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        // TODO: Re-write chat bubble drawing
        ChatModule.DoChatDrawing(spriteBatch, screenPos, drawColor);

        if (!LWM.IsDebug) {
            return;
        }

        PathfinderModule.DebugDrawPath(spriteBatch, screenPos);
        HousingModule.DebugDraw(spriteBatch);
    }

    public override void FindFrame(NPC npc, int frameHeight) {
        SpriteModule.FrameNPC(frameHeight);
    }

    private void SetMiscNPCFields(NPC npc) {
        npc.dontTakeDamage = false;
        npc.rotation = 0f;
        NPC.ShimmeredTownNPCs[npc.type] = npc.IsShimmerVariant;
        if (npc.HasBuff(BuffID.Shimmer)) {
            PathfinderModule.CancelPathfind();
        }

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
            if (projectile.active && projectile.type == ProjectileID.MechanicWrench && projectile.ai[1] == npc.whoAmI) {
                wrenchFound = true;
            }
        }

        npc.localAI[0] = wrenchFound.ToInt();
    }

    /// <summary>
    ///     Event that is triggered in the <see cref="GlobalNPC.SaveData" /> method for saving data to the NPC.
    /// </summary>
    public event Action<TagCompound> OnSave;

    /// <summary>
    ///     Event that is triggered in the <see cref="GlobalNPC.LoadData" /> method for loading data from file for the NPC.
    /// </summary>
    public event Action<TagCompound> OnLoad;
}