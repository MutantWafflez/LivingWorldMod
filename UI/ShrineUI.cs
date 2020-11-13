using BuilderEssentials.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using static Terraria.ModLoader.ModContent;
using LivingWorldMod.UI.Elements;
using LivingWorldMod.Tiles.Interactables;
using LivingWorldMod.NPCs.Villagers;

namespace LivingWorldMod.UI
{
    class ShrineUIState : UIState
    {
        public static ShrineUIState Instance;
        public static bool showUI;
        public static Vector2 itemSlotPos;
        private static CustomItemSlot itemSlot;
        private static UIText hoverItemSlotText;
        public static VillagerType shrineType;

        public static UIPanel SMPanel;
        private static float SMWidth;
        private static float SMHeight;


        public override void OnInitialize()
        {
            Instance = this;
            showUI = false;
            itemSlotPos = Vector2.Zero;
            itemSlot = null;
        }

        public static void CreateShapesMenuPanel()
        {
            //Texture dimensions
            SMWidth = 213f;
            SMHeight = 167f;

            SMPanel = new UIPanel();
            SMPanel.Width.Set(SMWidth, 0);
            SMPanel.Height.Set(SMHeight, 0);
            SMPanel.Left.Set(-4000f, 0); //outside of screen
            SMPanel.Top.Set(Main.screenHeight / 2 - SMHeight / 2, 0);
            SMPanel.BorderColor = Color.Red;
            SMPanel.BackgroundColor = Color.Transparent;
            SMPanel.OnMouseDown += (element, listener) =>
            {
                //Don't need to know where a click was for now, leave this here anyway
                //Vector2 SMPosition = new Vector2(SMPanel.Left.Pixels, SMPanel.Top.Pixels);
                //Vector2 clickPos = Vector2.Subtract(element.MousePosition, SMPosition);
            };

            CreateLayout();
            Instance.Append(SMPanel);
        }

