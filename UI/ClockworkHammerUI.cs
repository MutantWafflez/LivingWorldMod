using Terraria;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.ModLoader;
using LivingWorldMod.Items;
using System;
using Terraria.ID;

namespace LivingWorldMod.UI
{
    internal class ClockworkHammerUI: UIState
    {
        public const int BUTTON_COUNT = 8;

        private WheelButton[] buttons;
        public ClockworkHammer CurHammer { get; private set; }

        public override void OnInitialize()
        {
            //Console.WriteLine("intitialize clockwork hammer ui");
            buttons = new WheelButton[BUTTON_COUNT];
            for(int i = 0; i < buttons.Length; i++)
            {
                buttons[i] = new WheelButton(this, i);
                Append(buttons[i]);
            }
        }

        public ClockworkHammerUI ActivateFor(ClockworkHammer hammer)
        {
            CurHammer = hammer;
            return this;
        }
    }

    internal class WheelButton : UIImageButton
    {
        private const int BUTTON_SIZE = 36;
        private const int BUTTON_RADIUS = 50;
        private const double BUTTON_ANGLE_INCREMENT = Math.PI * 2 / ClockworkHammerUI.BUTTON_COUNT;

        private readonly ClockworkHammerUI state;
        private readonly SlopeSetting slopeSetting;
        private readonly double anglePos;

        public WheelButton(ClockworkHammerUI state, int index) : base(ModContent.GetTexture("LivingWorldMod/UI/HammerIcon_" + index))
        {
            this.state = state;
            this.slopeSetting = (SlopeSetting)index;
            //int x = Main.screenWidth / 2 - (ClockworkHammerUI.BUTTON_COUNT / 2 - index) * BUTTON_SIZE;
            //int y = Main.screenHeight / 2 - BUTTON_SIZE;
            this.anglePos = (Math.PI * 3 / 2) + BUTTON_ANGLE_INCREMENT * index; // determine radial position
            CalcPosition();
            // size is set in super
            //Width.Set(BUTTON_SIZE, 0);
            //Height.Set(BUTTON_SIZE, 0);

            OnClick += ButtonClicked;
        }

        private void CalcPosition()
        {
            int x = Main.screenWidth / 2 + (int)(Math.Cos(anglePos) * BUTTON_RADIUS);
            int y = Main.screenHeight / 2 + (int)(Math.Sin(anglePos) * BUTTON_RADIUS);
            Left.Set(x - BUTTON_SIZE / 2, 0);
            Top.Set(y - BUTTON_SIZE / 2, 0);
        }

        public override void Recalculate()
        {
            CalcPosition();
            base.Recalculate();
        }

        private void ButtonClicked(UIMouseEvent evt, UIElement listeningElement)
        {
            if(state.CurHammer != null)
                state.CurHammer.SlopeSetting = slopeSetting;
            LivingWorldMod.Instance.CloseClockworkHammerUI();
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
        }
    }
}
