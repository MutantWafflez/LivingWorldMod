using Terraria.ModLoader;
using Terraria.ID;
using Terraria;
using LivingWorldMod.Items.Placeable.Ores;

namespace LivingWorldMod.Items.Extra
{
    class RoseMirror : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Teleports you to the Harpy Village");
        }
        public override void SetDefaults()
        {
            item.width = 30;
            item.height = 32;
            item.useStyle = ItemUseStyleID.HoldingUp;
            item.useTurn = true;
            item.UseSound = SoundID.Item6;
            item.useTime = 90;
            item.useAnimation = 90;
            item.rare = ItemRarityID.Blue;
            item.value = Item.sellPrice(silver: 15);
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<RoseQuartz>(), 10);
            recipe.AddIngredient(ItemID.SunplateBlock, 8);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
        }
    }
}
