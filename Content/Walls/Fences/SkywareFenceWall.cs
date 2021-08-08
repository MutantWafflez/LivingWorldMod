using LivingWorldMod.Content.Items.Walls.Fences;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Walls.Fences {

    public class SkywareFenceWall : BaseWall {

        public override void SetStaticDefaults() {
            Main.wallHouse[Type] = true;
            WallID.Sets.AllowsWind[Type] = true;
            WallID.Sets.Transparent[Type] = true;

            DustType = DustID.t_LivingWood;

            ItemDrop = ModContent.ItemType<SkywareFenceItem>();

            AddMapEntry(Color.LightBlue);
        }
    }
}