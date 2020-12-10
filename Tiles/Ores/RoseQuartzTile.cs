using LivingWorldMod.Items.Placeable.Ores;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace LivingWorldMod.Tiles.Ores
{
    internal class RoseQuartzTile : ModTile
    {
        public override void SetDefaults()
        {
            TileID.Sets.Ore[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileValue[Type] = 410;
            Main.tileShine2[Type] = true;
            Main.tileShine[Type] = 900;
            Main.tileMergeDirt[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Rose Quartz");
            AddMapEntry(new Color(255, 150, 177), name);

            dustType = DustID.PinkCrystalShard;
            drop = ItemType<RoseQuartz>();
            soundType = SoundID.Tink;
            soundStyle = 1;
            mineResist = 1.5f;
            minPick = 30;
        }
    }
}