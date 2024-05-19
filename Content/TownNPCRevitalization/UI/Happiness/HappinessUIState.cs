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

    public UISimpleRectangle happinessBar;

    public UIImage desiredHappinessArrow;

    public UIPanel modifierListBackPanel;

    public UIBetterScrollbar modifierScrollbar;

    public UIList modifierList;

    public override void OnInitialize() {
        Asset<Texture2D> vanillaPanelBackground = Main.Assets.Request<Texture2D>("Images/UI/PanelBackground");
        Asset<Texture2D> gradientPanelBorder = ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Elements/GradientPanelBorder");
        Asset<Texture2D> shadowedPanelBorder = ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Elements/ShadowedPanelBorder");

        backPanel = new UIPanel(vanillaPanelBackground, gradientPanelBorder) {
            BackgroundColor = new Color(59, 97, 203),
            BorderColor = Color.White,
            HAlign = 0.5f,
            VAlign = 0.5f,
            Width = StyleDimension.FromPixels(200f),
            Height = StyleDimension.FromPixels(300f)
        };
        backPanel.SetPadding(12f);

        happinessBarBackPanel = new UISquarePanel(new Color(22, 29, 107), new Color(46, 46, 159)) {
            Width = StyleDimension.Fill,
            Height = StyleDimension.FromPixels(50f)
        };
        backPanel.Append(happinessBarBackPanel);

        happinessBar = new UISimpleRectangle(Color.White) {
            Height = StyleDimension.FromPercent(0.75f),
            Width = StyleDimension.FromPercent(0.5f),
            VAlign = 0.5f
        };
        happinessBarBackPanel.innerRectangle.Append(happinessBar);

        desiredHappinessArrow = new UIImage(Main.Assets.Request<Texture2D>("Images/UI/VK_Shift")) {
            VAlign = 1f,
            Left = StyleDimension.FromPixelsAndPercent(-13f, 0.5f)
        };
        happinessBarBackPanel.Append(desiredHappinessArrow);

        modifierListBackPanel = new UIPanel(vanillaPanelBackground, shadowedPanelBorder) {
            BackgroundColor = new Color(46, 46, 159),
            BorderColor = new Color(22, 29, 107),
            Width = StyleDimension.Fill,
            Height = StyleDimension.FromPixels(180f),
            VAlign = 1f
        };
        backPanel.Append(modifierListBackPanel);

        modifierScrollbar = new UIBetterScrollbar {
            Left = StyleDimension.FromPixelsAndPercent(-20f, 1f),
            Height = StyleDimension.Fill
        };
        modifierListBackPanel.Append(modifierScrollbar);

        modifierList = new UIList {
            Width = StyleDimension.FromPixels(160f),
            Height = StyleDimension.Fill
        };
        modifierList.SetScrollbar(modifierScrollbar);
        modifierListBackPanel.Append(modifierList);

        Append(backPanel);
    }

    public void SetStateToNPC(NPC npc) {
        NPCBeingTalkedTo = npc;
    }
}