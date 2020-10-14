using LivingWorldMod.Tiles.Interactables;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace LivingWorldMod.Items.Debug
{
    class HarpyShrine : DebugItem
    {
		public override void SetDefaults()
		{
			item.maxStack = 99;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 10;
			item.useTime = 10;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.consumable = false;
			item.createTile = TileType<HarpyShrineTile>();
		}
	}
}
