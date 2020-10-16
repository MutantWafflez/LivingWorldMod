using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using Terraria.DataStructures;
using static Terraria.ModLoader.ModContent;
using LivingWorldMod.UI.Elements;
using LivingWorldMod.Tiles.Interactables;
using Terraria.GameContent.UI.Elements;
using LivingWorldMod.NPCs.Villagers;

namespace LivingWorldMod.UI
{
    class HarpyShrineUIState : UIState
    {
        public static HarpyShrineUIState Instance;
        public static bool showUI;
        public static Vector2 itemSlotPos;
        //private static VanillaItemSlotWrapper _itemSlot;

        public static UIPanel SMPanel;
        private static float _SMWidth;
        private static float _SMHeight;

        public override void OnInitialize()
        {
            Instance = this;
            showUI = false;
            itemSlotPos = Vector2.Zero;
        }

        public static void CreateShapesMenuPanel()
        {
            //Texture dimensions
            _SMWidth = 213f;
            _SMHeight = 167f;

            SMPanel = new UIPanel();
            SMPanel.Width.Set(_SMWidth, 0);
            SMPanel.Height.Set(_SMHeight, 0);
            SMPanel.Left.Set(-4000f, 0); //outside of screen
            SMPanel.Top.Set(Main.screenHeight / 2 - _SMHeight / 2, 0);
            SMPanel.BorderColor = Color.Red;
            SMPanel.BackgroundColor = Color.Transparent;
            SMPanel.OnMouseDown += (element, listener) =>
            {
                Vector2 SMPosition = new Vector2(SMPanel.Left.Pixels, SMPanel.Top.Pixels);
                Vector2 clickPos = Vector2.Subtract(element.MousePosition, SMPosition);
            };

            CreateLayout();
            Instance.Append(SMPanel);
        }

        private static void CreateLayout()
        {
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
            closeMenuCross.Left.Set(_SMWidth - 35f, 0);
            closeMenuCross.Top.Set(-7f, 0);
            closeMenuCross.OnClick += (__, _) => { SMPanel.Remove(); showUI = false; };
            SMPanel.Append(closeMenuCross);

            //ItemSlot
            CustomItemSlot _itemSlot = new CustomItemSlot();
            _itemSlot.Width.Set(54f, 0);
            _itemSlot.Height.Set(54f, 0);
            _itemSlot.Left.Set(5f, 0);
            _itemSlot.Top.Set(25f, 0);
            _itemSlot.Activate(); //Runs OnInitialize
            SMPanel.Append(_itemSlot);
        }

        public static void TileRightClicked(int i, int j)
        {
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

        public static void ChangeStage()
        {
            int reputation = LWMWorld.GetReputation(VillagerType.Harpy) + 101;
            int stage = (int)(reputation / 13.4f);
        }

        public override void Update(GameTime gameTime)
        {
            if (SMPanel != null && SMPanel.IsMouseHovering)
                Main.LocalPlayer.mouseInterface = true;

            Vector2 itemSlotWorldPos = itemSlotPos.ToWorldCoordinates() - Main.screenPosition;
            SMPanel?.Left.Set(itemSlotWorldPos.X, 0);
            SMPanel?.Top.Set(itemSlotWorldPos.Y, 0);
        }
    }
}
