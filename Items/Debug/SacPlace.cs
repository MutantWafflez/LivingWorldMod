using LivingWorldMod.Tiles.WorldGen;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Items.Debug
{
    public class SacPlace : DebugItem
    {
        public override string Texture => "Terraria/Item_" + ItemID.SpiderEgg;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sac Place");
        }

        public override void SetDefaults()
        {
            item.useTime = 15;
            item.useAnimation = 15;
            item.createTile = ModContent.TileType<SpiderSacTile>();
            item.useStyle = ItemUseStyleID.SwingThrow;
        }
    }
}