using LivingWorldMod.Common.Players;
using LivingWorldMod.Content.StatusEffects.Debuffs.Consumables;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.Food;

/// <summary>
/// Upon first consumption, grants a permanent 5% move speed buff.
/// Inflicts the <see cref="SugarSuperfluity"/> debuff for a minute.
/// </summary>
public class EffervescentNugget : BaseItem {
    public override string Texture => "Terraria/Images/Item_" + ItemID.ChickenNugget;

    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        Item.CloneDefaults(ItemID.ChickenNugget);

        Item.buffType = ModContent.BuffType<SugarSuperfluity>();
        Item.buffTime = 60 * 60; // 1 minute
        Item.rare = ItemRarityID.LightPurple;

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

    public override void OnConsumeItem(Player player) {
        player.GetModPlayer<PermanentBuffPlayer>().effervescentNuggetBuff = true;
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        Texture2D drawTexture = TextureAssets.Item[ItemID.ChickenNugget].Value;
        int drawWidth = Item.width;
        int drawHeight = Item.height + 2;

        DrawingUtils.DrawTextureWithArmorShader(
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

        DrawingUtils.DrawTextureWithArmorShader(
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