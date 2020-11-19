using System;
using BuilderEssentials.UI.Elements;
using LivingWorldMod.NPCs.Villagers;
using LivingWorldMod.Tiles.Interactables;
using LivingWorldMod.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace LivingWorldMod.UI
{
    public class ShrineUIPanel : CustomUIPanel
    {
        public static ShrineUIPanel Instance;
        public static VillagerType shrineType;
        private const float width = 253f, height = 167f;
        private static CustomItemSlot itemSlot;
        private static Vector2 itemSlotPos = Vector2.Zero;

        public ShrineUIPanel(float scale = 1f, float opacity = 1f) : base(scale, opacity)
        {
            Instance = this;

            Width.Set(width, 0);
            Height.Set(height, 0);
            Left.Set(Main.screenWidth / 2 - width, 0);
            Top.Set(Main.screenHeight / 2 - height, 0);
            SetPadding(0);
            BorderColor = Microsoft.Xna.Framework.Color.Transparent;
            BackgroundColor = Microsoft.Xna.Framework.Color.Transparent;

            #region UI Initialization

            //Background
            CustomUIImage background =
                new CustomUIImage(GetTexture("LivingWorldMod/Textures/UIElements/ShrineMenu/Background"), 1f);
            background.Width.Set(0, 0);
            background.Height.Set(0f, 0);
            background.Left.Set(-12f, 0);
            background.Top.Set(-12f, 0);
            Append(background);

            //Cross to Close Menu
            CustomUIImage closeMenuCross =
                new CustomUIImage(GetTexture("LivingWorldMod/Textures/UIElements/ShrineMenu/CloseCross"), 1f);
            closeMenuCross.Width.Set(19f, 0);
            closeMenuCross.Height.Set(19f, 0);
            closeMenuCross.Left.Set(width - 35f, 0);
            closeMenuCross.Top.Set(-7f, 0);
            closeMenuCross.OnClick += (__, _) => Hide();
            Append(closeMenuCross);

            #region Item in itemSlot info

            //Gift Item Image
            CustomUIImage giftItem = new CustomUIImage(Main.itemTexture[0], 1f);
            giftItem.SetScaleToFit(true);
            giftItem.Left.Set(165f, 0);
            giftItem.Top.Set(35f, 0);
            Append(giftItem);
            giftItem.Hide();

            //Undiscovered Gift
            CustomUIText undiscoveredGift = new CustomUIText("?", 1f);
            undiscoveredGift.TextColor = Microsoft.Xna.Framework.Color.LightGray;
            undiscoveredGift.Left.Set(195f, 0);
            undiscoveredGift.Top.Set(40f, 0);
            Append(undiscoveredGift);
            undiscoveredGift.Hide();


            //Amount gifted
            CustomUIText giftAmount = new CustomUIText("", 0.9f);
            giftAmount.Left.Set(110f, 0);
            giftAmount.Top.Set(30f, 0);
            Append(giftAmount);
            giftAmount.Hide();


            //Liking Neutral
            CustomUIText giftLiking = new CustomUIText("", 0.9f);
            giftLiking.Left.Set(110f, 0);
            giftLiking.Top.Set(50f, 0);
            Append(giftLiking);
            giftLiking.Hide();

            #endregion

            //ItemSlot
            itemSlot = new CustomItemSlot();
            itemSlot.Width.Set(54f, 0);
            itemSlot.Height.Set(54f, 0);
            itemSlot.Left.Set(5f, 0);
            itemSlot.Top.Set(25f, 0);
            itemSlot.OnItemEquipped += _ => ShowItemSlotInfo();
            itemSlot.OnItemRemoved += _ => HideItemSlotInfo();
            itemSlot.SetBackgroundTexture(GetTexture("LivingWorldMod/Textures/UIElements/ShrineMenu/ItemSlot"));
            Append(itemSlot);

            //Gift
            Texture2D giftTexture = GetTexture("LivingWorldMod/Textures/UIElements/ShrineMenu/Gift");
            Texture2D giftTexture2 = GetTexture("LivingWorldMod/Textures/UIElements/ShrineMenu/Gift2");
            UIImageButton gift = new UIImageButton(giftTexture);
            gift.Width.Set(26f, 0);
            gift.Height.Set(26f, 0);
            gift.Left.Set(66f, 0);
            gift.Top.Set(38f, 0);
            gift.SetVisibility(1f, 1f);
            gift.OnMouseOver += (__, _) => { gift.SetImage(giftTexture2); };
            gift.OnMouseOut += (__, _) => { gift.SetImage(giftTexture); };
            gift.OnMouseDown += (__, _) =>
            {
                gift.Top.Set(gift.Top.Pixels + 1f, 0);
                GiftItem();
                HideItemSlotInfo();
            };
            gift.OnMouseUp += (__, _) => { gift.Top.Set(gift.Top.Pixels - 1f, 0); };
            Append(gift);

            //Gifts Text
            CustomUIText repText = new CustomUIText("Gifts: ", 0.8f);
            repText.Left.Set(8f, 0);
            repText.Top.Set(95f, 0);
            Append(repText);

            //Gifts ProgressBar
            Color Color = new Color(63, 113, 169);
            ShrineProgressBar repFrame = new ShrineProgressBar(true, 100, Color);
            repFrame.Width.Set(170f, 0);
            repFrame.Height.Set(16f, 0);
            repFrame.Left.Set(48f, 0);
            repFrame.Top.Set(95f, 0);
            repFrame.Activate();
            Append(repFrame);

            //Stage Text
            CustomUIText stageText = new CustomUIText("Stage: ", 0.8f);
            stageText.Left.Set(5f, 0);
            stageText.Top.Set(120f, 0);
            Append(stageText);

            //Stage ProgressBar
            ShrineProgressBar stageFrame = new ShrineProgressBar(false, 5, Color);
            stageFrame.Width.Set(170f, 0);
            stageFrame.Height.Set(16f, 0);
            stageFrame.Left.Set(48f, 0);
            stageFrame.Top.Set(120f, 0);
            stageFrame.Activate();
            Append(stageFrame);

            #region Methods

            void GiftItem()
            {
                int itemType = itemSlot.Item.type;
                Main.NewText("itemSlot itemType: " + itemType);
                LWMWorld.AddGiftToProgress(shrineType, itemType);
                itemSlot.Item.TurnToAir();
            }
            
            void ShowItemSlotInfo()
            {
                giftItem.SetImage(Main.itemTexture[itemSlot.Item.type]);
                giftItem.SetSize(24f, 24f);

                Color color;
                if (LWMWorld.GetGiftAmount(shrineType, itemSlot.Item.type) == 0)
                {
                    color = new Color(66, 66, 66);
                    giftItem.Show();
                    undiscoveredGift.Show();
                }
                else
                    color = Microsoft.Xna.Framework.Color.White;

                giftItem.SetOverlayColor(color);

                //Update Amount Gifted
                int amount = LWMWorld.GetGiftAmount(shrineType, itemSlot.Item.type);
                string amountText = "Gifted: " + (amount == 0 ? "" : amount.ToString());
                giftAmount.SetText(amountText);

                //Update Gift Liking
                int liking = LivingWorldMod.GetGiftValue(shrineType, itemSlot.Item.type);

                string likingText = "Liking: ";
                if (amount != 0 && Math.Abs(liking) == 3)
                    likingText += liking > 0 ? "Good" : "Bad";
                else if (amount != 0 && Math.Abs(liking) == 5)
                    likingText += liking > 0 ? "Great" : "Awful";
                else if (amount != 0 && liking == 0)
                    likingText += "Neutral";
                giftLiking.SetText(likingText);

                giftAmount.Show();
                giftLiking.Show();
            }

            void HideItemSlotInfo()
            {
                giftItem.Hide();
                giftAmount.Hide();
                giftLiking.Hide();
                undiscoveredGift.Hide();
            }

            #endregion

            #endregion
        }

        public static void TileRightClicked(int i, int j, VillagerType villageShrineType)
        {
            shrineType = villageShrineType;
            Instance.Toggle();

            //Finding top left corner
            for (int k = 0; k < 4; k++)
            {
                for (int l = 0; l < 5; l++)
                {
                    if (Framing.GetTileSafely(i - k, j - l).type == TileType<HarpyShrineTile>())
                        itemSlotPos = new Vector2(i - k, j - l);
                    else continue;
                }
            }

            itemSlotPos += new Vector2(-5, -11); //offset from the topleft corner (in tiles)
        }

        public void Update()
        {
            if (Visible && IsMouseHovering)
                Main.LocalPlayer.mouseInterface = true;

            Vector2 itemSlotWorldPos = itemSlotPos.ToWorldCoordinates() - Main.screenPosition;
            Left.Set(itemSlotWorldPos.X, 0);
            Top.Set(itemSlotWorldPos.Y, 0);
        }
    }
}