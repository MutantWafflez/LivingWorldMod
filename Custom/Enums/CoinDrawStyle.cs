namespace LivingWorldMod.Custom.Enums;

/// <summary>
/// The style of how coins are drawn in a UICoinDisplay.
/// </summary>
public enum CoinDrawStyle {
    /// <summary>
    /// Vanilla drawing style. This means that all coins will be drawn regardless of what their
    /// value is.
    /// </summary>
    Vanilla,

    /// <summary>
    /// Any coin, regardless of the statuses of other coins, won't be drawn if it has no value.
    /// </summary>
    NoCoinsWithZeroValue,

    /// <summary>
    /// Same as NoCoinsWithZeroValue, except if a larger coin has a value and a lesser coin
    /// doesn't, the lesser coin will be drawn. For example, if the display is showing 1 gold
    /// exactly, it will display 1 gold 0 silver and 0 copper.
    /// </summary>
    LargerCoinsForceDrawLesserCoins
}