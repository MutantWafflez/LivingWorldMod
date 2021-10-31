using LivingWorldMod.Common.Systems;
using LivingWorldMod.Content.NPCs.Villagers;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
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

        /// <summary>
        /// Whether or not this villager is currently selected.
        /// </summary>
        public bool IsSelected => myVillager.NPC.whoAmI == Main.instance.mouseNPCIndex;

        /// <summary>
        /// Whether or not this NPC is "allowed" to be housed, which is to say whether or not the
        /// village that the villager belongs to likes the player.
        /// </summary>
        public bool IsAllowed => myVillager.RelationshipStatus >= VillagerRelationship.Like;

        public UIHousingVillagerDisplay(Villager villager) {
            myVillager = villager;

            // Very arbitrary number, I know, but this is about the smallest we can get the
            // width/height while keeping 3 objects in a row in the grid
            Width.Set(50.284f, 0f);
            Height.Set(50.284f, 0f);
        }

        public override void Click(UIMouseEvent evt) {
            //Prevent any interaction if the villagers do not like the player
            if (!IsAllowed) {
                return;
            }

            //Change the mouse type properly
            if (IsSelected) {
                Main.instance.SetMouseNPC(-1, -1);
            }
            else {
                //Our IL edit in NPCHousingPatches.cs handles the drawing here.
                Main.instance.SetMouseNPC(myVillager.NPC.whoAmI, myVillager.Type);
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
        }

        public override void Update(GameTime gameTime) {
            if (!IsMouseHovering || myVillager is null) {
                return;
            }

            Main.LocalPlayer.mouseInterface = true;
            Main.instance.MouseText(IsAllowed ? myVillager.NPC.GivenName : LocalizationUtils.GetLWMTextValue("UI.VillagerHousing.VillagerTypeLocked", myVillager.VillagerType.ToString()));
        }

        protected override void DrawChildren(SpriteBatch spriteBatch) {
            base.DrawChildren(spriteBatch);

            spriteBatch.Draw(TextureAssets.InventoryBack11.Value,
                GetDimensions().ToRectangle(),
                null,
                !IsAllowed ? Color.Gray : Color.White,
                0f,
                default,
                SpriteEffects.None,
                0f);

            Texture2D bodyTexture = myVillager.bodyAssets[myVillager.bodySpriteType].Value;
            Texture2D headTexture = myVillager.headAssets[myVillager.headSpriteType].Value;

            float drawScale = 0.67f;
            int frameHeight = bodyTexture.Height / Main.npcFrameCount[myVillager.Type];

            // Make sure to draw from center!
            Vector2 drawPos = GetDimensions().Center();
            Rectangle textureDrawRegion = new Rectangle(0, 0, bodyTexture.Width, frameHeight);
            Vector2 drawOrigin = new Vector2(textureDrawRegion.Width / 2f, textureDrawRegion.Height / 2f * 1.25f);

            Color drawColor = IsSelected ? Color.Yellow : Color.White;

            spriteBatch.Draw(bodyTexture,
                drawPos,
                textureDrawRegion,
                drawColor,
                0f,
                drawOrigin,
                drawScale,
                SpriteEffects.None,
                0f);

            spriteBatch.Draw(headTexture,
                drawPos,
                textureDrawRegion,
                drawColor,
                0f,
                drawOrigin,
                drawScale,
                SpriteEffects.None,
                0f);

            //Draw lock icon if not "allowed" (player is not high enough rep)
            if (!IsAllowed) {
                int lockSize = 22;

                spriteBatch.Draw(TextureAssets.HbLock[0].Value,
                    GetDimensions().Center(),
                    new Rectangle(0, 0, lockSize, lockSize),
                    Color.White,
                    0f,
                    new Vector2(lockSize / 2f, lockSize / 2f),
                    1.25f,
                    SpriteEffects.None,
                    0f);
            }
        }
    }
}