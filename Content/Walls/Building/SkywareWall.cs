using LivingWorldMod.Content.Items.Walls.Building;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Walls.Building {
    public class SkywareWall : BaseWall {
        public override void SetStaticDefaults() {
            Main.wallHouse[Type] = true;

            DustType = DustID.BlueMoss;

            ItemDrop = ModContent.ItemType<SkywareWallItem>();

            AddMapEntry(Color.Blue);
        }
    }
}