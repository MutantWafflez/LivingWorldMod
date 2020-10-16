//Adapted from https://github.com/tModLoader/tModLoader/blob/master/ExampleMod/UI/ExampleResourceBar.cs

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.ModLoader;
namespace LivingWorldMod.UI.Elements
{
    class ShrineProgressBar : UIElement
    {
        private UIText text;
        private UIImage barFrame;
        private Color positiveC1;
        private Color negativeC1;
        private Color positiveC2;
        private Color negativeC2;
        private readonly int maxProgress;
        private readonly bool repMode;

        public ShrineProgressBar(bool repMode, int maxProgress, Color positiveC1, Color negativeC1, Color positiveC2 = default, Color negativeC2 = default)
        {
            this.repMode = repMode;
            this.maxProgress = maxProgress;
            this.positiveC1 = positiveC1;
            this.negativeC1 = negativeC1;
            this.positiveC2 = positiveC2 == default ? positiveC1 : positiveC2;
            this.negativeC2 = negativeC2 == default ? negativeC1 : negativeC2;
        }

        public override void OnInitialize()
        {
            UIElement area = new UIElement();
            area.Width.Set(130f, 0);
            area.Height.Set(16f, 0);

            barFrame = new UIImage(ModContent.GetTexture("LivingWorldMod/Textures/UIElements/ShrineMenu/Frame"));
            barFrame.Width.Set(130f, 0);
            barFrame.Height.Set(16f, 0);

            text = new UIText("0/0", 0.8f);
            text.Width.Set(130f, 0);
            text.Height.Set(16f, 0);
            text.Top.Set(1f, 0);
            text.Left.Set(0f, 0);

            area.Append(text);
            area.Append(barFrame);
            Append(area);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            int shrineType = (int)ShrineUIState.shrineType;
            int shrineStage = ShrineUIState.shrineStage[shrineType];
            int reputation = LWMWorld.villageReputation[shrineType];
            float quotient;
            if (repMode)
            {
                text.SetText($"{reputation} / {maxProgress}");
                quotient = Math.Abs(reputation) / (float)maxProgress;
            }
            else //stageMode
            {
                text.SetText($"{shrineStage} / {maxProgress}");
                quotient = shrineStage / (float)maxProgress;
            }

            quotient = Terraria.Utils.Clamp(quotient, 0f, 1f);
            Rectangle hitbox = barFrame.GetInnerDimensions().ToRectangle();

            //Magic numbers to keep everything aligned
            hitbox.X += 2;
            hitbox.Width -= 4;

            int left = hitbox.Left;
            int right = hitbox.Right;
            int steps = (int)((right - left) * quotient);
            for (int i = 0; i < steps; i += 1)
            {
                //float percent = (float)i / steps; // Alternate Gradient Approach
                float percent = (float)i / (right - left);
                Color color = reputation > 0 ? Color.Lerp(positiveC1, positiveC2, percent) : Color.Lerp(negativeC1, negativeC2, percent);
                spriteBatch.Draw(Main.magicPixel, new Rectangle(left + i, hitbox.Y, 1, hitbox.Height), color);
            }
        }
    }
}
