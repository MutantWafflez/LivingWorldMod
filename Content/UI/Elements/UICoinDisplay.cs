using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;

namespace LivingWorldMod.Content.UI.Elements {

    /// <summary>
    /// UIElement class that draws coins as either the monetary savings of the player or as some
    /// kind of coin count, such as for a price in the villager shop UI.
    /// </summary>
    public class UICoinDisplay : UIElement {

        /// <summary>
        /// Money to display. If 0, displays the player's savings instead.
        /// </summary>
        public ulong moneyToDisplay;

        /// <summary>
        /// The scale of the entire display and all of its elements.
        /// </summary>
        public float displayScale;

        /// <summary>
        /// Whether or not to draw the "Savings" text and the actual numbers/coin icons in one
        /// straight line with no line break.
        /// </summary>
        public bool drawHorizontally;

        public UICoinDisplay(ulong moneyToDisplay = 0, float displayScale = 1f, bool drawHorizontally = false) {
            this.moneyToDisplay = moneyToDisplay;
            this.displayScale = displayScale;
            this.drawHorizontally = drawHorizontally;

            MinWidth.Set((drawHorizontally ? (moneyToDisplay == 0 ? 192f : 98f) : 92f) * displayScale, 0f);
            MinHeight.Set((drawHorizontally ? 30f : 52f) * displayScale, 0f);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            Vector2 startPos = GetDimensions().Position();

            if (moneyToDisplay == 0) {
                //Adapted Vanilla Code
                Player player = Main.LocalPlayer;

                //Here in vanilla, not sure why, but it is necessary for CoinsCount to work so here it is.
                bool _;

                long playerInvCashCount = Utils.CoinsCount(out _, player.inventory);
                long piggyCashCount = Utils.CoinsCount(out _, player.bank.item);
                long safeCashCount = Utils.CoinsCount(out _, player.bank2.item);
                long defForgeCashCount = Utils.CoinsCount(out _, player.bank3.item);
                long voidVaultCashCount = Utils.CoinsCount(out _, player.bank4.item);
                long combinedCashCount = Utils.CoinsCombineStacks(out _, playerInvCashCount, piggyCashCount, safeCashCount, defForgeCashCount, voidVaultCashCount);

                if (combinedCashCount > 0) {
                    int[] splitCoinArray = Utils.CoinsSplit(combinedCashCount);
                    string savingsText = Language.GetTextValue("LegacyInterface.66");

                    Main.instance.LoadItem(ItemID.PiggyBank);

                    spriteBatch.Draw(TextureAssets.Item[ItemID.PiggyBank].Value,
                        Utils.CenteredRectangle(new Vector2(startPos.X + (70f * displayScale), startPos.Y + (20f * displayScale)),
                            TextureAssets.Item[87].Value.Size() * 0.65f * displayScale),
                        null,
                        Color.White);

                    Utils.DrawBorderStringFourWay(spriteBatch,
                        FontAssets.MouseText.Value,
                        savingsText,
                        startPos.X,
                        startPos.Y,
                        Color.White,
                        Color.Black,
                        Vector2.Zero,
                        displayScale);

                    if (drawHorizontally) {
                        for (int i = 0; i < 4; i++) {
                            Vector2 position = new Vector2(startPos.X + ChatManager.GetStringSize(FontAssets.MouseText.Value, savingsText, Vector2.One * displayScale).X + (24f * displayScale * i) + 45f, startPos.Y + (50f * displayScale));

                            Main.instance.LoadItem(74 - i);

                            spriteBatch.Draw(TextureAssets.Item[74 - i].Value,
                                position,
                                null,
                                Color.White,
                                0f,
                                TextureAssets.Item[74 - i].Value.Size() / 2f,
                                displayScale,
                                SpriteEffects.None,
                                0f);

                            Utils.DrawBorderStringFourWay(spriteBatch,
                                FontAssets.ItemStack.Value,
                                splitCoinArray[3 - i].ToString(),
                                position.X - 11f, position.Y,
                                Color.White,
                                Color.Black,
                                new Vector2(0.3f),
                                0.75f * displayScale);
                        }
                    }
                    else {
                        for (int i = 0; i < 4; i++) {
                            int platinumOverflowDisplacement = i == 0 && splitCoinArray[3 - i] > 99 ? -6 : 0;

                            Main.instance.LoadItem(ItemID.PlatinumCoin - i);

                            spriteBatch.Draw(TextureAssets.Item[ItemID.PlatinumCoin - i].Value,
                                new Vector2(startPos.X + 11f + (24f * displayScale * i), startPos.Y + 35f),
                                null,
                                Color.White,
                                0f,
                                TextureAssets.Item[74 - i].Value.Size() / 2f,
                                displayScale,
                                SpriteEffects.None,
                                0f);

                            Utils.DrawBorderStringFourWay(spriteBatch,
                                FontAssets.ItemStack.Value,
                                splitCoinArray[3 - i].ToString(),
                                startPos.X + (24f * displayScale * i) + platinumOverflowDisplacement,
                                startPos.Y + 35f,
                                Color.White,
                                Color.Black,
                                new Vector2(0.3f),
                                0.75f * displayScale);
                        }
                    }
                }
            }
            else {
                //Also Adapted Vanilla Code
                int[] splitCoinArray = Utils.CoinsSplit((long)moneyToDisplay);

                for (int i = 0; i < 4; i++) {
                    Vector2 position = new Vector2(startPos.X + (24f * displayScale * i) + (14f * displayScale), startPos.Y + (14f * displayScale));

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