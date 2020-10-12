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
			item.useAnimation = 15;
			item.useTime = 10;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.consumable = true;
			item.createTile = TileType<HarpyShrine_Tile>();
		}
	}
}
