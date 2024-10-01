using System;
using System.Collections.Generic;
using Hjson;
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
            return !eventOccuringThatBlocksSleep && currentTime.IsBetween(npcSleepSchedule.StartTime, npcSleepSchedule.EndTime);
        }
    }

    public static SleepSchedule GetSleepProfileOrDefault(int npcType) => _sleepSchedules.GetValueOrDefault(npcType, DefaultSleepSchedule);

    public static void Load() {
        _sleepSchedules = new Dictionary<int, SleepSchedule>();
        JsonObject sleepSchedulesJSON = LWMUtils.GetJSONFromFile("Assets/JSONData/TownNPCSleepSchedules.json").Qo();
        foreach ((string npcName, JsonValue sleepSchedule) in sleepSchedulesJSON) {
            int npcType = NPCID.Search.GetId(npcName);

            _sleepSchedules[npcType] = new SleepSchedule(TimeOnly.Parse(sleepSchedule["Start"]), TimeOnly.Parse(sleepSchedule["End"]));
        }
    }

    public override void Update() {
        if (!ShouldSleep) {
            sleepValue -= 1f;
        }
    }
}