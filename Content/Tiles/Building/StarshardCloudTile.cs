using LivingWorldMod.Content.Items.Placeables.Building;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Tiles.Building {

    public class StarshardCloudTile : BaseTile {

        public override void SetStaticDefaults() {
            Main.tileSolid[Type] = true;
            Main.tileNoSunLight[Type] = true;
            Main.tileMergeDirt[Type] = false;

            MineResist = 1.34f;

            ItemDrop = ModContent.ItemType<StarshardCloudItem>();
            DustType = DustID.Cloud;

            AddMapEntry(Color.LightYellow);
        }

        public override bool HasWalkDust() => true;
    }
}