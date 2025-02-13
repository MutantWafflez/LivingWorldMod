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

    /// <summary>
    ///     The panel background color used with various UIs in LWM.
    /// </summary>
    public static readonly Color LWMCustomUIPanelBackgroundColor = new (59, 97, 203);

    /// <summary>
    ///     The background color of the sub-panels within pre-existing panels used with various UIs in LWM.
    /// </summary>
    public static readonly Color LWMCustomUISubPanelBackgroundColor = new (46, 46, 159);

    /// <summary>
    ///     The border color of the sub-panels within pre-existing panels used with various UIs in LWM.
    /// </summary>
    public static readonly Color LWMCustomUISubPanelBorderColor = new (22, 29, 107);

    /// <summary>
    ///     The default background color that vanilla uses for its UI Panels.
    /// </summary>
    public static readonly Color UIPanelBackgroundColor = new Color(63, 82, 151) * 0.7f;
}