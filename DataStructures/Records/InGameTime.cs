using System;
using LivingWorldMod.Utilities;

namespace LivingWorldMod.DataStructures.Records;

/// <summary>
///     Value type that represents the state of the in-game day/night cycle, measured in hours (real life minute), minutes (real life second), and ticks.
/// </summary>
public readonly record struct InGameTime(int Hour, int Minute, int Tick) : IComparable<InGameTime> {
    public static InGameTime CurrentTime  {
        get {
            // Adapted vanilla code
            double currentTime = Main.time;
            if (!Main.dayTime) {
                currentTime += LWMUtils.InGameDaylight;
            }

            double preciseHour = currentTime / LWMUtils.InGameFullDay * 24f - 19.5f;
            // "Day time" starts at 04:30 and must be shifted accordingly
            if (preciseHour < 0) {
                preciseHour += 24f;
            }

            int hour = (int)preciseHour;
            return new InGameTime(hour, (int)((preciseHour - hour) * 60), (int)(Main.time % LWMUtils.InGameMinute));
        }
    }

    public static bool operator <(InGameTime left, InGameTime right) => left.CompareTo(right) < 0;

    public static bool operator >(InGameTime left, InGameTime right) => left.CompareTo(right) > 0;

    public static bool operator <=(InGameTime left, InGameTime right) => left.CompareTo(right) <= 0;

    public static bool operator >=(InGameTime left, InGameTime right) => left.CompareTo(right) >= 0;

    public override int GetHashCode() => HashCode.Combine(Hour, Minute, Tick);

    public int CompareTo(InGameTime other) {
        int hourComparison = Hour.CompareTo(other.Hour);
        if (hourComparison != 0) {
            return hourComparison;
        }

        int minuteComparison = Minute.CompareTo(other.Minute);
        return minuteComparison != 0 ? minuteComparison : Tick.CompareTo(other.Tick);
    }

    public bool Equals(InGameTime other) => Hour == other.Hour
        && Minute == other.Minute
        && Tick == other.Tick;
}