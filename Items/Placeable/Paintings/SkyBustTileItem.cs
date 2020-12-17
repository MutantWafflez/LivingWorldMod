using LivingWorldMod.Tiles.Furniture.Paintings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Items.Placeable.Paintings
{
    
    // FIXME the item slot behavior is currently only implemented in this class. It was easier, and I was more looking for proof of concept rather than a final implementation
    // ideally this should be implemented into item slots, rather than the items themselves.
    public class SkyBustTileItem : ModItem
    {
        /// <summary>
        /// If true, an X is drawn over the sprite in PostDrawInInventory.
        /// This is set in a PlayerHook after buying the last of an item.
        /// </summary>
        public bool isOutOfStock = false;
        
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sky Bust");
            Tooltip.SetDefault("'R. Oaken'");
        }

        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.TheMerchant);
            item.value = Item.buyPrice(copper: 1);
            item.createTile = ModContent.TileType<SkyBustTile>();
            item.placeStyle = 0;
            // set to false normally; is set to true in villager shop setup
            // true modifies the tooltip to reflect single item pricing
            item.buyOnce = false;
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor,
            Vector2 origin, float scale)
        {
            if(isOutOfStock)
                spriteBatch.Draw(ModContent.GetTexture("Terraria/CoolDown"), position, frame, drawColor, 0, origin, scale, SpriteEffects.None, 0);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // buyOnce is only true in the shop window, so we don't want to modify the tooltips outside of that
            if (!item.buyOnce) return;
            
            TooltipLine line = tooltips.FirstOrDefault(l => l.Name == "Price");
            if (line != null)
            {
                // this can be generalized later with the coded price, but for testing this works
                line.text = "Buy Price: 1 Copper";
            }
        }
    }
}