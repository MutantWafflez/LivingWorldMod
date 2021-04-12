using LivingWorldMod.Content.Buffs.PotionBuffs;
using LivingWorldMod.Content.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.Potions
{
    public class CharmPotion : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Temporarily raises your reputation with the Harpy Village" +
                "\nDoes not stack with other players");
        }

        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.IronskinPotion);
            item.value = Item.sellPrice(silver: 20);
            item.buffType = ModContent.BuffType<Charmed>();
            item.buffTime = 60 * 60 * 8; //8 minutes
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.BottledWater);
            recipe.AddIngredient(ModContent.ItemType<SkyBudItem>());
            recipe.alchemy = true;
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}