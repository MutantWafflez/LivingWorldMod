using System.Collections.Generic;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.DataStructures.Records;
using LivingWorldMod.DataStructures.Structs;
using LivingWorldMod.Utilities;
using Terraria.GameContent.Events;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;

public sealed  class TownNPCSleepModule (NPC npc) : TownNPCModule(npc) {
    /// <summary>
    ///     Minimum ticks required for highest mood boost from sleeping. Equivalent to a 8 hours of in-game time.
    /// </summary>
    public const int BestSleepThreshold = LWMUtils.InGameHour * 8;

    /// <summary>
    ///     Minimum ticks required for the second highest mood boost from sleep. Equivalent to 6 hours of in-game time.
    /// </summary>
    public const int DecentSleepThreshold = LWMUtils.InGameHour * 6;

    /// <summary>
    ///     Minimum ticks required for the second lowest mood loss from sleep. Equivalent to 3 hours of in-game time.
    /// </summary>
    public const int BadSleepThreshold = LWMUtils.InGameHour * 3;

    public static readonly SleepProfile DefaultSleepProfile =  new (new InGameTime(19, 30, 0), new InGameTime(4, 30, 0));

    private static Dictionary<int, SleepProfile> _sleepProfiles;

    /// <summary>
    ///     Current amount of sleep "points" that the NPC has stored. Gives various mood boosts/losses based on how many points the NPC has at any given point in time.
    /// </summary>
    // TODO: Save/Load this data
    public BoundedNumber<float> sleepValue = new (LWMUtils.InGameMoonlight, 0, LWMUtils.InGameMoonlight);

    public bool ShouldSleep {
        get {
            bool eventOccuringThatBlocksSleep = LanternNight.LanternsUp || Main.bloodMoon || Main.snowMoon || Main.pumpkinMoon;
            SleepProfile npcSleepProfile = _sleepProfiles.GetValueOrDefault(npc.type, DefaultSleepProfile);

            InGameTime currentTime = InGameTime.CurrentTime;
            bool curTimeGreaterThanStartTime = currentTime >= npcSleepProfile.StartTime;
            bool curTimeLessThanEndTime = currentTime <= npcSleepProfile.EndTime;

            return !eventOccuringThatBlocksSleep
                && (npcSleepProfile.EndTime < npcSleepProfile.StartTime ? curTimeGreaterThanStartTime || curTimeLessThanEndTime : curTimeGreaterThanStartTime && curTimeLessThanEndTime);
        }
    }

    public static void Load() {
        // TODO: Load specific sleep profiles
        _sleepProfiles = [];
    }

    public override void Update() {
        if (!ShouldSleep) {
            sleepValue -= 1f;
        }

        string sleepQualityKey = "SleepDeprived";
        int moodOffset = -20;
        if (sleepValue >= BestSleepThreshold) {
            sleepQualityKey = "VeryWellRested";
            moodOffset = 12;
        }
        else if (sleepValue >= DecentSleepThreshold) {
            sleepQualityKey = "WellRested";
            moodOffset = 8;
        }
        else if (sleepValue >= BadSleepThreshold) {
            sleepQualityKey = "Tired";
            moodOffset = -8;
        }

        GlobalNPC.MoodModule.AddModifier(
            new SubstitutableLocalizedText($"TownNPCMoodDescription.{sleepQualityKey}".Localized()),
            new SubstitutableLocalizedText($"TownNPCMoodFlavorText.{LWMUtils.GetNPCTypeNameOrIDName(npc.type)}.{sleepQualityKey}".Localized()),
            moodOffset
        );
    }
}