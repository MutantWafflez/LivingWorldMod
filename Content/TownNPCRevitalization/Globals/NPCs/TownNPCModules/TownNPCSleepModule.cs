using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LivingWorldMod.Content.TownNPCRevitalization.AIStates;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Hooks;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using LivingWorldMod.Content.TownNPCRevitalization.UI.Bestiary;
using LivingWorldMod.DataStructures.Records;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs.TownNPCModules;

public sealed class TownNPCSleepModule : TownNPCModule, IOnTownNPCAttack {
    private const int MaxAwakeValue = LWMUtils.InGameHour * 24;
    private const float DefaultAwakeValue = MaxAwakeValue * 0.2f;

    private const int MaxBlockedSleepValue = LWMUtils.RealLifeSecond * 10;

    private static readonly SleepThresholds DefaultSleepThresholds = new (LWMUtils.InGameHour * 17, LWMUtils.InGameHour * 13, LWMUtils.InGameHour * 5);
    private static readonly SleepSchedule DefaultSleepSchedule = new(new TimeOnly(19, 30, 0), new TimeOnly(4, 30, 0));

    private static readonly Gradient<Color> SleepIconColorGradient = new (Color.Lerp, (0f, Color.Red), (0.5f, Color.DarkOrange), (1f, Color.White));

    /// <summary>
    ///     The amount of ticks that this NPC has been awake. The higher the value, the more severe effects on mood will occur. This value is decreased rapidly by a Town NPC sleeping.
    /// </summary>
    public BoundedNumber<float> awakeTicks = new(DefaultAwakeValue, 0, MaxAwakeValue);

    /// <summary>
    ///     The amount of ticks that must pass before this NPC is allowed to sleep, even if they really want to.
    /// </summary>
    private BoundedNumber<int> _blockedSleepTimer = new(0, 0, MaxBlockedSleepValue);

    public bool IsAsleep => NPC.ai[0] == TownNPCAIState.GetStateInteger<PassedOutAIState>()
        || (NPC.ai[0] == TownNPCAIState.GetStateInteger<BeAtHomeAIState>() && NPC.ai[1] == BeAtHomeAIState.IsSleepingStateFlag);

    public override int UpdatePriority => 1;

    public TownNPCDrawRequest SleepSpriteDrawData {
        get {
            Main.instance.LoadItem(ItemID.SleepingIcon);
            Texture2D sleepingIconTexture = TextureAssets.Item[ItemID.SleepingIcon].Value;
            return new TownNPCDrawRequest(
                sleepingIconTexture,
                new Vector2(0, -10f + MathF.Sin(Main.GlobalTimeWrappedHourly)),
                Color: SleepIconColorGradient.GetValue(SleepQualityModifier) * 0.67f,
                Rotation: 0f,
                Origin: Vector2.Zero,
                SpriteEffect: SpriteEffects.None
            );
        }
    }

    /// <summary>
    ///     The current sleep "quality" modifier, which determines how "well" an NPC should be sleeping. Lower values represent worse quality of sleep.
    /// </summary>
    public float SleepQualityModifier {
        get {
            bool[] currentEvents = [Main.eclipse, Main.slimeRain, Main.invasionType > InvasionID.None, Main.bloodMoon, Main.snowMoon, Main.pumpkinMoon];
            return currentEvents.Where(eventIsOccuring => eventIsOccuring).Aggregate(1f, (current, _) => current * 0.8f);
        }
    }

    /// <summary>
    ///     Denotes whether there is anything event or tertiary circumstances that is preventing this NPC from sleeping. If this value is false, it means this NPC cannot sleep normally. They can still
    ///     pass out, however.
    /// </summary>
    public bool CanSleep => _blockedSleepTimer <= 0
        && !NPC.GetGlobalNPC<TownNPCChatModule>().IsChattingWithPlayerDirectly
        && NPC.ai[0] != TownNPCAIState.GetStateInteger<PassedOutAIState>()
        && !NPC.GetGlobalNPC<TownNPCCombatModule>().IsAttacking;

    /// <summary>
    ///     In contrast to <see cref="CanSleep" />, this flag denotes whether this NPC has had its random checks succeed and "wants" to sleep. See the <see cref="CheckNPCUrgeToSleep" /> code for
    ///     more details on these checks. Note that regardless of this flag, the NPC will <b>NOT</b> sleep if <see cref="CanSleep" /> is <see langword="false" />.
    /// </summary>
    public bool WantsToSleep {
        get;
        private set;
    }

