using LivingWorldMod.Content.Items.Placeable.Ores;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace LivingWorldMod.Content.Tiles.WorldGen {

    public class RoseQuartzCluster : ModTile {

        public override void SetDefaults() {
            TileID.Sets.Ore[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileValue[Type] = 410;
            Main.tileShine2[Type] = true;
            Main.tileShine[Type] = 900;
            Main.tileFrameImportant[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.addTile(Type);

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Rose Quartz");
            AddMapEntry(new Color(255, 150, 177), name);

            dustType = DustID.PinkCrystalShard;
            soundType = SoundID.Tink;
            soundStyle = 1;
            mineResist = 1.5f;
            minPick = 30;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) {
            Item.NewItem(new Vector2(i * 16, j * 16), ModContent.ItemType<RoseQuartz>(), Main.rand.Next(1, 5));
        }
    }
}