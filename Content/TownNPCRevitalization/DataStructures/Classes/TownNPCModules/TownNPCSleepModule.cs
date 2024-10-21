using System;
using System.Collections.Generic;
using LivingWorldMod.Content.TownNPCRevitalization.AIStates;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using LivingWorldMod.DataStructures.Structs;
using LivingWorldMod.Utilities;
using Terraria.GameContent.Events;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;

public sealed  class TownNPCSleepModule  : TownNPCModule {
    private const int MaxSleepValue = LWMUtils.InGameHour * 12;
    private const float DefaultSleepValue = MaxSleepValue * 0.8f;

    private static readonly SleepSchedule DefaultSleepSchedule = new(new TimeOnly(19, 30, 0), new TimeOnly(4, 30, 0));

    /// <summary>
    ///     Current amount of sleep "points" that the NPC has stored. Gives various mood boosts/losses based on how many points the NPC has at any given point in time.
    /// </summary>
    public BoundedNumber<float> sleepValue = new(DefaultSleepValue, 0, MaxSleepValue);

    public bool ShouldSleep {
        get {
            bool eventOccuringThatBlocksSleep = LanternNight.LanternsUp || Main.slimeRain || Main.invasionType > InvasionID.None || Main.bloodMoon || Main.snowMoon || Main.pumpkinMoon;
            SleepSchedule npcSleepSchedule = GetSleepProfileOrDefault(npc.type);

            return !eventOccuringThatBlocksSleep && LWMUtils.CurrentInGameTime.IsBetween(npcSleepSchedule.StartTime, npcSleepSchedule.EndTime);
        }
    }

    private bool IsAsleep => npc.ai[0] == TownNPCAIState.GetStateInteger<BeAtHomeAIState>() && npc.ai[1] == 1f;

    public TownNPCSleepModule(NPC npc, TownGlobalNPC globalNPC) : base(npc, globalNPC) {
        globalNPC.OnSave += tag => tag[nameof(sleepValue)] = sleepValue.Value;
        globalNPC.OnLoad += tag => sleepValue = new BoundedNumber<float>(
            tag.TryGet(nameof(sleepValue), out float savedSleepValue) ? savedSleepValue : DefaultSleepValue,
            0,
            MaxSleepValue
        );
    }

    public static SleepSchedule GetSleepProfileOrDefault(int npcType) => TownNPCDataSystem.sleepSchedules.GetValueOrDefault(npcType, DefaultSleepSchedule);

    public override void Update() {
        if (!IsAsleep) {
            sleepValue -= 0.5f;
        }
    }
}