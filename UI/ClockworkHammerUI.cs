using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.UI;

namespace LivingWorldMod.UI
{
    internal class ClockworkHammerUI: UIState
    {
        private const int BUTTON_COUNT = 8;

        private WheelButton[] buttons;

        public override void OnInitialize()
        {
            
        }
    }

    internal class WheelButton : UIElement
    {
        private readonly int x;
        private readonly int y;

        WheelButton(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {

        }
    }

    public class HammerWheelUI : UIElement
    {
        public HammerWheelUI()
        {
            Width.Set(100, 0f);
            Height.Set(100, 0f);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {

        }
    }
}
