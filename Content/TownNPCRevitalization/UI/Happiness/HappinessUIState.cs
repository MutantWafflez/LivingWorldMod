using LivingWorldMod.Globals.UIElements;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace LivingWorldMod.Content.TownNPCRevitalization.UI.Happiness;

public class HappinessUIState : UIState {
    private sealed class UIHappinessModifier : UIPanel {
        //TODO: Use Scalie's asset generator?
        public UIHappinessModifier() : base(Main.Assets.Request<Texture2D>("Images/UI/PanelBackground"), ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Elements/GradientPanelBorder")) {
            Height = StyleDimension.FromPixels(40f);
            Width = StyleDimension.Fill;
        }
    }

    public NPC NPCBeingTalkedTo {
        get;
        private set;
    }

    public UIPanel backPanel;

    public UISquarePanel happinessBarBackPanel;

    public UISimpleRectangle happinessBar;

    public UIImage desiredHappinessArrow;

    public UIBetterText discountTextNumber;

    public UIImage moneyBagIcon;

    public UITooltipElement moneyBagTooltipElement;

    public UIPanel modifierListBackPanel;

    public UIBetterScrollbar modifierScrollbar;

    public UIList modifierList;

    public override void OnInitialize() { }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);

        Asset<Texture2D> vanillaPanelBackground = Main.Assets.Request<Texture2D>("Images/UI/PanelBackground");
        Asset<Texture2D> gradientPanelBorder = ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Elements/GradientPanelBorder");
        Asset<Texture2D> shadowedPanelBorder = ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Elements/ShadowedPanelBorder");

        backPanel = new UIPanel(vanillaPanelBackground, gradientPanelBorder) {
            IgnoresMouseInteraction = true,
            BackgroundColor = new Color(59, 97, 203),
            BorderColor = Color.White,
            HAlign = 0.5f,
            VAlign = 0.5f,
            Width = StyleDimension.FromPixels(300f),
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
            Left = StyleDimension.FromPixelsAndPercent(-13f, 0.5f),
            Top = StyleDimension.FromPixelsAndPercent(4f, 1f)
        };
        happinessBarBackPanel.Append(desiredHappinessArrow);

        moneyBagIcon = new UIImage(ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Icons/MoneyBag")) {
            Top = StyleDimension.FromPixels(100f)
        };
        backPanel.Append(moneyBagIcon);

        moneyBagTooltipElement = new UITooltipElement("UI.NPCHappiness.Discount".Localized()) {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill
        };
        moneyBagIcon.Append(moneyBagTooltipElement);

        discountTextNumber = new UIBetterText("88%", 0.75f, true) {
            Top = StyleDimension.FromPixels(2f),
            Left = StyleDimension.FromPixelsAndPercent(8f, 1f)
        };
        moneyBagTooltipElement.Append(discountTextNumber);

        modifierListBackPanel = new UIPanel(vanillaPanelBackground, shadowedPanelBorder) {
            BackgroundColor = new Color(46, 46, 159),
            BorderColor = new Color(22, 29, 107),
            Width = StyleDimension.Fill,
            Height = StyleDimension.FromPixels(140f),
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

        RemoveAllChildren();
        Append(backPanel);
    }

    public void SetStateToNPC(NPC npc) {
        NPCBeingTalkedTo = npc;
    }
}