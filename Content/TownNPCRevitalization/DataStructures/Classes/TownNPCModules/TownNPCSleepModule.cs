using System;
using System.Collections.Generic;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.DataStructures.Structs;
using LivingWorldMod.Utilities;
using Terraria.GameContent.Events;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;

public sealed  class TownNPCSleepModule (NPC npc, TownGlobalNPC globalNPC) : TownNPCModule(npc, globalNPC) {
    public static readonly SleepProfile DefaultSleepProfile =  new (new TimeOnly(19, 30, 0), new TimeOnly(4, 30, 0));

    private static Dictionary<int, SleepProfile> _sleepProfiles;

    /// <summary>
    ///     Current amount of sleep "points" that the NPC has stored. Gives various mood boosts/losses based on how many points the NPC has at any given point in time.
    /// </summary>
    // TODO: Save/Load this data
    public BoundedNumber<float> sleepValue = new (LWMUtils.InGameMoonlight, 0, LWMUtils.InGameMoonlight);

    public bool ShouldSleep {
        get {
            bool eventOccuringThatBlocksSleep = LanternNight.LanternsUp || Main.bloodMoon || Main.snowMoon || Main.pumpkinMoon;
            SleepProfile npcSleepProfile = GetSleepProfileOrDefault(npc.type);

            TimeOnly currentTime = LWMUtils.CurrentInGameTime;
            bool curTimeGreaterThanStartTime = currentTime >= npcSleepProfile.StartTime;
            bool curTimeLessThanEndTime = currentTime <= npcSleepProfile.EndTime;

            return !eventOccuringThatBlocksSleep
                && (npcSleepProfile.EndTime < npcSleepProfile.StartTime ? curTimeGreaterThanStartTime || curTimeLessThanEndTime : curTimeGreaterThanStartTime && curTimeLessThanEndTime);
        }
    }

    public static SleepProfile GetSleepProfileOrDefault(int npcType) => _sleepProfiles.GetValueOrDefault(npcType, DefaultSleepProfile);

    public static void Load() {
        // TODO: Load specific sleep profiles
        _sleepProfiles = [];
    }

    public override void Update() {
        if (!ShouldSleep) {
            sleepValue -= 1f;
        }
    }
}