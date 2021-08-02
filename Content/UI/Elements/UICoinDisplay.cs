using LivingWorldMod.Custom.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI;

namespace LivingWorldMod.Content.UI.Elements {

    /// <summary>
    /// UIElement class that draws coins as either the monetary savings of the player or as some
    /// kind of coin count, such as for a price in the villager shop UI.
    /// </summary>
    public class UICoinDisplay : UIElement {

        /// <summary>
        /// Money to display. If 0, displays the player's savings instead.
        /// </summary>
        public long moneyToDisplay;

        /// <summary>
        /// The scale of the entire display and all of its elements.
        /// </summary>
        public float displayScale;

        /// <summary>
        /// Whether or not to draw coins that have a value of 0 attributed to them. For example, if
        /// the player only has 43 gold exactly, this UI won't draw the platinum coin and its
        /// respective text.
        /// </summary>
        public CoinDrawStyle coinDrawStyle;

        public UICoinDisplay(long moneyToDisplay = 0, float displayScale = 1f) {
            this.moneyToDisplay = moneyToDisplay;
            this.displayScale = displayScale;

            coinDrawStyle = CoinDrawStyle.Vanilla;

            MinWidth.Set(92f * displayScale, 0f);
            MinHeight.Set(30f * displayScale, 0f);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            Vector2 startPos = GetDimensions().Position();

            //Adapted Vanilla Code
            int[] splitCoinArray = Utils.CoinsSplit(moneyToDisplay);

            bool largerCoinHasValue = false;
            for (int i = 0; i < 4; i++) {
                Vector2 position = new Vector2(startPos.X + (24f * displayScale * i) + (14f * displayScale), startPos.Y + (14f * displayScale));

                Main.instance.LoadItem(ItemID.PlatinumCoin - i);

                if (coinDrawStyle == CoinDrawStyle.Vanilla || (coinDrawStyle == CoinDrawStyle.LargerCoinsForceDrawLesserCoins && largerCoinHasValue) || splitCoinArray[3 - i] > 0f) {
                    largerCoinHasValue = true;
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
                        position.X - (10f * displayScale),
                        position.Y,
                        Color.White,
                        Color.Black,
                        default,
                        0.75f * displayScale);
                }
            }
        }
    }
}