using System;
using System.Linq;
using LivingWorldMod.DataStructures.Records;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.UI;

namespace LivingWorldMod.Globals.UIElements;

/// <summary>
///     UIElement class that draws coins as either the monetary savings of the player or as some
///     kind of coin count, such as for a price in the villager shop UI.
/// </summary>
public class UICoinDisplay : UIElement {
    public readonly record struct CoinDrawStyle(CoinDrawCondition Condition, bool ForceDrawLesserCoins = false);

    public enum CoinDrawCondition {
        /// <summary>
        ///     This coin will always be drawn, regardless of any other coins' styles.
        /// </summary>
        Default,

        /// <summary>
        ///     Opposite of default. Will not be drawn, unless overriden.
        /// </summary>
        DoNotDraw,

        /// <summary>
        ///     Won't be drawn if it has no value.
        /// </summary>
        DrawOnlyWithNonZeroValue
    }

    private const float DefaultCoinPadding = 24f;
    private const float CoinSize = 14f;

    private const int PlatinumCoinIndex = 3;

    private readonly int[] _coinValues;
    private readonly CoinDrawStyle[] _coinDrawStyles;
    private readonly bool[] _drawnCoins;

    /// <summary>
    ///     The scale of the entire display and all of its elements.
    /// </summary>
    private readonly float _displayScale;

    /// <summary>
    ///     How much horizontal padding there is between each of the coins.
    /// </summary>
    private readonly float _coinPadding;

    public UICoinDisplay(long moneyToDisplay, float coinPadding = DefaultCoinPadding, float displayScale = 1f) : this(coinPadding, displayScale) {
        SetDisplay(moneyToDisplay, new CoinDrawStyle(CoinDrawCondition.Default));
    }

    public UICoinDisplay(long moneyToDisplay, CoinDrawStyle drawStyle, float coinPadding = DefaultCoinPadding, float displayScale = 1f) : this(coinPadding, displayScale) {
        SetDisplay(moneyToDisplay, drawStyle);
    }

    public UICoinDisplay(long moneyToDisplay, CoinDrawStyle[] drawStyles, float coinPadding = DefaultCoinPadding, float displayScale = 1f) : this(coinPadding, displayScale) {
        SetDisplay(moneyToDisplay, drawStyles);
    }

    private UICoinDisplay(float coinPadding, float displayScale) {
        _coinValues = new int[Main.InventoryCoinSlotsCount];
        _coinDrawStyles = new CoinDrawStyle[Main.InventoryCoinSlotsCount];
        _drawnCoins = new bool[Main.InventoryCoinSlotsCount];
        _coinPadding = coinPadding;
        _displayScale = displayScale;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        Vector2 startPos = GetDimensions().Position();

        int actuallyDrawnCoins = 0;
        for (int i = 0; i < Main.InventoryCoinSlotsCount; i++) {
            if (!_drawnCoins[PlatinumCoinIndex - i]) {
                continue;
            }

            Vector2 position = new(startPos.X + _coinPadding * _displayScale * actuallyDrawnCoins + CoinSize * _displayScale, startPos.Y + CoinSize * _displayScale);

            Main.instance.LoadItem(ItemID.PlatinumCoin - i);
            spriteBatch.Draw(
                TextureAssets.Item[ItemID.PlatinumCoin - i].Value,
                position,
                null,
                Color.White,
                0f,
                TextureAssets.Item[ItemID.PlatinumCoin - i].Value.Size() / 2f,
                _displayScale,
                SpriteEffects.None,
                0f
            );

            Utils.DrawBorderStringFourWay(
                spriteBatch,
                FontAssets.ItemStack.Value,
                _coinValues[PlatinumCoinIndex - i].ToString(),
                position.X - 10f * _displayScale,
                position.Y,
                Color.White,
                Color.Black,
                default(Vector2),
                0.75f * _displayScale
            );

            actuallyDrawnCoins++;
        }
    }

    public void SetNewCoinValues(long newValue) {
        Array.Copy(Utils.CoinsSplit(newValue), _coinValues, Main.InventoryCoinSlotsCount);

        DetermineDrawnCoins();
        CalculateDimensions();
    }

    public void SetDisplay(long newValue, CoinDrawStyle coinDrawStyle) => SetDisplay(newValue, Enumerable.Repeat(coinDrawStyle, Main.InventoryCoinSlotsCount).ToArray());

    public void SetDisplay(long newValue, CoinDrawStyle[] coinDrawStyles) {
        if (coinDrawStyles.Length != Main.InventoryCoinSlotsCount) {
            throw new ArgumentOutOfRangeException(nameof(coinDrawStyles), $"{nameof(coinDrawStyles)} must be of length {Main.InventoryCoinSlotsCount}");
        }

        Array.Copy(coinDrawStyles, _coinDrawStyles, Main.InventoryCoinSlotsCount);

        SetNewCoinValues(newValue);
    }

    /// <summary>
    ///     Calculates & reloads the dimensions of this display depending on the current
    ///     money being displayed.
    /// </summary>
    private void CalculateDimensions() {
        BoundedNumber<int> actuallyDrawCoins = new (0, 0, int.MaxValue) ;
        for (int i = 0; i < Main.InventoryCoinSlotsCount; i++) {
            if (!_drawnCoins[i]) {
                continue;
            }

            actuallyDrawCoins += 1;
        }

        Width.Set(((actuallyDrawCoins - 1) * _coinPadding + CoinSize * actuallyDrawCoins) * _displayScale, 0f);
        //Since height is not a factor that changes between styles, we can define height here
        Height.Set(30 * _displayScale, 0f);
    }

    /// <summary>
    ///     Given the current draw styles, determine which coins are going to be drawn, and copy that to <see cref="_drawnCoins" />.
    /// </summary>
    private void DetermineDrawnCoins() {
        bool forceDraw = false;
        for (int i = 0; i < Main.InventoryCoinSlotsCount; i++) {
            (CoinDrawCondition coinDrawCondition, bool forceDrawLesserCoins) = _coinDrawStyles[PlatinumCoinIndex - i];
            int coinValue = _coinValues[PlatinumCoinIndex - i];

            switch (coinDrawCondition) {
                default:
                case CoinDrawCondition.Default:
                    break;
                case CoinDrawCondition.DoNotDraw when !forceDraw:
                case CoinDrawCondition.DrawOnlyWithNonZeroValue when coinValue == 0 && !forceDraw:
                    continue;
            }

            forceDraw |= forceDrawLesserCoins;
            _drawnCoins[PlatinumCoinIndex - i] = true;
        }
    }
}