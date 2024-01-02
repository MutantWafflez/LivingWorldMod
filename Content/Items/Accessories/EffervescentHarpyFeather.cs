using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;

namespace LivingWorldMod.Content.Items.Accessories;

/// <summary>
/// While equipped, grants the feather-fall potion effect, and increases
/// DR by 4% while airborne.
/// </summary>
public class EffervescentHarpyFeather : BaseItem {
    public override string Texture => "Terraria/Images/Item_" + ItemID.GiantHarpyFeather;

    public override void SetStaticDefaults() {
        ItemID.Sets.ShimmerTransformToItem[ItemID.GiantHarpyFeather] = Type;
    }

    public override void SetDefaults() {
        Item.CloneDefaults(ItemID.GiantHarpyFeather);
        Item.rare = ItemRarityID.LightPurple;
        Item.accessory = true;

        // For some reason there is a mis-match with the height and texture, so manually setting it here
        Item.height = 34;
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.slowFall = true;

        if (player.velocity.Y != 0f) {
            player.endurance += 0.04f;
        }
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        Texture2D drawTexture = TextureAssets.Item[ItemID.GiantHarpyFeather].Value;

        Utilities.DrawTextureWithArmorShader(
            spriteBatch,
            drawTexture,
            GameShaders.Armor.GetShaderIdFromItemId(ItemID.HallowBossDye),
            drawColor,
            new Rectangle((int)position.X, (int)position.Y, (int)(drawTexture.Width * scale), (int)(drawTexture.Height * scale)),
            new Rectangle(0, 0, drawTexture.Width, drawTexture.Height),
            origin,
            0f,
            Main.UIScaleMatrix
        );

        return false;
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
        Texture2D drawTexture = TextureAssets.Item[ItemID.GiantHarpyFeather].Value;

        Utilities.DrawTextureWithArmorShader(
            spriteBatch,
            drawTexture,
            GameShaders.Armor.GetShaderIdFromItemId(ItemID.HallowBossDye),
            lightColor,
            new Rectangle((int)(Item.position.X - Main.screenPosition.X), (int)(Item.position.Y - Main.screenPosition.Y), (int)(drawTexture.Width * scale), (int)(drawTexture.Height * scale)),
            new Rectangle(0, 0, drawTexture.Width, drawTexture.Height),
            default(Vector2),
            rotation,
            Main.GameViewMatrix.ZoomMatrix
        );

        return false;
    }
}