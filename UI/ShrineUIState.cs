using BuilderEssentials.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using static Terraria.ModLoader.ModContent;
using LivingWorldMod.UI.Elements;
using LivingWorldMod.Tiles.Interactables;
using LivingWorldMod.NPCs.Villagers;
using On.Terraria.GameContent.Biomes;

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
