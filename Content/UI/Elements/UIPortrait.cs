using System;
using System.Collections.Generic;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace LivingWorldMod.Content.UI.Elements {

    /// <summary>
    /// UIElement class extension that handles and creates portraits for villagers in the shop UI, primarily.
    /// </summary>
    public class UIPortrait : UIElement {
        public UIImage portraitBase;
        public UIImage portraitClothing;
        public UIImage portraitHair;
        public UIImage portraitExpression;

        public Dictionary<VillagerPortraitExpression, Asset<Texture2D>> expressionDictionary;

        public VillagerPortraitExpression currentExpression;
        public VillagerPortraitExpression temporaryExpression;
        public float temporaryExpressionTimer;

        private VillagerType villagerType;

        private string PortraitSpritePath => $"{IOUtilities.LWMSpritePath}/UI/ShopUI/{villagerType}/Portraits/";

        public UIPortrait(VillagerType villagerType) {
            this.villagerType = villagerType;
            Width.Set(190f, 0f);
            Height.Set(190f, 0f);
        }

        public override void OnInitialize() {
            portraitBase = new UIImage(ModContent.Request<Texture2D>(PortraitSpritePath + "Base"));
            Append(portraitBase);

            portraitClothing = new UIImage(ModContent.Request<Texture2D>(PortraitSpritePath + "Body1"));
            Append(portraitClothing);

            portraitHair = new UIImage(ModContent.Request<Texture2D>(PortraitSpritePath + "Hair1"));
            Append(portraitHair);

            PopulateExpressionDictionary();

            currentExpression = VillagerPortraitExpression.Neutral;
            portraitExpression = new UIImage(expressionDictionary[currentExpression].Value);
            Append(portraitExpression);

            OnClick += ClickedElement;
        }

        public void ChangePortraitType(VillagerType newType) {
            villagerType = newType;
            ReloadPortrait();
        }

        public void ReloadPortrait() {
            PopulateExpressionDictionary();

            portraitBase.SetImage(ModContent.Request<Texture2D>(PortraitSpritePath + "Base"));

            portraitClothing.SetImage(ModContent.Request<Texture2D>(PortraitSpritePath + "Body1"));

            portraitHair.SetImage(ModContent.Request<Texture2D>(PortraitSpritePath + "Hair1"));

            portraitExpression.SetImage(expressionDictionary[currentExpression]);
        }

        public override void Update(GameTime gameTime) {
            //Allows for temporary expressions, for whatever reason that it may need
            if (temporaryExpression != currentExpression && temporaryExpressionTimer > 0f) {
                temporaryExpressionTimer--;
                portraitExpression.SetImage(expressionDictionary[temporaryExpression]);
            }
            else if (temporaryExpression != currentExpression && temporaryExpressionTimer == 0f) {
                temporaryExpressionTimer = -1f;
                portraitExpression.SetImage(expressionDictionary[currentExpression]);
            }
            else {
                temporaryExpression = currentExpression;
                temporaryExpressionTimer = -1f;
            }

            base.Update(gameTime);
        }

        private void PopulateExpressionDictionary() {
            expressionDictionary = new Dictionary<VillagerPortraitExpression, Asset<Texture2D>>();

            foreach (VillagerPortraitExpression expression in Enum.GetValues(typeof(VillagerPortraitExpression))) {
                expressionDictionary.Add(expression, ModContent.Request<Texture2D>(PortraitSpritePath + $"Expression{expression}"));
            }
        }

        private void ClickedElement(UIMouseEvent evt, UIElement listeningElement) {
            //Little Easter Egg where clicking on the Portrait will make them smile for a half a second
            temporaryExpression = VillagerPortraitExpression.Happy;
            temporaryExpressionTimer = 30f;
            SoundEngine.PlaySound(SoundID.Item16);
        }
    }
}