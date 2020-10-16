using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace LivingWorldMod.UI.Elements
{
    class CustomItemSlot : UIElement
    {
        //MAKE SURE TO CALL ACTIVATE() WHEN USING THIS UIELEMENT

        internal Item Item;
        internal Func<Item, bool> ValidItemFunc;
        private readonly float _globalScale;
        private readonly float _opacity;
        private readonly Texture2D _texture;

        public CustomItemSlot(Texture2D texture = null, float scale = 1f, float opacity = 1f)
        {
            _texture = texture ?? ModContent.GetTexture("LivingWorldMod/Textures/UIElements/ShrineMenu/ItemSlot");
            _globalScale = scale;
            _opacity = opacity;
            Item = new Item();
            Item.SetDefaults(0);
        }

        public override void OnInitialize()
        {
            //This code can only run after the UIElement Width/Height has been set
            Width.Set(Width.Pixels * _globalScale, 0);
            Height.Set(Height.Pixels * _globalScale, 0);

            OnMouseDown += CustomItemSlot_OnMouseDown;
        }

        private void CustomItemSlot_OnMouseDown(UIMouseEvent evt, UIElement listeningElement)
        {
            //if mouseItem is stackable, do not allow swapping with itemSlot (unless stack == 1)
            //Maybe send itemSlot Item to the inventory if shift is pressed?
            if (Main.mouseItem.maxStack > 1 && Item.IsAir)
            {
                //stack > 0, items can't be reforged. No need to clone
                Item.SetDefaults(Main.mouseItem.type);
                Main.mouseItem.stack--;

                return;
            }

            if (Main.mouseItem.maxStack > 1 && !Item.IsAir)
            {
                if (Main.mouseItem.type == Item.type)
                {
                    Main.mouseItem.stack++;
                    Item.TurnToAir();

                    return;
                }

                //empty mouseItem still has a >1 maxStack
                if (Main.mouseItem.stack != 1 && !Main.mouseItem.IsAir) return;
            }

            //if mouseItem is empty or non stackable, allow swap with itemSlot
            //Maybe cloning more than needed?
            Item tempItem = Item.Clone();
            Item = Main.mouseItem.Clone();
            Main.mouseItem = tempItem.Clone();
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            //Background of ItemSlot
            Vector2 position = GetDimensions().ToRectangle().TopLeft();
            spriteBatch.Draw(_texture, position, _texture.Bounds, new Color(255, 255, 255, _opacity), 0f, Vector2.Zero, _globalScale, SpriteEffects.None, 0f);

            Texture2D itemInSlot = Main.itemTexture[Item.type];
            Rectangle rect = itemInSlot.Bounds;
            float scale = 1f;
            if (rect.Width > 32 || rect.Height > 32)
                scale = rect.Width > rect.Height ? 32f / rect.Width : 32f / rect.Height;
            //For items bigger than 32x32, Vector2 offset = new Vector2(itemHeight - 5). For items smaller than 32x32 Vector(27) works
            Vector2 origin = rect.Center() - new Vector2(itemInSlot.Height > 32 ? itemInSlot.Height - 5 : 27);

            //Drawing item's texture inside the ItemSlot
            spriteBatch.Draw(itemInSlot, position, rect, new Color(255, 255, 255, 1f), 0f, origin, scale * _globalScale, SpriteEffects.None, 0f);
        }
    }
}
