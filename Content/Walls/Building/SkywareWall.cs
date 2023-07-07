using LivingWorldMod.Content.Items.Walls.Building;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Walls.Building {
    public class SkywareWall : BaseWall {
        public override Color? WallColorOnMap => Color.Cyan;

        public override void SetStaticDefaults() {
            Main.wallHouse[Type] = true;

            DustType = DustID.BlueMoss;

            ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = ModContent.ItemType<SkywareWallItem>();

            base.SetStaticDefaults();
        }
    }
}