using LivingWorldMod.DataStructures.Structs;
using LivingWorldMod.Utilities;
using Terraria.GameContent.Events;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;

public sealed class TownNPCSleepModule (NPC npc) : TownNPCModule(npc) {
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

    /// <summary>
    ///     Current amount of sleep "points" that the NPC has stored. Gives various mood boosts/losses based on how many points the NPC has at any given point in time.
    /// </summary>
    public BoundedNumber<float> sleepValue = new (0, 0, LWMUtils.InGameMoonlight);

    public static bool ShouldSleep => !(Main.dayTime || LanternNight.LanternsUp || Main.bloodMoon || Main.snowMoon || Main.pumpkinMoon);

    public override void Update() {
        if (!ShouldSleep) {
            sleepValue -= 1f;
        }
    }
}