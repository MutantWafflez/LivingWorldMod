using LivingWorldMod.Content.Items.Placeables.Building;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Tiles.Building {
    public class SkywareBlockTile : BaseTile {
        public override Color? TileColorOnMap => Color.LightBlue;

        public override void SetStaticDefaults() {
            Main.tileSolid[Type] = true;
            Main.tileNoSunLight[Type] = true;
            Main.tileMergeDirt[Type] = false;

            MineResist = 1.15f;

            ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = ModContent.ItemType<SkywareBlockItem>();
            DustType = DustID.BlueMoss;
        }
    }
}