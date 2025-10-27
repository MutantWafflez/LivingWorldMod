using LivingWorldMod.Globals.BaseTypes.Items;
using LivingWorldMod.Globals.BaseTypes.StatusEffects;
using LivingWorldMod.Globals.Players;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;

namespace LivingWorldMod.Content.Villages.HarpyVillage.Food;

/// <summary>
///     Upon first consumption, grants a permanent 5% move speed buff.
///     Inflicts the <see cref="SugarSuperfluity" /> debuff for a minute.
/// </summary>
public class EffervescentNugget : BaseItem {
    public override string Texture => "Terraria/Images/Item_" + ItemID.ChickenNugget;

    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        Item.CloneDefaults(ItemID.ChickenNugget);

        Item.buffType = ModContent.BuffType<SugarSuperfluity>();
        Item.buffTime = LWMUtils.RealLifeMinute;
        Item.rare = ItemRarityID.LightPurple;

        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));

        ItemID.Sets.IsFood[Type] = true;
        ItemID.Sets.FoodParticleColors[Type] = [Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Violet];
    }

    public override void OnConsumeItem(Player player) {
        player.GetModPlayer<PermanentBuffPlayer>().effervescentNuggetBuff = true;
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        Texture2D drawTexture = TextureAssets.Item[ItemID.ChickenNugget].Value;
        int drawWidth = Item.width;
        int drawHeight = Item.height + 2;

        LWMUtils.DrawTextureWithArmorShader(
            spriteBatch,
            drawTexture,
            GameShaders.Armor.GetShaderIdFromItemId(ItemID.HallowBossDye),
            drawColor,
            new Rectangle((int)position.X, (int)position.Y, (int)(drawWidth * scale), (int)(drawHeight * scale)),
            new Rectangle(0, 0, drawWidth, drawHeight),
            origin,
            0f,
            Main.UIScaleMatrix
        );

        return false;
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
        Texture2D drawTexture = TextureAssets.Item[ItemID.ChickenNugget].Value;
        int drawWidth = Item.width;
        int drawHeight = Item.height + 2;

        LWMUtils.DrawTextureWithArmorShader(
            spriteBatch,
            drawTexture,
            GameShaders.Armor.GetShaderIdFromItemId(ItemID.HallowBossDye),
            lightColor,
            new Rectangle((int)(Item.position.X - Main.screenPosition.X), (int)(Item.position.Y - Main.screenPosition.Y), (int)(drawWidth * scale), (int)(drawHeight * scale)),
            new Rectangle(0, 0, drawWidth, drawHeight),
            default(Vector2),
            rotation,
            Main.GameViewMatrix.ZoomMatrix
        );

        return false;
    }
}

/// <summary>
///     Granted by the <see cref="EffervescentNugget" /> food item.
///     Causes lighting to go rainbow for the duration (purely visual).
/// </summary>
public class SugarSuperfluity : BaseStatusEffect {
    public override string Texture => "Terraria/Images/Buff_" + BuffID.SugarRush;

    public override void SetStaticDefaults() {
        Main.debuff[Type] = true;
        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
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