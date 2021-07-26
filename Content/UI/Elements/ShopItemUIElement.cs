using Terraria.GameContent.UI.Elements;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace LivingWorldMod.Content.UI.Elements {

    /// <summary>
    /// UIElement class that holds different UIImages and UIElements that is the index in a given
    /// shop UI list. Holds data on the entire entry for the given item.
    /// </summary>
    public class ShopItemUIElement : UIElement {
        public UIImage shopItemFrame;
        public UIBetterItemIcon itemImage;

        public UIText itemNameText;
        public UIText stockText;
        public UIText itemCostText;

        public VillagerType villagerType;

        private Item displayedItem;
        private int costPerItem;
        private Rectangle visibleListArea;

        public ShopItemUIElement(int itemType, int remainingStock, int costPerItem, VillagerType villagerType, Rectangle visibleListArea) {
            shopItemFrame = new UIImage(ModContent.Request<Texture2D>($"{IOUtilities.LWMSpritePath}/UI/ShopUI/{villagerType}/{villagerType}ShopItemFrame"));
            displayedItem = new Item();
            displayedItem.SetDefaults(itemType);
            itemImage = new UIBetterItemIcon(displayedItem);
            itemNameText = new UIText(displayedItem.HoverName, 1.25f);
            stockText = new UIText(remainingStock.ToString(), large: true);
            this.costPerItem = costPerItem;
            this.villagerType = villagerType;
            this.visibleListArea = visibleListArea;
            Activate();
        }

        public override void OnInitialize() {
            Width.Set(424f, 0f);
            Height.Set(110f, 0f);
            Append(shopItemFrame);

            itemImage.Top.Set(52f, 0f);
            itemImage.Left.Set(52f, 0f);
            shopItemFrame.Append(itemImage);

            itemNameText.Top.Set(44f, 0f);
            itemNameText.Left.Set(80f, 0f);
            shopItemFrame.Append(itemNameText);

            stockText.Top.Set(34f, 0f);
            stockText.Left.Set(itemNameText.Left.Pixels + 156f, 0f);
            shopItemFrame.Append(stockText);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            CalculatedStyle style = itemImage.GetDimensions();

            //So trying to figure out how to get hover functionality has been extremely messy. No idea why simply using the OnHover delegate didn't work, so I eventually arrived at this
            //This has been a headache, I have not the SLIGHTEST clue why this was so difficult, but this luckily works so I will use it even if it's kinda disgusting/hardcoded
            Rectangle dimensions = new Rectangle((int)style.X - 16, (int)style.Y - 16, 32, 32);
            if (dimensions.Contains(Main.mouseX, Main.mouseY) && visibleListArea.Contains(Main.mouseX, Main.mouseY)) {
                ItemSlot.MouseHover(ref displayedItem, itemImage.context);
            }
        }
    }
}