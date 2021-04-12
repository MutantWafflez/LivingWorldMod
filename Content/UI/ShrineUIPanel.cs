using System;
using LivingWorldMod.Content.Tiles.Interactables;
using LivingWorldMod.Content.UI.Elements;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace LivingWorldMod.Content.UI
{
    public class ShrineUIPanel : CustomUIPanel
    {
        //TODO: FIX MENU MOVING WITH UI ZOOM/SCALE

        public static ShrineUIPanel Instance;
        public static VillagerID shrineType;
        private const float width = 304f, height = 214f;
        private const string texturePath = "LivingWorldMod/Assets/Textures/UIElements/ShrineMenu/";
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
                new CustomUIImage(GetTexture(texturePath + "Shrine_Base"), 1f);
            background.Width.Set(0, 0);
            background.Height.Set(0f, 0);
            Append(background);

            //Menu Title
            CustomUIText title = new CustomUIText("Shrine Offerings", 1.2f);
            title.Left.Set(10f, 0);
            title.Top.Set(7f, 0);
            Append(title);

            //Cross to Close Menu
            Texture2D crossTexture = GetTexture(texturePath + "Shrine_Exit1");
            Texture2D crossTexture2 = GetTexture(texturePath + "Shrine_Exit2");
            CustomUIImage closeMenuCross = new CustomUIImage(crossTexture, 1f);
            closeMenuCross.Width.Set(22f, 0);
            closeMenuCross.Height.Set(22f, 0);
            closeMenuCross.Left.Set(width - 31f, 0);
            closeMenuCross.Top.Set(6f, 0);
            closeMenuCross.OnMouseOver += (__, _) => { closeMenuCross.SetImage(crossTexture2); };
            closeMenuCross.OnMouseOut += (__, _) => { closeMenuCross.SetImage(crossTexture); };
            closeMenuCross.OnMouseDown += BounceMouseDown;
            closeMenuCross.OnMouseUp += BounceMouseUp;
            closeMenuCross.OnClick += (__, _) => Hide();
            Append(closeMenuCross);

            #region Item in itemSlot info

            //Gift Item Image
            CustomUIImage giftItem = new CustomUIImage(Main.itemTexture[0], 1f);
            giftItem.SetScaleToFit(true);
            giftItem.Left.Set(197f, 0);
            giftItem.Top.Set(62f, 0);
            Append(giftItem);
            giftItem.Hide();

            //Undiscovered Gift
            CustomUIText undiscoveredGift = new CustomUIText("?", 1f)
            {
                TextColor = Color.LightGray
            };
            undiscoveredGift.Left.Set(227f, 0);
            undiscoveredGift.Top.Set(67f, 0);
            Append(undiscoveredGift);
            undiscoveredGift.Hide();

            //Amount gifted
            CustomUIText giftAmount = new CustomUIText("", 0.9f);
            giftAmount.Left.Set(142f, 0);
            giftAmount.Top.Set(57f, 0);
            Append(giftAmount);
            giftAmount.Hide();

            //Liking Neutral
            CustomUIText giftLiking = new CustomUIText("", 0.9f);
            giftLiking.Left.Set(142f, 0);
            giftLiking.Top.Set(77f, 0);
            Append(giftLiking);
            giftLiking.Hide();

            #endregion Item in itemSlot gift info

            //ItemSlot
            itemSlot = new CustomItemSlot();
            itemSlot.Width.Set(54f, 0);
            itemSlot.Height.Set(54f, 0);
            itemSlot.Left.Set(17f, 0);
            itemSlot.Top.Set(47f, 0);
            itemSlot.OnItemEquipped += _ => ShowItemSlotInfo();
            itemSlot.OnItemRemoved += _ => HideItemSlotInfo();
            itemSlot.SetBackgroundTexture(GetTexture(texturePath + "Shrine_Slot"));
            Append(itemSlot);

            //Gift
            Texture2D giftTexture = GetTexture(texturePath + "Shrine_Button1");
            Texture2D giftTexture2 = GetTexture(texturePath + "Shrine_Button2");
            Texture2D giftTexture3 = GetTexture(texturePath + "Shrine_Button3");
            CustomUIImage gift = new CustomUIImage(giftTexture, 1f);
            gift.Width.Set(26f, 0);
            gift.Height.Set(26f, 0);
            gift.Left.Set(78f, 0);
            gift.Top.Set(56f, 0);
            gift.OnMouseOver += (__, _) => { gift.SetImage(giftTexture2); };
            gift.OnMouseOut += (__, _) => { gift.SetImage(giftTexture); };
            gift.OnMouseDown += (evt, e) =>
            {
                BounceMouseDown(evt, e);
                gift.SetImage(giftTexture3);
            };
            gift.OnMouseUp += (evt, e) =>
            {
                BounceMouseUp(evt, e);
                gift.SetImage(gift.IsMouseHovering ? giftTexture2 : giftTexture);
            };
            gift.OnClick += (__, _) =>
            {
                GiftItem();
                HideItemSlotInfo();
            };
            Append(gift);

            //Gifts Text
            CustomUIText repText = new CustomUIText("Gifts: ", 1.1f);
            repText.Left.Set(23f, 0);
            repText.Top.Set(125f, 0);
            Append(repText);

            Texture2D repBackground =
                ModContent.GetTexture(texturePath + "Shrine_Meter1");
            Texture2D repProgress =
                ModContent.GetTexture(texturePath + "Shrine_Meter3");
            CustomProgressBar repFrame = new CustomProgressBar(repBackground, repProgress, 100, true);
            repFrame.Width.Set(204f, 0);
            repFrame.Height.Set(24f, 0);
            repFrame.Left.Set(80f, 0);
            repFrame.Top.Set(122f, 0);
            repFrame.Activate();
            Append(repFrame);

            //Stage Text
            CustomUIText stageText = new CustomUIText("Stage: ", 1.1f);
            stageText.Left.Set(20f, 0);
            stageText.Top.Set(165f, 0);
            Append(stageText);

            //Stage ProgressBar
            Texture2D stageBackground =
                ModContent.GetTexture(texturePath + "Shrine_Meter2");
            Texture2D stageProgress =
                ModContent.GetTexture(texturePath + "Shrine_Meter4");
            CustomProgressBar stageFrame = new CustomProgressBar(stageBackground, stageProgress, 5, false);
            stageFrame.Width.Set(204f, 0);
            stageFrame.Height.Set(24f, 0);
            stageFrame.Left.Set(80f, 0);
            stageFrame.Top.Set(162f, 0);
            stageFrame.Activate();
            Append(stageFrame);

            #region Methods

            void BounceMouseDown(UIMouseEvent evt, UIElement element)
            {
                if (element is CustomUIImage img)
                    img.Top.Set(img.Top.Pixels + 1f, 0);
            }

            void BounceMouseUp(UIMouseEvent evt, UIElement element)
            {
                if (element is CustomUIImage img)
                    img.Top.Set(img.Top.Pixels - 1f, 0);
            }

            void GiftItem()
            {
                int itemType = itemSlot.Item.type;
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
                    color = Color.White;

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

            #endregion Methods

            #endregion UI Initialization
        }

        /// <summary>
        /// Opens the Shrine UI Menu for the given VillagerType.
        /// </summary>
        /// <param name="i"> Tile Coordinate in the X axis. </param>
        /// <param name="j"> Tile Coordinate in the Y axis. </param>
        /// <param name="villageShrineType"> Type of villager. </param>
        public static void TileRightClicked(int i, int j, VillagerID villageShrineType)
        {
            shrineType = villageShrineType;
            Instance.Toggle();
            itemSlotPos = WorldGenUtils.FindMultiTileTopLeft(i, j, TileType<HarpyShrineTile>()) + new Vector2(-8, -14);
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