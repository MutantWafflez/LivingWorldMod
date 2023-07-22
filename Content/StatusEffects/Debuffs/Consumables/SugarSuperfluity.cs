using LivingWorldMod.Content.Items.Food;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace LivingWorldMod.Content.StatusEffects.Debuffs.Consumables {
    /// <summary>
    /// Granted by the <see cref="EffervescentNugget"/> food item. Triples
    /// all movement stats. Once the debuff ends, kills the player.
    /// </summary>
    public class SugarSuperfluity : BaseStatusEffect {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.SugarRush;

        public override void SetStaticDefaults() {
            Main.debuff[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex) {
            player.moveSpeed *= 3;
            player.wingAccRunSpeed *= 3;
            player.wingRunAccelerationMult *= 3;

            if (player.buffTime[buffIndex] <= 2f) {
                player.KillMe(PlayerDeathReason.ByCustomReason(LocalizationUtils.GetLWMTextValue($"PlayerDeathReason.{Name}", player.name)), 9999, 0);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams) {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer, null, Main.UIScaleMatrix);

            DrawData buffDrawData = new(drawParams.Texture, drawParams.MouseRectangle, drawParams.SourceRectangle, drawParams.DrawColor, 0f, default(Vector2), SpriteEffects.None);
            GameShaders.Armor.Apply(GameShaders.Armor.GetShaderIdFromItemId(ItemID.HallowBossDye), null, buffDrawData);

            return base.PreDraw(spriteBatch, buffIndex, ref drawParams);
        }

        public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams) {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
        }
    }
}