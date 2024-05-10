using LivingWorldMod.Globals.UIElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace LivingWorldMod.Content.TownNPCRevitalization.UI.Happiness;

public class HappinessUIState : UIState {
    public NPC NPCBeingTalkedTo {
        get;
        private set;
    }

    public UIPanel backPanel;

    public UISquarePanel happinessBarBackPanel;


    public override void OnInitialize() {
        Asset<Texture2D> vanillaPanelBackground = Main.Assets.Request<Texture2D>("Images/UI/PanelBackground");
        Asset<Texture2D> gradientPanelBorder = ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Elements/GradientPanelBorder");
        Asset<Texture2D> shadowedPanelBorder = ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Elements/ShadowedPanelBorder");

        backPanel = new UIPanel(vanillaPanelBackground, gradientPanelBorder) {
            BackgroundColor = new Color(59, 97, 203),
            BorderColor = Color.White,
            HAlign = 0.5f,
            VAlign = 0.5f
        };
        backPanel.Width.Set(200f, 0f);
        backPanel.Height.Set(300f, 0f);
        backPanel.SetPadding(12f);

        happinessBarBackPanel = new UISquarePanel(new Color(22, 29, 107), new Color(46, 46, 159)) {
            Width = StyleDimension.Fill,
            Height = StyleDimension.FromPixels(50f)
        };
        backPanel.Append(happinessBarBackPanel);

        Append(backPanel);
    }

    public void SetStateToNPC(NPC npc) {
        NPCBeingTalkedTo = npc;
    }
}