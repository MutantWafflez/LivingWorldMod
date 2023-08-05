using LivingWorldMod.Content.NPCs.Villagers;
using LivingWorldMod.Custom.Classes;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace LivingWorldMod.Content.UI.VillagerShop {
    /// <summary>
    /// UIElement class extension that handles and creates portraits for villagers in the shop UI, primarily.
    /// </summary>
    public class UIPortrait : UIElement {
        private string PortraitSpritePath => $"{LivingWorldMod.LWMSpritePath}UI/ShopUI/{_villager.VillagerType}/Portraits/";

        public VillagerPortraitExpression temporaryExpression;
        public float temporaryExpressionTimer;

        // TODO: Make array when more villages are added
        private readonly LayeredDrawObject _drawObject;
        private VillagerPortraitExpression _currentExpression;
        private Villager _villager;

        public UIPortrait(Villager villager) {
            _villager = villager;
            Width.Set(190f, 0f);
            Height.Set(190f, 0f);

            _drawObject = DrawingUtils.LoadDrawObject(new[] { 5, 5, 5, 15 }, new[] { "Base", "Outfit", "Hair", "Face" }, PortraitSpritePath);
        }

        public override void OnInitialize() {
            OnLeftClick += ClickedElement;
        }

        public override void Update(GameTime gameTime) {
            //Allows for temporary expressions, for whatever reason that it may be applicable
            if (--temporaryExpressionTimer <= 0f) {
                temporaryExpressionTimer = -1f;
            }

            base.Update(gameTime);
        }

        public void ReloadPortrait(Villager newVillager) {
            _villager = newVillager;

            switch (_villager.RelationshipStatus) {
                case <= VillagerRelationship.SevereDislike:
                    _currentExpression = VillagerPortraitExpression.Angered;
                    break;

                case > VillagerRelationship.SevereDislike and < VillagerRelationship.Love:
                    _currentExpression = VillagerPortraitExpression.Neutral;
                    break;

                case >= VillagerRelationship.Love:
                    _currentExpression = VillagerPortraitExpression.Happy;
                    break;
            }

            const int paleSkinFrame = 0;
            const int tanSkinFrame = 1;
            const int darkSkinFrame = 2;

            // The index for the tan skin color in terms of sprite file names
            // Here for readability
            const int tanSkinIndex = 2;

            LayeredDrawObject drawObject = _villager.drawObject;
            int faceSkinFrame = drawObject.drawIndices[HarpyVillager.BodyIndexID] switch {
                < tanSkinIndex => paleSkinFrame,
                tanSkinIndex => tanSkinFrame,
                > tanSkinIndex => darkSkinFrame
            };

            _drawObject.drawIndices = new[] {
                drawObject.drawIndices[HarpyVillager.BodyIndexID],
                drawObject.drawIndices[HarpyVillager.OutfitIndexID],
                drawObject.drawIndices[HarpyVillager.HairIndexID],
                drawObject.drawIndices[HarpyVillager.FaceIndexID] * 3 + faceSkinFrame
            };
        }


        protected override void DrawSelf(SpriteBatch spriteBatch) {
            int frameWidth = _drawObject.GetFrameWidth();
            int frameHeight = _drawObject.GetFrameHeight();

            Rectangle faceRect = new(0, (int)(temporaryExpressionTimer > 0 ? temporaryExpression : _currentExpression) * frameHeight, frameWidth, frameHeight);

            _drawObject.Draw(spriteBatch,
                GetDimensions().ToRectangle(),
                new Rectangle?[] { null, null, null, faceRect },
                Color.White,
                0f,
                default(Vector2),
                SpriteEffects.None,
                0f
            );
        }

        private void ClickedElement(UIMouseEvent evt, UIElement listeningElement) {
            //Little Easter Egg where clicking on the Portrait will make them smile for a half a second
            temporaryExpression = VillagerPortraitExpression.Happy;
            temporaryExpressionTimer = 30f;
            SoundEngine.PlaySound(SoundID.Item16);
        }
    }
}