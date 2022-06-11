using LivingWorldMod.Content.Items.Miscellaneous;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.ID;

namespace LivingWorldMod.Content.Items.Food {
    public class FriedHarpyEgg : BaseItem {
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.FriedEgg);

            Item.buffType = BuffID.WellFed3;
            Item.buffTime = 14400; //4 minutes
            Item.rare = ItemRarityID.Blue;

            ItemID.Sets.IsFood[Type] = true;
            ItemID.Sets.FoodParticleColors[Type] = ItemID.Sets.FoodParticleColors[ItemID.FriedEgg];
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
            //Food item drawing is a bit wack
            Rectangle rectanglizedPosition = new Rectangle((int)position.X - 6, (int)position.Y + 8, 28, 16);
            Rectangle newFrame = new Rectangle(0, 0, 28, 16);

            spriteBatch.Draw(TextureAssets.Item[Type].Value, rectanglizedPosition, newFrame, drawColor, 0f, Vector2.Zero, SpriteEffects.None, scale);

            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
            Rectangle rectanglizedPosition = new Rectangle((int)Item.position.X, (int)Item.position.Y, 28, 16);
            Rectangle frame = new Rectangle(0, 0, 28, 16);

            spriteBatch.Draw(TextureAssets.Item[Type].Value, rectanglizedPosition, frame, alphaColor, rotation, Vector2.Zero, SpriteEffects.None, scale);

            return false;
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient<HarpyEgg>()
                .AddTile(TileID.CookingPots)
                .Register();
        }
    }
}