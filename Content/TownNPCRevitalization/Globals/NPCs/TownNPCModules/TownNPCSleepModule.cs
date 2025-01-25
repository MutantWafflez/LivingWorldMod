using System;
using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.TownNPCRevitalization.AIStates;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using LivingWorldMod.Content.TownNPCRevitalization.UI.Bestiary;
using LivingWorldMod.DataStructures.Records;
using LivingWorldMod.DataStructures.Structs;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Events;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs.TownNPCModules;

public sealed  class TownNPCSleepModule  : TownNPCModule {
    private const int MaxAwakeValue = LWMUtils.InGameHour * 24;
    private const float DefaultAwakeValue = MaxAwakeValue * 0.2f;

    private static readonly SleepSchedule DefaultSleepSchedule = new(new TimeOnly(19, 30, 0), new TimeOnly(4, 30, 0));
    private static readonly Gradient<Color> SleepIconColorGradient = new (Color.Lerp, (0f, Color.Red), (0.5f, Color.DarkOrange), (1f, Color.White));

    /// <summary>
    ///     The amount of ticks that this NPC has been awake. The higher the value, the more severe effects on mood will occur. This value is decreased rapidly by a Town NPC sleeping.
    /// </summary>
    public BoundedNumber<float> awakeTicks = new(DefaultAwakeValue, 0, MaxAwakeValue);

    public bool IsAsleep => NPC.ai[0] == TownNPCAIState.GetStateInteger<PassedOutAIState>()
        || (NPC.ai[0] == TownNPCAIState.GetStateInteger<BeAtHomeAIState>() && NPC.ai[1] == BeAtHomeAIState.IsSleepingStateFlag);

    public override int UpdatePriority => 1;

    public TownNPCDrawRequest GetSleepSpriteDrawData {
        get {
            Main.instance.LoadItem(ItemID.SleepingIcon);
            Texture2D sleepingIconTexture = TextureAssets.Item[ItemID.SleepingIcon].Value;
            return new TownNPCDrawRequest(
                sleepingIconTexture,
                new Vector2(sleepingIconTexture.Width / -2f, -32f + MathF.Sin(Main.GlobalTimeWrappedHourly)),
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

    public bool ShouldSleep {
        get {
            bool sleepBeingBlocked = LanternNight.LanternsUp
                // TODO: Allow sleeping once tired enough, even if party is occurring
                || GenuinePartyIsOccurring
                || NPC.GetGlobalNPC<TownNPCChatModule>().IsChattingWithPlayerDirectly;
            SleepSchedule npcSleepSchedule = GetSleepProfileOrDefault(NPC.type);

            return !sleepBeingBlocked && LWMUtils.CurrentInGameTime.IsBetween(npcSleepSchedule.StartTime, npcSleepSchedule.EndTime);
        }
    }

    private static bool GenuinePartyIsOccurring => BirthdayParty.PartyIsUp && BirthdayParty.GenuineParty;

    public static SleepSchedule GetSleepProfileOrDefault(int npcType) => TownNPCDataSystem.sleepSchedules.GetValueOrDefault(npcType, DefaultSleepSchedule);

    public override void SetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.Add(new TownNPCPreferredSleepTimeSpanElement(npc.type));
    }

    public override void SaveData(NPC npc, TagCompound tag) {
        tag[nameof(awakeTicks)] = awakeTicks.Value;
    }

    public override void LoadData(NPC npc, TagCompound tag) {
        awakeTicks = new BoundedNumber<float>(
            tag.TryGet(nameof(awakeTicks), out float savedSleepValue) ? savedSleepValue : DefaultAwakeValue,
            0,
            MaxAwakeValue
        );
    }

    public override bool? CanChat(NPC npc) => npc.ai[0] == TownNPCAIState.GetStateInteger<PassedOutAIState>() ? false : null;

    public override void UpdateModule() {
        if (!IsAsleep) {
            awakeTicks += 1f;
        }

        if (awakeTicks < MaxAwakeValue) {
            return;
        }

        NPC.GetGlobalNPC<TownNPCPathfinderModule>().CancelPathfind();
        TownNPCStateModule.RefreshToState<PassedOutAIState>(NPC);
    }
}