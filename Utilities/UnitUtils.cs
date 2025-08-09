namespace LivingWorldMod.Utilities;

// Utilities that help with unit conversions.
public static partial class LWMUtils {
    /// <summary>
    ///     Ticks in a real world second.
    /// </summary>
    public const int RealLifeSecond = 60;

    /// <summary>
    ///     Ticks in a real world minute.
    /// </summary>
    public const int RealLifeMinute = RealLifeSecond * 60;

    /// <summary>
    ///     Ticks in a real world hour.
    /// </summary>
    public const int RealLifeHour = RealLifeMinute * 60;

    /// <summary>
    ///     Ticks in a real world day.
    /// </summary>
    public const int RealLifeDay = RealLifeHour * 24;

    /// <summary>
    ///     Ticks in the duration of an in-game day when the sun is up. Equivalent to 15 in-game hours.
    /// </summary>
    public const int InGameDaylight = (int)Main.dayLength;

    /// <summary>
    ///     Ticks in the duration of an in-game day when the moon is out. Equivalent to 9 in-game hours.
    /// </summary>
    public const int InGameMoonlight = (int)Main.nightLength;

    /// <summary>
    ///     Ticks in the full duration of an in-game day.
    /// </summary>
    public const int InGameFullDay = InGameDaylight + InGameMoonlight;

    /// <summary>
    ///     Ticks in an in-game minute. Equivalent to a real life second.
    /// </summary>
    public const int InGameMinute = RealLifeSecond;

    /// <summary>
    ///     Ticks in an in-game hour. Equivalent to a real life minute.
    /// </summary>
    public const int InGameHour = RealLifeMinute;

    /// <summary>
    ///     The side length of a tile, in pixels.
    /// </summary>
    public const int TilePixelsSideLength = 16;
}