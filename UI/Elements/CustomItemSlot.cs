using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.UI;

namespace BuilderEssentials.UI.Elements
{
    public class CustomItemSlot : CustomUIElement
    {
        /// <summary>
        /// Create custom condition to filter which items can be placed in the ItemSlot.
        /// </summary>
        public Func<Item, bool> ValidItem;
        /// <summary>
        /// Get the current background texture.
        /// </summary>
        public Texture2D BackgroundTexture { get; private set; }
        /// <summary>
        /// Get the current ItemSlot Item.
        /// </summary>
        public Item Item { get; private set; }
        /// <summary>
        /// Get the current ItemSlot display state.
        /// </summary>
        public bool DisplayOnly { get; private set; }
        /// <summary>
        /// Called when an item is equipped in the ItemSlot.
        /// </summary>
        public event ElementEvent OnItemEquipped;
        /// <summary>
        /// Called when an item is removed from the ItemSlot.
        /// </summary>
        public event ElementEvent OnItemRemoved;

        //TODO: Implement mouse hover stats display like vanilla itemSlot?

        /// <summary></summary>
        /// <param name="scale">ItemSlot's drawing scale</param>
        /// <param name="itemType">ItemSlot's Item type</param>
        /// <param name="opacity">ItemSlots's opactiy level. (higher value, higher opacity)</param>
        /// <param name="displayOnly">Whether the ItemSlot is interactable or not. If true, the ItemSlot will not be interactable.</param>
        public CustomItemSlot(int itemType = 0, float scale = 1f, float opacity = 1f, bool displayOnly = false)
        {
            BackgroundTexture = Main.inventoryBack9Texture;

            Item = new Item();
            Item.SetDefaults(itemType);
            SetScale(scale);
            SetOpacity(opacity);
            SetDisplayOnly(displayOnly);
            //DisplayOnly = displayOnly;
        }

        private void SubscribeMouseDown() => OnMouseDown += CustomItemSlot_OnMouseDown;
        private void UnsubscribeMouseDown() => OnMouseDown -= CustomItemSlot_OnMouseDown;

        private void CustomItemSlot_OnMouseDown(UIMouseEvent evt, UIElement listeningElement)
        {
            if (ValidItem != null && !ValidItem(Main.mouseItem)) return;

            //if mouseItem is stackable, do not allow swapping with itemSlot (unless stack == 1)
            //Maybe send itemSlot Item to the inventory if shift is pressed?
            if (Main.mouseItem.maxStack > 1 && Item.IsAir)
            {
                //stack > 0, items can't be reforged. No need to clone
                Item.SetDefaults(Main.mouseItem.type);
                Main.mouseItem.stack--;

                OnItemRemovedEvent();
                OnItemEquippedEvent();
                return;
            }

            if (Main.mouseItem.maxStack > 1 && !Item.IsAir)
            {
                if (Main.mouseItem.type == Item.type)
                {
                    Main.mouseItem.stack++;
                    Item.TurnToAir();

                    OnItemRemovedEvent();
                    OnItemEquippedEvent();
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

            OnItemRemovedEvent();
            OnItemEquippedEvent();
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            //Background of ItemSlot
            Vector2 position = GetDimensions().ToRectangle().TopLeft();
            spriteBatch.Draw(BackgroundTexture, position, BackgroundTexture.Bounds, Color.White * Opacity, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);

            //Maybe I need to use vanilla's way to get the texture to render animated items?
            Texture2D itemInSlot = Main.itemTexture[Item.type];
            Rectangle rect = itemInSlot.Bounds;
            float itemScale = 1f;
            if (rect.Width > 32 || rect.Height > 32)
                itemScale = rect.Width > rect.Height ? 32f / rect.Width : 32f / rect.Height;

            //Centering the item's texture within the item slot. (would be a lot easier if vanilla textures were squaaaaaaaared)
            Vector2 origin;
            if (rect.Width > rect.Height)
                origin = rect.Center() - new Vector2(itemInSlot.Width > 32 ? itemInSlot.Width - 5 : 27);
            else origin = rect.Center() - new Vector2(itemInSlot.Height > 32 ? itemInSlot.Height - 5 : 27);

            origin -= new Vector2(itemInSlot.Width >= 40 ? -2 : 0, itemInSlot.Height >= 40 ? -2 : 0);
            origin -= new Vector2(itemInSlot.Width >= 64 ? -3 : 0, itemInSlot.Height >= 64 ? -3 : 0);

            //Drawing item's texture inside the ItemSlot
            spriteBatch.Draw(itemInSlot, position, rect, Color.White * Opacity, 0f, origin, itemScale * Scale, SpriteEffects.None, 0f);
        }

        public override void SetScale(float scale)
        {
            base.SetScale(scale);
            Width.Set(BackgroundTexture.Width * Scale, 0);
            Height.Set(BackgroundTexture.Height * Scale, 0);
        }

        //Custom events
        private void OnItemEquippedEvent()
        {
            if (!Item.IsAir)
                OnItemEquipped?.Invoke(this);
        }

        private void OnItemRemovedEvent()
        {
            if (Item.IsAir)
                OnItemRemoved?.Invoke(this);
        }

        //GET/SET METHODS

        /// <summary>
        /// Sets the ItemSlot background texture.
        /// </summary>
        /// <param name="texture">A XNA Framework Texture2D object.</param>
        public void SetBackgroundTexture(Texture2D texture)
        {
            BackgroundTexture = texture ?? Main.inventoryBack9Texture;
        }

        /// <summary>
        /// Sets the current Item inside the ItemSlot.
        /// </summary>
        /// <param name="itemType"></param>
        public void SetItem(int itemType)
        {
            if (itemType >= 0)
                Item.SetDefaults(itemType);
        }

        /// <summary>
        /// Sets the current Item inside the ItemSlot.
        /// </summary>
        /// <param name="item"></param>
        public void SetItem(Item item)
        {
            Item = item ?? new Item();
        }

        /// <summary>
        /// Sets the ItemSlot to display only or interactable.
        /// </summary>
        /// <param name="displayOnly">If true, the ItemSlot will not be interactable.</param>
        public void SetDisplayOnly(bool displayOnly)
        {
            //Always unsubscribe (to prevent event handler hooked twice)
            UnsubscribeMouseDown();
            if (!displayOnly)
                SubscribeMouseDown();

            DisplayOnly = displayOnly;
        }
    }
}
