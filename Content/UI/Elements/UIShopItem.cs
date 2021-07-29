using System.Runtime.CompilerServices;
using LivingWorldMod.Common.Systems.UI;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.UI.Elements {

    /// <summary>
    /// UIImage class that holds different UITexts and UIElements that is the index in a given shop
    /// UI list. Holds data on the entire entry for the given item.
    /// </summary>
    public class UIShopItem : UIImage {
        public UIBetterItemIcon itemImage;
        public UIBetterText itemNameText;
        public UICoinDisplay itemCostDisplay;

        public Item displayedItem;

        public int remainingStock;
        public ulong costPerItem;

        public VillagerType villagerType;

        private float manualUpdateTime;

        public UIShopItem(int itemType, int remainingStock, ulong costPerItem, VillagerType villagerType) : base(ModContent.Request<Texture2D>($"{IOUtilities.LWMSpritePath}/UI/ShopUI/{villagerType}/{villagerType}ShopItemFrame")) {
            displayedItem = new Item();
            displayedItem.SetDefaults(itemType);
            this.remainingStock = remainingStock;
            this.costPerItem = costPerItem;
            this.villagerType = villagerType;
        }

        public override void OnInitialize() {
            itemImage = new UIBetterItemIcon(displayedItem, 32f, true) {
                VAlign = 0.5f
            };
            itemImage.Left.Set(28f, 0f);
            itemImage.Width.Set(32f, 0f);
            itemImage.Height.Set(32f, 0f);
            Append(itemImage);

            itemNameText = new UIBetterText(displayedItem.HoverName, 1.25f) {
                VAlign = 0.5f,
                horizontalTextConstraint = 176f
            };
            itemNameText.Left.Set(66f, 0f);
            Append(itemNameText);

            itemCostDisplay = new UICoinDisplay(costPerItem, 1.34f, true) {
                VAlign = 0.5f,
                drawCoinsWithNoValue = false
            };
            itemCostDisplay.Left.Set(260f, 0f);
            Append(itemCostDisplay);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            if (ContainsPoint(Main.MouseScreen)) {
                RasterizerState defaultRasterizerState = new RasterizerState { CullMode = CullMode.None, ScissorTestEnable = true };

                Effect shader = ShopUISystem.hoverFlashShader.Value;

                manualUpdateTime += 1f / 45f;
                if (manualUpdateTime >= MathHelper.TwoPi) {
                    manualUpdateTime = 0f;
                }

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, defaultRasterizerState, shader, Main.UIScaleMatrix);

                //So I am unsure as to why exactly this needed to be done, cause this is definitely the definition of a band-aid fix.
                //In short, when using this shader, uTime isn't being updated at all, causing the shader to just stay one color instead of breathing in a sine wave fashion like intended.
                //Thus, for the time being, until I can figure out why uTime isn't being automatically updated, I am manually setting this new Parameter
                shader.Parameters["manualUTime"].SetValue(manualUpdateTime);
                base.DrawSelf(spriteBatch);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, defaultRasterizerState, null, Main.UIScaleMatrix);
            }
            else {
                base.DrawSelf(spriteBatch);
            }
        }
    }
}