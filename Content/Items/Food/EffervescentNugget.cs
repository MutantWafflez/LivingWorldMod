using LivingWorldMod.Content.StatusEffects.Debuffs.Consumables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.Food {
    public class EffervescentNugget : BaseItem {
        public override string Texture => "Terraria/Images/Item_" + ItemID.ChickenNugget;

        public override void SetStaticDefaults() {
            Item.ResearchUnlockCount = 1;
        }

        public override bool CanUseItem(Player player) => !player.HasBuff<SugarSuperfluity>();

        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.ChickenNugget);

            Item.buffType = ModContent.BuffType<SugarSuperfluity>();
            Item.buffTime = (int)(60 * 60 * 3.5f) + 2; // Two additional ticks to actually make the counter pin-point accurate (since we do the death check based on <= 2)
            Item.rare = ItemRarityID.Expert;

            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));

            ItemID.Sets.IsFood[Type] = true;
            ItemID.Sets.FoodParticleColors[Type] = new[] {
                Color.Red,
                Color.Orange,
                Color.Yellow,
                Color.Green,
                Color.Blue,
                Color.Indigo,
                Color.Violet
            };
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
            DrawNugget(
                spriteBatch,
                drawColor,
                new Rectangle((int)position.X, (int)position.Y, Item.width, Item.height),
                origin,
                0f,
                Main.UIScaleMatrix
            );

            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
            DrawNugget(
                spriteBatch,
                lightColor,
                new Rectangle((int)(Item.position.X - Main.screenPosition.X), (int)(Item.position.Y - Main.screenPosition.Y), Item.width, Item.height),
                default(Vector2),
                rotation,
                Main.GameViewMatrix.ZoomMatrix
            );

            return false;
        }

        private void DrawNugget(SpriteBatch spriteBatch, Color drawColor, Rectangle destinationRectangle, Vector2 origin, float rotation, Matrix drawMatrix) {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer, null, drawMatrix);

            Rectangle sourceRectangle = new(0, 0, Item.width, Item.height + 2);
            DrawData itemDrawData = new(TextureAssets.Item[ItemID.ChickenNugget].Value, destinationRectangle, sourceRectangle, drawColor, rotation, origin, SpriteEffects.None);

            GameShaders.Armor.Apply(GameShaders.Armor.GetShaderIdFromItemId(ItemID.HallowBossDye), null, itemDrawData);
            spriteBatch.Draw(TextureAssets.Item[ItemID.ChickenNugget].Value, destinationRectangle, sourceRectangle, drawColor, rotation, origin, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, Main.Rasterizer, null, drawMatrix);
        }
    }
}