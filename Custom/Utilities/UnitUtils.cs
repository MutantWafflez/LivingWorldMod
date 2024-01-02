using Terraria;

namespace LivingWorldMod.Custom.Utilities {
    /// <summary>
    /// Utilities that help with unit conversions.
    /// </summary>
    public static class UnitUtils {
        /// <summary>
        /// Ticks in a real world second.
        /// </summary>
        public const int RealLifeSecond = 60;

        /// <summary>
        /// Ticks in a real world minute.
        /// </summary>
        public const int RealLifeMinute = RealLifeSecond * 60;

        /// <summary>
        /// Ticks in a real world hour.
        /// </summary>
        public const int RealLifeHour = RealLifeMinute * 60;

        /// <summary>
        /// Ticks in a real world day.
        /// </summary>
        public const int RealLifeDay = RealLifeHour * 24;

        /// <summary>
        /// Ticks in the duration of an in-game day when the sun is up.
        /// </summary>
        public const int InGameDaylight = (int)Main.dayLength;

        /// <summary>
        /// Ticks in the duration of an in-game day when the moon is out.
        /// </summary>
        public const int InGameMoonlight = (int)Main.nightLength;

        /// <summary>
        /// Ticks in the full duration of an in-game day.
        /// </summary>
        public const int InGameFullDay = InGameDaylight + InGameMoonlight;
    }
}