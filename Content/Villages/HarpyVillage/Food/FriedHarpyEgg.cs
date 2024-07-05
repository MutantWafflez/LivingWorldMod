using LivingWorldMod.Content.Villages.HarpyVillage.Materials;
using LivingWorldMod.Globals.BaseTypes.Items;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace LivingWorldMod.Content.Villages.HarpyVillage.Food;

public class FriedHarpyEgg : BaseItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        Item.DefaultToFood(22, 22, BuffID.WellFed3, 14400);
        Item.rare = ItemRarityID.Blue;

        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));

        ItemID.Sets.IsFood[Type] = true;
        ItemID.Sets.FoodParticleColors[Type] = [Color.White, Color.LightGray, Color.Blue];
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<HarpyEgg>()
            .AddTile(TileID.CookingPots)
            .Register();
    }
}