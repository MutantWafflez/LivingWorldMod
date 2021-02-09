using LivingWorldMod.NPCs.Villagers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace LivingWorldMod.UI.Elements
{
    public class CustomProgressBar : CustomUIElement
    {
        private Texture2D _backgroundTexture;
        private Texture2D _progressTexture;
        private UIElement _frame;
        private readonly int _maxProgress;
        private readonly bool _giftMode;

        public CustomProgressBar(Texture2D backgroundTexture, Texture2D progressTexture, int maxProgress, bool giftMode)
        {
            _backgroundTexture = backgroundTexture;
            _progressTexture = progressTexture;
            _maxProgress = maxProgress;
            _giftMode = giftMode;
        }

        public override void OnInitialize()
        {
            _frame = new UIElement();
            _frame.Width.Set(Width.Pixels, 0);
            _frame.Height.Set(Height.Pixels, 0);
            
            Append(_frame);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            VillagerType shrineType = ShrineUIPanel.shrineType;

            float quotient;
            if (_giftMode)
            {
                int giftProgress = LWMWorld.GetGiftProgress(shrineType);
                quotient = giftProgress / (float) _maxProgress;
            }
            else //stageMode
            {
                int shrineStage = LWMWorld.GetShrineStage(shrineType);
                quotient = shrineStage / (float) _maxProgress;
            }

            quotient = Utils.Clamp(quotient, 0f, 1f);
            Rectangle frameRect = _frame.GetInnerDimensions().ToRectangle();
            frameRect.Height += 8;
            
            spriteBatch.Draw(_backgroundTexture, frameRect, new Rectangle(0, 0, 
                (int)(frameRect.Width), frameRect.Height), Color.White);

            frameRect.Width = (int)(_frame.Width.Pixels * quotient);
            spriteBatch.Draw(_progressTexture, frameRect, new Rectangle(0, 0, 
                (int)(frameRect.Width), frameRect.Height), Color.White);
        }
    }
}