using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace LivingWorldMod.Content.TownNPCRevitalization.UI.TaxSheet;

/// <summary>
///     UIState for the Tax Sheet UI added to the Tax Collector.
/// </summary>
public class TaxSheetUIState : UIState {
    private UIPanel _backPanel;

    public NPC NPCBeingTalkedTo {
        get;
        private set;
    }

    public override void OnInitialize() {
        Asset<Texture2D> vanillaPanelBackground = Main.Assets.Request<Texture2D>("Images/UI/PanelBackground");
        Asset<Texture2D> gradientPanelBorder = ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Elements/GradientPanelBorder");
        Asset<Texture2D> shadowedPanelBorder = ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Elements/ShadowedPanelBorder");

        _backPanel = new UIPanel(vanillaPanelBackground, gradientPanelBorder) {
            BackgroundColor = LWMUtils.LWMCustomUIPanelBackgroundColor,
            BorderColor = Color.White,
            HAlign = 0.5f,
            VAlign = 0.5f,
            Width = StyleDimension.FromPixels(200f),
            Height = StyleDimension.FromPixels(400f)
        };

        Append(_backPanel);
    }

    public void SetStateToNPC(NPC npc) {
        NPCBeingTalkedTo = npc;

        RecalculateChildren();
    }
}