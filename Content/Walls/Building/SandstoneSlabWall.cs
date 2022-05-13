using LivingWorldMod.Content.Items.Walls.Building;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Walls.Building {
    public class SandstoneSlabWall : BaseWall {
        public override void SetStaticDefaults() {
            Main.wallHouse[Type] = true;
            Main.wallLargeFrames[Type] = Main.wallLargeFrames[WallID.StoneSlab];

            DustType = DustID.Sand;

            ItemDrop = ModContent.ItemType<SandstoneSlabWallItem>();

            AddMapEntry(new Color(108, 103, 72));
        }
    }
}