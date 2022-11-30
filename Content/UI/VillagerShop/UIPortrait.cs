using System;
using System.Collections.Generic;
using LivingWorldMod.Content.NPCs.Villagers;
using LivingWorldMod.Custom.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace LivingWorldMod.Content.UI.VillagerShop {
    /// <summary>
    /// UIElement class extension that handles and creates portraits for villagers in the shop UI, primarily.
    /// </summary>
    public class UIPortrait : UIElement {
        /// <summary>
        /// Small enum that defines what expression fits with what sprite on the portraits of the shop
        /// UI. Case-sensitive with the sprites file names, very important!
        /// </summary>
        public enum VillagerPortraitExpression {
            Neutral,
            Angered,
            Happy
        }

        public UIImage portraitBase;
        public UIImage portraitClothing;
        public UIImage portraitHead;
        public UIImage portraitExpression;

        public Dictionary<VillagerPortraitExpression, Asset<Texture2D>> expressionDictionary;

        public VillagerPortraitExpression currentExpression;
        public VillagerPortraitExpression temporaryExpression;
        public float temporaryExpressionTimer;

        private Villager _villager;

        private string PortraitSpritePath => $"{LivingWorldMod.LWMSpritePath}UI/ShopUI/{_villager.VillagerType}/Portraits/";

        public UIPortrait(Villager villager) {
            _villager = villager;
            Width.Set(190f, 0f);
            Height.Set(190f, 0f);
        }

        public override void OnInitialize() {
            portraitBase = new UIImage(ModContent.Request<Texture2D>(PortraitSpritePath + "Base", AssetRequestMode.ImmediateLoad));
            Append(portraitBase);

            portraitClothing = new UIImage(ModContent.Request<Texture2D>(PortraitSpritePath + "Body1", AssetRequestMode.ImmediateLoad));
            Append(portraitClothing);

            portraitHead = new UIImage(ModContent.Request<Texture2D>(PortraitSpritePath + "Head1", AssetRequestMode.ImmediateLoad));
            Append(portraitHead);

            PopulateExpressionDictionary();

            currentExpression = VillagerPortraitExpression.Neutral;
            portraitExpression = new UIImage(expressionDictionary[currentExpression].Value);
            Append(portraitExpression);

            OnClick += ClickedElement;
        }

        public override void Update(GameTime gameTime) {
            //Allows for temporary expressions, for whatever reason that it may be applicable
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

        public void ChangePortraitType(Villager newVillager) {
            _villager = newVillager;
            ReloadPortrait();
        }

        public void ReloadPortrait() {
            PopulateExpressionDictionary();

            switch (_villager.RelationshipStatus) {
                case <= VillagerRelationship.SevereDislike:
                    currentExpression = VillagerPortraitExpression.Angered;
                    break;

                case > VillagerRelationship.SevereDislike and < VillagerRelationship.Love:
                    currentExpression = VillagerPortraitExpression.Neutral;
                    break;

                case >= VillagerRelationship.Love:
                    currentExpression = VillagerPortraitExpression.Happy;
                    break;
            }

            portraitBase.SetImage(ModContent.Request<Texture2D>(PortraitSpritePath + "Base", AssetRequestMode.ImmediateLoad));

            portraitClothing.SetImage(ModContent.Request<Texture2D>(PortraitSpritePath + $"Body{_villager.bodySpriteType}", AssetRequestMode.ImmediateLoad));

            portraitHead.SetImage(ModContent.Request<Texture2D>(PortraitSpritePath + $"Head{_villager.headSpriteType}", AssetRequestMode.ImmediateLoad));

            portraitExpression.SetImage(expressionDictionary[currentExpression]);
        }

        private void PopulateExpressionDictionary() {
            expressionDictionary = new Dictionary<VillagerPortraitExpression, Asset<Texture2D>>();

            foreach (VillagerPortraitExpression expression in Enum.GetValues(typeof(VillagerPortraitExpression))) {
                expressionDictionary.Add(expression, ModContent.Request<Texture2D>(PortraitSpritePath + $"Expression{expression}", AssetRequestMode.ImmediateLoad));
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