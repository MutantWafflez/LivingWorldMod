using System;

namespace LivingWorldMod.Utilities;

public partial class LWMUtils {
    /// <summary>
    ///     Calculates and returns the current time in game.
    /// </summary>
    public static TimeOnly CurrentInGameTime {
        get {
            // Adapted vanilla code
            double currentTime = Main.time;
            if (!Main.dayTime) {
                currentTime += InGameDaylight;
            }

            double preciseHour = currentTime / InGameFullDay * 24f - 19.5f;
            // "Day time" starts at 04:30 and must be shifted accordingly
            if (preciseHour < 0) {
                preciseHour += 24f;
            }

            int hour = (int)preciseHour;
            return new TimeOnly(hour, (int)((preciseHour - hour) * 60), (int)(Main.time % InGameMinute));
        }
    }
}