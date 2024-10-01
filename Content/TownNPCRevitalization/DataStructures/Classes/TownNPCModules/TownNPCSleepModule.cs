using System;
using System.Collections.Generic;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.DataStructures.Structs;
using LivingWorldMod.Utilities;
using Terraria.GameContent.Events;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;

public sealed  class TownNPCSleepModule (NPC npc, TownGlobalNPC globalNPC) : TownNPCModule(npc, globalNPC) {
    public static readonly SleepSchedule DefaultSleepSchedule =  new (new TimeOnly(19, 30, 0), new TimeOnly(4, 30, 0));

    private static Dictionary<int, SleepSchedule> _sleepSchedules;

    /// <summary>
    ///     Current amount of sleep "points" that the NPC has stored. Gives various mood boosts/losses based on how many points the NPC has at any given point in time.
    /// </summary>
    // TODO: Save/Load this data
    public BoundedNumber<float> sleepValue = new (LWMUtils.InGameMoonlight, 0, LWMUtils.InGameMoonlight);

    public bool ShouldSleep {
        get {
            bool eventOccuringThatBlocksSleep = LanternNight.LanternsUp || Main.slimeRain || Main.invasionType > InvasionID.None || Main.bloodMoon || Main.snowMoon || Main.pumpkinMoon;
            SleepSchedule npcSleepSchedule = GetSleepProfileOrDefault(npc.type);

            TimeOnly currentTime = LWMUtils.CurrentInGameTime;
            bool curTimeGreaterThanStartTime = currentTime >= npcSleepSchedule.StartTime;
            bool curTimeLessThanEndTime = currentTime <= npcSleepSchedule.EndTime;

            return !eventOccuringThatBlocksSleep
                && (npcSleepSchedule.EndTime < npcSleepSchedule.StartTime ? curTimeGreaterThanStartTime || curTimeLessThanEndTime : curTimeGreaterThanStartTime && curTimeLessThanEndTime);
        }
    }

    public static SleepSchedule GetSleepProfileOrDefault(int npcType) => _sleepSchedules.GetValueOrDefault(npcType, DefaultSleepSchedule);

    public static void Load() {
        // TODO: Load specific sleep profiles
        _sleepSchedules = new Dictionary<int, SleepSchedule> {
            { NPCID.ArmsDealer, new SleepSchedule(new TimeOnly(22, 45, 0), new TimeOnly(6, 45, 0)) }, { NPCID.Princess, new SleepSchedule(new TimeOnly(6, 30, 0), new TimeOnly(12, 30, 0)) }
        };
    }

    public override void Update() {
        if (!ShouldSleep) {
            sleepValue -= 1f;
        }
    }
}