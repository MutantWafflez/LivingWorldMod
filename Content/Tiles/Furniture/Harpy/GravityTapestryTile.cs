using LivingWorldMod.Content.Items.Placeables.Furniture.Harpy;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace LivingWorldMod.Content.Tiles.Furniture.Harpy {

    public class GravityTapestryTile : BaseTile {

        public override void SetStaticDefaults() {
            Main.tileSolid[Type] = false;
            Main.tileNoSunLight[Type] = false;
            Main.tileNoAttach[Type] = true;
            Main.tileWaterDeath[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileFrameImportant[Type] = true;
            //TODO: Fix wind sway
            //TileID.Sets.SwaysInWindBasic[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top);
            TileObjectData.newTile.Width = 3;
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.Origin = new Point16(1, 0);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16 };
            TileObjectData.addTile(Type);

            ModTranslation mapName = CreateMapEntryName();
            mapName.SetDefault("Tapestry");

            AddMapEntry(Color.LightBlue);
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) {
            Item.NewItem(i * 16, j * 16, 48, 48, ModContent.ItemType<GravityTapestryItem>());
        }
    }
}