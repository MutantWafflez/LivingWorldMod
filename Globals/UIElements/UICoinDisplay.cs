using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.UI;

namespace LivingWorldMod.Globals.UIElements;

/// <summary>
/// UIElement class that draws coins as either the monetary savings of the player or as some
/// kind of coin count, such as for a price in the villager shop UI.
/// </summary>
public class UICoinDisplay : UIElement {
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

    /// <summary>
    /// The total monetary value to display on this element.
    /// </summary>
    public long moneyToDisplay;

    /// <summary>
    /// The scale of the entire display and all of its elements.
    /// </summary>
    public float displayScale;

    /// <summary>
    /// Controls the method by which the coins in this display are drawn. Read each of the
    /// enum's summary to understand in more detail. Defaults to vanilla's drawing style.
    /// </summary>
    public CoinDrawStyle coinDrawStyle;

    public UICoinDisplay(long moneyToDisplay, CoinDrawStyle coinDrawStyle = CoinDrawStyle.Vanilla, float displayScale = 1f) {
        this.moneyToDisplay = moneyToDisplay;
        this.coinDrawStyle = coinDrawStyle;
        this.displayScale = displayScale;

        CalculateDimensions();
    }

    /// <summary>
    /// Calculates & reloads the dimensions of this display depending on the current
    /// money being displayed.
    /// </summary>
    public void CalculateDimensions() {
        int[] splitCoinArray = Utils.CoinsSplit(moneyToDisplay);

        //Since height is not a factor that changes between styles, we can define height here
        Height.Set(30 * displayScale, 0f);

        switch (coinDrawStyle) {
            case CoinDrawStyle.Vanilla:
                Width.Set(94f * displayScale, 0f);

                break;

            case CoinDrawStyle.NoCoinsWithZeroValue:
                float finalWidth = 0f;

                for (int i = 0; i < splitCoinArray.Length; i++) {
                    if (splitCoinArray[i] != 0f) {
                        finalWidth += 24; //Coin + padding size
                    }
                }

                Width.Set(finalWidth * displayScale, 0f);

                break;

            case CoinDrawStyle.LargerCoinsForceDrawLesserCoins:
                bool largerCoinHasValue = false;
                finalWidth = 0f;

                for (int i = 0; i < splitCoinArray.Length; i++) {
                    if (splitCoinArray[i] != 0f || largerCoinHasValue) {
                        largerCoinHasValue = true;
                        finalWidth += 24; //Coin + padding size
                    }
                }

                Width.Set(finalWidth * displayScale, 0f);

                break;

            default:
                ModContent.GetInstance<LWM>().Logger.Error($"Invalid CoinDrawStyle found: {coinDrawStyle}");

                break;
        }
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        Vector2 startPos = GetDimensions().Position();

        int[] splitCoinArray = Utils.CoinsSplit(moneyToDisplay);

        switch (coinDrawStyle) {
            case CoinDrawStyle.Vanilla:
                //Adapted Vanilla Code

                for (int i = 0; i < splitCoinArray.Length; i++) {
                    Vector2 position = new(startPos.X + 24f * displayScale * i + 14f * displayScale, startPos.Y + 14f * displayScale);

                    Main.instance.LoadItem(ItemID.PlatinumCoin - i);

                    spriteBatch.Draw(TextureAssets.Item[ItemID.PlatinumCoin - i].Value,
                        position,
                        null,
                        Color.White,
                        0f,
                        TextureAssets.Item[ItemID.PlatinumCoin - i].Value.Size() / 2f,
                        displayScale,
                        SpriteEffects.None,
                        0f);

                    Utils.DrawBorderStringFourWay(spriteBatch,
                        FontAssets.ItemStack.Value,
                        splitCoinArray[3 - i].ToString(),
                        position.X - 10f * displayScale,
                        position.Y,
                        Color.White,
                        Color.Black,
                        default(Vector2),
                        0.75f * displayScale);
                }

                break;

            case CoinDrawStyle.NoCoinsWithZeroValue:
                int actuallyDrawnCoins = 0;

                for (int i = 0; i < 4; i++) {
                    if (splitCoinArray[3 - i] != 0f) {
                        Vector2 position = new(startPos.X + 24f * displayScale * actuallyDrawnCoins + 14f * displayScale, startPos.Y + 14f * displayScale);

                        Main.instance.LoadItem(ItemID.PlatinumCoin - i);

                        spriteBatch.Draw(TextureAssets.Item[ItemID.PlatinumCoin - i].Value,
                            position,
                            null,
                            Color.White,
                            0f,
                            TextureAssets.Item[ItemID.PlatinumCoin - i].Value.Size() / 2f,
                            displayScale,
                            SpriteEffects.None,
                            0f);

                        Utils.DrawBorderStringFourWay(spriteBatch,
                            FontAssets.ItemStack.Value,
                            splitCoinArray[3 - i].ToString(),
                            position.X - 10f * displayScale,
                            position.Y,
                            Color.White,
                            Color.Black,
                            default(Vector2),
                            0.75f * displayScale);

                        actuallyDrawnCoins++;
                    }
                }

                break;

            case CoinDrawStyle.LargerCoinsForceDrawLesserCoins:
                bool largerCoinHasValue = false;
                actuallyDrawnCoins = 0;

                for (int i = 0; i < 4; i++) {
                    if (splitCoinArray[3 - i] != 0f || largerCoinHasValue) {
                        Vector2 position = new(startPos.X + 24f * displayScale * actuallyDrawnCoins + 14f * displayScale, startPos.Y + 14f * displayScale);

                        Main.instance.LoadItem(ItemID.PlatinumCoin - i);

                        spriteBatch.Draw(TextureAssets.Item[ItemID.PlatinumCoin - i].Value,
                            position,
                            null,
                            Color.White,
                            0f,
                            TextureAssets.Item[ItemID.PlatinumCoin - i].Value.Size() / 2f,
                            displayScale,
                            SpriteEffects.None,
                            0f);

                        Utils.DrawBorderStringFourWay(spriteBatch,
                            FontAssets.ItemStack.Value,
                            splitCoinArray[3 - i].ToString(),
                            position.X - 10f * displayScale,
                            position.Y,
                            Color.White,
                            Color.Black,
                            default(Vector2),
                            0.75f * displayScale);

                        actuallyDrawnCoins++;
                        largerCoinHasValue = true;
                    }
                }

                break;

            default:
                ModContent.GetInstance<LWM>().Logger.Error($"Invalid CoinDrawStyle found: {coinDrawStyle}");
                break;
        }
    }
}