using LivingWorldMod.Content.Items.Placeables.Furniture.Harpy;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace LivingWorldMod.Content.Tiles.Furniture.Harpy {
    public class SkywareAnvilTile : BaseTile {
        public override Color? TileColorOnMap => Color.LightBlue;

        public override void SetStaticDefaults() {
            Main.tileSolid[Type] = false;
            Main.tileNoSunLight[Type] = false;
            Main.tileSolidTop[Type] = true;
            Main.tileWaterDeath[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileFrameImportant[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.addTile(Type);

            AdjTiles = new int[] { TileID.Anvils };
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 16, ModContent.ItemType<SkywareAnvilItem>());
        }
    }
}