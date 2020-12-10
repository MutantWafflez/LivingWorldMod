using Microsoft.Xna.Framework;
using Terraria.UI;

namespace LivingWorldMod.UI
{
    public class ShrineUIState : UIState
    {
        public static ShrineUIPanel shrineUIPanel;

        public override void OnInitialize()
        {
            shrineUIPanel = new ShrineUIPanel();
            shrineUIPanel.Activate();
            Append(shrineUIPanel);
        }

        public override void Update(GameTime gameTime)
        {
            shrineUIPanel?.Update();
        }
    }
}