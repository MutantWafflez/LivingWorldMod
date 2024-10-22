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
    private const int MaxAwakeValue = LWMUtils.InGameHour * 24;
    private const float DefaultAwakeValue = MaxAwakeValue * 0.2f;

    private static readonly SleepSchedule DefaultSleepSchedule = new(new TimeOnly(19, 30, 0), new TimeOnly(4, 30, 0));

    /// <summary>
    ///     The amount of ticks that this NPC has been awake. The higher the value, the more severe effects on mood will occur. This value is decreased rapidly by a Town NPC sleeping.
    /// </summary>
    public BoundedNumber<float> awakeTicks = new(DefaultAwakeValue, 0, MaxAwakeValue);

    public bool ShouldSleep {
        get {
            bool eventOccuringThatBlocksSleep = LanternNight.LanternsUp || Main.slimeRain || Main.invasionType > InvasionID.None || Main.bloodMoon || Main.snowMoon || Main.pumpkinMoon;
            SleepSchedule npcSleepSchedule = GetSleepProfileOrDefault(npc.type);

            return !eventOccuringThatBlocksSleep && LWMUtils.CurrentInGameTime.IsBetween(npcSleepSchedule.StartTime, npcSleepSchedule.EndTime);
        }
    }

    private bool IsAsleep => npc.ai[0] == TownNPCAIState.GetStateInteger<BeAtHomeAIState>() && npc.ai[1] == 1f;

    public TownNPCSleepModule(NPC npc, TownGlobalNPC globalNPC) : base(npc, globalNPC) {
        globalNPC.OnSave += tag => tag[nameof(awakeTicks)] = awakeTicks.Value;
        globalNPC.OnLoad += tag => awakeTicks = new BoundedNumber<float>(
            tag.TryGet(nameof(awakeTicks), out float savedSleepValue) ? savedSleepValue : DefaultAwakeValue,
            0,
            MaxAwakeValue
        );
    }

    public static SleepSchedule GetSleepProfileOrDefault(int npcType) => TownNPCDataSystem.sleepSchedules.GetValueOrDefault(npcType, DefaultSleepSchedule);

    public override void Update() {
        if (!IsAsleep) {
            awakeTicks += 1f;
        }

        if (awakeTicks >= MaxAwakeValue) {
            // TODO: Add passed out state
        }
    }
}