        private static void CreateLayout()
        {
            //Popup UI Elements
            hoverItemSlotText = new UIText("");

            //Background
            UIImage SMBackground = new UIImage(GetTexture("LivingWorldMod/Textures/UIElements/ShrineMenu/Background"));
            SMBackground.Width.Set(0, 0);
            SMBackground.Height.Set(0f, 0);
            SMBackground.Left.Set(-12f, 0);
            SMBackground.Top.Set(-12f, 0);
            SMPanel.Append(SMBackground);

            //Cross to Close Menu
            UIImage closeMenuCross = new UIImage(GetTexture("LivingWorldMod/Textures/UIElements/ShrineMenu/CloseCross"));
            closeMenuCross.Width.Set(19f, 0);
            closeMenuCross.Height.Set(19f, 0);
            closeMenuCross.Left.Set(SMWidth - 35f, 0);
            closeMenuCross.Top.Set(-7f, 0);
            closeMenuCross.OnClick += (__, _) => { SMPanel.Remove(); showUI = false; };
            SMPanel.Append(closeMenuCross);

            //ItemSlot
            itemSlot = new CustomItemSlot();
            itemSlot.Width.Set(54f, 0);
            itemSlot.Height.Set(54f, 0);
            itemSlot.Left.Set(5f, 0);
            itemSlot.Top.Set(25f, 0);
            itemSlot.OnMouseOver += (__, _) => ItemSlotHoverTextCreation();
            itemSlot.OnMouseOut += (__, _) => hoverItemSlotText.Remove();
            itemSlot.OnItemEquipped += (_) => ItemSlotHoverTextCreation();
            itemSlot.OnItemRemoved += (_) => hoverItemSlotText.Remove();
            itemSlot.SetBackgroundTexture(GetTexture("LivingWorldMod/Textures/UIElements/ShrineMenu/ItemSlot"));
            SMPanel.Append(itemSlot);

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
            gift.OnMouseDown += (__, _) => { gift.Top.Set(gift.Top.Pixels + 1f, 0); GiftItem(); };
            gift.OnMouseUp += (__, _) => { gift.Top.Set(gift.Top.Pixels - 1f, 0); };
            SMPanel.Append(gift);

            //Reputation Text
            UIText repText = new UIText("Gifts: ", 0.8f);
            repText.Left.Set(8f, 0);
            repText.Top.Set(95f, 0);
            SMPanel.Append(repText);

            //Reputation ProgressBar
            Color Color = new Color(63, 113, 169);
            ShrineProgressBar repFrame = new ShrineProgressBar(true, 100, Color);
            repFrame.Width.Set(130f, 0);
            repFrame.Height.Set(16f, 0);
            repFrame.Left.Set(48f, 0);
            repFrame.Top.Set(95f, 0);
            repFrame.Activate();
            SMPanel.Append(repFrame);

            //Stage Text
            UIText stageText = new UIText("Stage: ", 0.8f);
            stageText.Left.Set(5f, 0);
            stageText.Top.Set(120f, 0);
            SMPanel.Append(stageText);

            //Stage ProgressBar
            ShrineProgressBar stageFrame = new ShrineProgressBar(false, 5, Color);
            stageFrame.Width.Set(130f, 0);
            stageFrame.Height.Set(16f, 0);
            stageFrame.Left.Set(48f, 0);
            stageFrame.Top.Set(120f, 0);
            stageFrame.Activate();
            SMPanel.Append(stageFrame);

            void ItemSlotHoverTextCreation()
            {
                hoverItemSlotText = new UIText("Amount Gifted: " + 2 + "\nLiking: Neutral");
                Instance.Append(hoverItemSlotText);
            }

            //ItemSlot Item gifted amount
            //Item gift modifier state(good, bad, neutral..)
            //Gift cooldown

            ////Gifted Amount Text
            //UIText giftedAmountText = new UIText("Amount: 2", 0.8f);
            //giftedAmountText.Left.Set(95f, 0);
            //giftedAmountText.Top.Set(25f, 0);
            //SMPanel.Append(giftedAmountText);

            ////Gift Value
            //UIText giftValue = new UIText("Value: Horrible", 0.8f); //Liking?
            //giftValue.Left.Set(95f, 0);
            //giftValue.Top.Set(40f, 0);
            //SMPanel.Append(giftValue);

            ////Gift Cooldown
            //UIText giftCooldown = new UIText("Cooldown: 5min", 0.8f);
            //giftCooldown.Left.Set(95f, 0);
            //giftCooldown.Top.Set(55f, 0);
            //SMPanel.Append(giftCooldown);
        }

        public static void GiftItem()
        {
            int itemType = itemSlot.Item.type;
            LWMWorld.AddGiftToProgress(shrineType, itemType);
            itemSlot.Item.TurnToAir();
        }

        public static void TileRightClicked(int i, int j, VillagerType shrineType)
        {
            ShrineUIState.shrineType = shrineType;

            showUI = !showUI;
            if (showUI) CreateShapesMenuPanel();
            else SMPanel.Remove();

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

        public override void Update(GameTime gameTime)
        {
            if (SMPanel != null && SMPanel.IsMouseHovering)
                Main.LocalPlayer.mouseInterface = true;

            Vector2 itemSlotWorldPos = itemSlotPos.ToWorldCoordinates() - Main.screenPosition;
            SMPanel?.Left.Set(itemSlotWorldPos.X, 0);
            SMPanel?.Top.Set(itemSlotWorldPos.Y, 0);

            if (itemSlot != null)
            {
                if (itemSlot.IsMouseHovering && !itemSlot.Item.IsAir)
                {
                    hoverItemSlotText?.Left.Set(Main.mouseX + 22f, 0);
                    hoverItemSlotText?.Top.Set(Main.mouseY - 22f, 0);
                }
            }
        }
    }
}
