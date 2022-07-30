using LivingWorldMod.Content.Items.RespawnItems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace LivingWorldMod.Content.Items.Food {
    public class FriedHarpyEgg : BaseItem {
        public override void SetStaticDefaults() {
            SacrificeTotal = 1;
        }

        public override void SetDefaults() {
            Item.DefaultToFood(22, 22, BuffID.WellFed3, 14400);
            Item.rare = ItemRarityID.Blue;

            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));

            ItemID.Sets.IsFood[Type] = true;
            ItemID.Sets.FoodParticleColors[Type] = new Color[3] {
                Color.White,
                Color.LightGray,
                Color.Blue
            };
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient<HarpyEgg>()
                .AddTile(TileID.CookingPots)
                .Register();
        }
    }
}