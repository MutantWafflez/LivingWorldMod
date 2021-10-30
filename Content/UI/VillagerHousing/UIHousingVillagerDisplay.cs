using LivingWorldMod.Content.NPCs.Villagers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace LivingWorldMod.Content.UI.VillagerHousing {

    /// <summary>
    /// Element that shows a specific villager in the housing menu. An instance of this is created
    /// per villager that exists in the world of a given type when the player is in the housing menu.
    /// </summary>
    public class UIHousingVillagerDisplay : UIElement {

        /// <summary>
        /// The villager instance that this element is displaying.
        /// </summary>
        public Villager myVillager;

        public UIImage backingPanel;

        public UIHousingVillagerDisplay(Villager villager) {
            myVillager = villager;

            Asset<Texture2D> backingAsset = TextureAssets.InventoryBack11;
            backingPanel = new UIImage(backingAsset) {
                ImageScale = 0.967f
            };

            Width.Set(backingAsset.Width() * backingPanel.ImageScale, 0f);
            Height.Set(backingAsset.Height() * backingPanel.ImageScale, 0f);

            Append(backingPanel);
        }

        public override void Update(GameTime gameTime) {
            if (!IsMouseHovering || myVillager is null) {
                return;
            }

            Main.LocalPlayer.mouseInterface = true;
            Main.instance.MouseText(myVillager.NPC.GivenName);
        }

        protected override void DrawChildren(SpriteBatch spriteBatch) {
            base.DrawChildren(spriteBatch);

            Texture2D bodyTexture = myVillager.bodyAssets[myVillager.bodySpriteType].Value;
            Texture2D headTexture = myVillager.headAssets[myVillager.headSpriteType].Value;

            float drawScale = 0.67f;
            int frameHeight = bodyTexture.Height / Main.npcFrameCount[myVillager.Type];

            // Make sure to draw from center!
            Vector2 drawPos = GetDimensions().Center();
            Rectangle textureDrawRegion = new Rectangle(0, 0, bodyTexture.Width, frameHeight);
            Vector2 drawOrigin = new Vector2(textureDrawRegion.Width / 2f, textureDrawRegion.Height / 2f * 1.25f);

            spriteBatch.Draw(bodyTexture,
                drawPos,
                textureDrawRegion,
                Color.White,
                0f,
                drawOrigin,
                drawScale,
                SpriteEffects.None,
                0f);

            spriteBatch.Draw(headTexture,
                drawPos,
                textureDrawRegion,
                Color.White,
                0f,
                drawOrigin,
                drawScale,
                SpriteEffects.None,
                0f);
        }
    }
}