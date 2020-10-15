using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.ID;
using Terraria.DataStructures;

namespace LivingWorldMod.UI.Elements
{
    class CustomItemSlot : UIElement
    {
        internal Item Item;
        internal Func<Item, bool> ValidItemFunc;
        internal readonly float _scale;
        internal readonly float _opacity;
        internal readonly Texture2D _texture;

        public CustomItemSlot(Texture2D texture = null, float scale = 1f, float opacity = 1f)
        { 
            _texture = texture ?? ModContent.GetTexture("LivingWorldMod/Textures/UIElements/ShrineMenu/ItemSlot");
            _scale = scale;
            _opacity = opacity;
            Item = new Item();
            Item.SetDefaults(0);

            OnMouseDown += CustomItemSlot_OnMouseDown;
        }

        private void CustomItemSlot_OnMouseDown(UIMouseEvent evt, UIElement listeningElement)
        {
            if (Item.type > ItemID.None)
            {
                Main.mouseItem = Item.Clone();
                Item.TurnToAir();
            }
            else
            {
                Item = Main.mouseItem.Clone();
                Main.mouseItem.TurnToAir();
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            //Background of ItemSlot
            Vector2 position = GetDimensions().ToRectangle().TopLeft();
            spriteBatch.Draw(_texture, position, new Color(255, 255, 255, _opacity));

            Texture2D itemInSlot = Main.itemTexture[Item.type];
            Rectangle rect = itemInSlot.Bounds;
            float scale = 1f;
            if (rect.Width > 32 || rect.Height > 32)
                scale = rect.Width > rect.Height ? 32f / rect.Width : 32f / rect.Height;
            //For items bigger than 32x32, Vector2 offset = new Vector2(itemHeight - 5). For items smaller than 32x32 Vector(27) works
            Vector2 origin = rect.Center() - new Vector2(itemInSlot.Height > 32 ? itemInSlot.Height - 5 : 27);

            //Drawing item's texture inside the ItemSlot
            spriteBatch.Draw(itemInSlot, position, rect, new Color(255, 255, 255, 1f), 0f, origin, scale, SpriteEffects.None, 0f);
        }
    }
}
