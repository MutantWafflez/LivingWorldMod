using LivingWorldMod.Content.Items.Walls.Building;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Walls.Building {
    public class SandstoneSlabWall : BaseWall {
        public override Color? WallColorOnMap => new Color(108, 103, 72);

        public override void SetStaticDefaults() {
            Main.wallHouse[Type] = true;
            Main.wallLargeFrames[Type] = Main.wallLargeFrames[WallID.StoneSlab];

            DustType = DustID.Sand;

            ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = ModContent.ItemType<SandstoneSlabWallItem>();

            base.SetStaticDefaults();
        }
    }
}