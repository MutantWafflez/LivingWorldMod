//Adapted from https://github.com/tModLoader/tModLoader/blob/master/ExampleMod/UI/ExampleResourceBar.cs

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.ModLoader;
using LivingWorldMod.NPCs.Villagers;

namespace LivingWorldMod.UI.Elements
{
    class ShrineProgressBar : UIElement
    {
        private UIText text;
        private UIImage barFrame;
        private Color colorA;
        private Color colorB;
        private readonly int maxProgress;
        private readonly bool giftMode;

        public ShrineProgressBar(bool giftMode, int maxProgress, Color colorA, Color colorB = default)
        {
            this.giftMode = giftMode;
            this.maxProgress = maxProgress;
            this.colorA = colorA;
            this.colorB = colorB == default ? colorA : colorB;
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
            VillagerType shrineType = ShrineUIState.shrineType;
            //int reputation = LWMWorld.GetReputation(shrineType);
            float quotient;
            if (giftMode)
            {
                int giftProgress = LWMWorld.GetGiftProgress(shrineType);

                text.SetText($"{giftProgress} / {maxProgress}");
                quotient = giftProgress / (float)maxProgress;
            }
            else //stageMode
            {
                int shrineStage = LWMWorld.GetShrineStage(shrineType);

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
                Color color = Color.Lerp(colorA, colorB, percent);
                spriteBatch.Draw(Main.magicPixel, new Rectangle(left + i, hitbox.Y, 1, hitbox.Height), color);
            }
        }
    }
}
