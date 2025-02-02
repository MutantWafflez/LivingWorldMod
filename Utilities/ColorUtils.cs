using Microsoft.Xna.Framework;

namespace LivingWorldMod.Utilities;

// Utilities section that includes various Color constants from Vanilla and helpers
public static partial class LWMUtils {
    /// <summary>
    ///     The color of the chat text from Vanilla for various "errors" or "issues", namely for Pylon teleportation and housing requirement problems.
    /// </summary>
    public static readonly Color YellowErrorTextColor = new (255, 240, 20);

    /// <summary>
    ///     The color of the chat text from Vanilla when a naturally occurring parties happens.
    /// </summary>
    public static readonly Color DarkPinkPartyTextColor = new(255, 0, 160);

    /// <summary>
    ///     The color of the chat text from Vanilla when a Town NPC dies or "leaves."
    /// </summary>
    public static readonly Color RedTownNPCDeathTextColor = new(255, 25, 25);
}