using LivingWorldMod.Content.Items.Walls.Fences;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Walls.Fences {
    public class SkywareFenceWall : BaseWall {
        public override Color? WallColorOnMap => Color.LightBlue;

        public override void SetStaticDefaults() {
            Main.wallHouse[Type] = true;
            WallID.Sets.AllowsWind[Type] = true;
            WallID.Sets.Transparent[Type] = true;

            DustType = DustID.t_LivingWood;

            ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = ModContent.ItemType<SkywareFenceItem>();

            base.SetStaticDefaults();
        }
    }
}