    public static SleepSchedule GetSleepProfileOrDefault(int npcType) => TownNPCDataSystem.sleepSchedules.GetValueOrDefault(npcType, DefaultSleepSchedule);

    public static SleepThresholds GetSleepThresholdsOrDefault(int npcType) => TownNPCDataSystem.sleepThresholds.GetValueOrDefault(npcType, DefaultSleepThresholds);

    public override void SetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.Add(new TownNPCPreferredSleepTimeSpanElement(npc.type));
    }

    public override void SaveData(NPC npc, TagCompound tag) {
        tag[nameof(awakeTicks)] = awakeTicks.Value;
        tag[nameof(WantsToSleep)] = WantsToSleep;
    }

    public override void LoadData(NPC npc, TagCompound tag) {
        awakeTicks = new BoundedNumber<float>(
            tag.TryGet(nameof(awakeTicks), out float savedSleepValue) ? savedSleepValue : DefaultAwakeValue,
            0,
            MaxAwakeValue
        );

        WantsToSleep = tag.TryGet(nameof(WantsToSleep), out bool wantsToSleep) && wantsToSleep;
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter) {
        bitWriter.WriteBit(WantsToSleep);
        binaryWriter.Write(awakeTicks);
        binaryWriter.Write(_blockedSleepTimer);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader) {
        WantsToSleep = bitReader.ReadBit();
        awakeTicks = new BoundedNumber<float>(binaryReader.ReadSingle(), awakeTicks.LowerBound, awakeTicks.UpperBound);
        _blockedSleepTimer = new BoundedNumber<int>(binaryReader.ReadInt32(), _blockedSleepTimer.LowerBound, _blockedSleepTimer.UpperBound);
    }

    public override bool? CanChat(NPC npc) => npc.ai[0] == TownNPCAIState.GetStateInteger<PassedOutAIState>() ? false : null;

    public override void HitEffect(NPC npc, NPC.HitInfo hit) {
        _blockedSleepTimer += LWMUtils.RealLifeSecond * 2;
    }

    public override void UpdateModule() {
        _blockedSleepTimer -= 1;
        if (!IsAsleep && !NightPartySystem.IsNightPartyOccuring) {
            awakeTicks += 1f;
        }

        CheckNPCUrgeToSleep();

        if (awakeTicks < MaxAwakeValue) {
            return;
        }

        // TODO: Add passed-out mood modifier
        NPC.GetGlobalNPC<TownNPCPathfinderModule>().CancelPathfind();
        TownNPCStateModule.RefreshToState<PassedOutAIState>(NPC);
    }

    public void OnTownNPCAttack(NPC npc) {
        _blockedSleepTimer += LWMUtils.RealLifeSecond * 8;
    }

    private void CheckNPCUrgeToSleep() {
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            return;
        }

        SleepThresholds thresholds = GetSleepThresholdsOrDefault(NPC.type);
        int bestRestedLimit = MaxAwakeValue - thresholds.BestRestLimit;

        SleepSchedule npcSleepSchedule = GetSleepProfileOrDefault(NPC.type);
        bool isBedTime = LWMUtils.CurrentInGameTime.IsBetween(npcSleepSchedule.StartTime, npcSleepSchedule.EndTime);
        switch (WantsToSleep) {
            case true when (!isBedTime || awakeTicks <= bestRestedLimit / 3f) && Main.rand.NextBool(Math.Max(1, (int)awakeTicks)):
                WantsToSleep = false;
                NPC.netUpdate = true;
                break;
            case false when (isBedTime && awakeTicks >= thresholds.WellRestedLimit && !NightPartySystem.IsNightPartyOccuring) || awakeTicks >= thresholds.TiredLimit: {
                // The denominator of the chance for this NPC to fall asleep each tick (i.e. 1/x chance)
                int chanceToSleep = (MaxAwakeValue - (int)awakeTicks) / 10;

                // If it's the NPC's bedtime, the NPC will more heavily prefer trying to sleep
                if (isBedTime) {
                    chanceToSleep /= 4;
                }

                if (Main.rand.NextBool(Utils.Clamp(chanceToSleep, 1, MaxAwakeValue))) {
                    WantsToSleep = NPC.netUpdate = true;
                }

                break;
            }
        }
    }
}