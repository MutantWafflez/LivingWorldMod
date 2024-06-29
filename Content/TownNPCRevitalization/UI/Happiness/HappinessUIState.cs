using System.Linq;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Globals.UIElements;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace LivingWorldMod.Content.TownNPCRevitalization.UI.Happiness;

public class HappinessUIState : UIState {
    private sealed class UIMoodModifier : UIElement {
        //TODO: Use Scalie's asset generator?

        public readonly UITooltipElement tooltipElement;
        public readonly UIPanel backPanel;

        public readonly UIBetterText moodDescriptionText;
        public readonly UIBetterText moodOffsetText;

        public UIMoodModifier(MoodModifierInstance instance) {
            Height = StyleDimension.FromPixels(40f);
            Width = StyleDimension.Fill;

            tooltipElement = new UITooltipElement(instance.flavorText, instance.flavorTextSubstitutes) {
                Width = StyleDimension.Fill,
                Height = StyleDimension.Fill
            };
            Append(tooltipElement);

            backPanel = new UIPanel(Main.Assets.Request<Texture2D>("Images/UI/PanelBackground"), ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Elements/GradientPanelBorder")) {
                Width = StyleDimension.Fill,
                Height = StyleDimension.Fill
            };
            tooltipElement.Append(backPanel);

            moodDescriptionText = new UIBetterText(instance.modifierType.ModifierDesc) {
                Height = StyleDimension.Fill,
                VAlign = 0.5f
            };
            backPanel.Append(moodDescriptionText);

            moodOffsetText = new UIBetterText(instance.modifierType.MoodOffset.ToString("#.##")) {
                Height = StyleDimension.Fill,
                HAlign = 1f,
                VAlign = 0.5f,
                TextColor = instance.modifierType.MoodOffset < 0f ? Color.Red : Color.Lime
            };
            backPanel.Append(moodOffsetText);
        }
    }

    public UIPanel backPanel;

    public UITooltipElement happinessBarZone;

    public UISquarePanel happinessBarBackPanel;

    public UISimpleRectangle happinessBar;

    public UIImage moodLowIcon;

    public UIImage moodMidIcon;

    public UIImage moodHighIcon;

    public UIBetterText priceModifierTextNumber;

    public UIImage moneyBagIcon;

    public UITooltipElement moneyBagTooltipElement;

    public UIPanel modifierListBackPanel;

    public UIBetterScrollbar modifierScrollbar;

    public UIList modifierList;

    public NPC NPCBeingTalkedTo {
        get;
        private set;
    }

    public override void OnInitialize() {
        Asset<Texture2D> vanillaPanelBackground = Main.Assets.Request<Texture2D>("Images/UI/PanelBackground");
        Asset<Texture2D> gradientPanelBorder = ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Elements/GradientPanelBorder");
        Asset<Texture2D> shadowedPanelBorder = ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Elements/ShadowedPanelBorder");

        backPanel = new UIPanel(vanillaPanelBackground, gradientPanelBorder) {
            BackgroundColor = new Color(59, 97, 203),
            BorderColor = Color.White,
            HAlign = 0.5f,
            VAlign = 0.5f,
            Width = StyleDimension.FromPixels(300f),
            Height = StyleDimension.FromPixels(300f)
        };
        backPanel.SetPadding(12f);

        happinessBarZone = new UITooltipElement("UI.Fraction".Localized(), new {
            Numerator = 0,
            Denominator = 0
        }) {
            Width = StyleDimension.Fill,
            Height = StyleDimension.FromPixels(50f)
        };
        backPanel.Append(happinessBarZone);

        happinessBarBackPanel = new UISquarePanel(new Color(22, 29, 107), new Color(46, 46, 159)) {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill
        };
        happinessBarZone.Append(happinessBarBackPanel);

        happinessBar = new UISimpleRectangle(Color.White) {
            Height = StyleDimension.FromPercent(0.75f),
            Width = StyleDimension.FromPercent(0.5f),
            VAlign = 0.5f
        };
        happinessBarBackPanel.innerRectangle.Append(happinessBar);

        moodLowIcon = new UIImage(ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Icons/TownNPCMoodLow")) {
            Top = StyleDimension.FromPixelsAndPercent(4f, 1f),
            Width = StyleDimension.FromPixels(32),
            Height = StyleDimension.FromPercent(32)
        };
        happinessBarZone.Append(moodLowIcon);

        moodMidIcon = new UIImage(ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Icons/TownNPCMoodMid")) {
            Top = StyleDimension.FromPixelsAndPercent(4f, 1f),
            HAlign = 0.5f,
            Width = StyleDimension.FromPixels(32),
            Height = StyleDimension.FromPercent(32)
        };
        happinessBarZone.Append(moodMidIcon);

        moodHighIcon = new UIImage(ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Icons/TownNPCMoodHigh")) {
            Top = StyleDimension.FromPixelsAndPercent(4f, 1f),
            HAlign = 1f,
            Width = StyleDimension.FromPixels(32),
            Height = StyleDimension.FromPercent(32)
        };
        happinessBarZone.Append(moodHighIcon);

        moneyBagIcon = new UIImage(ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Icons/MoneyBag")) {
            Top = StyleDimension.FromPixels(100f),
            Width = StyleDimension.FromPixels(32),
            Height = StyleDimension.FromPixels(32)
        };
        backPanel.Append(moneyBagIcon);

        moneyBagTooltipElement = new UITooltipElement("UI.NPCHappiness.PriceModifier".Localized()) {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill
        };
        moneyBagIcon.Append(moneyBagTooltipElement);

        priceModifierTextNumber = new UIBetterText("", 0.75f, true) {
            Top = StyleDimension.FromPixels(2f),
            Left = StyleDimension.FromPixelsAndPercent(8f, 1f)
        };
        moneyBagTooltipElement.Append(priceModifierTextNumber);

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
            Width = StyleDimension.FromPixels(220f),
            Height = StyleDimension.Fill
        };
        modifierList.SetScrollbar(modifierScrollbar);
        modifierListBackPanel.Append(modifierList);

        Append(backPanel);
    }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);
        TownGlobalNPC globalNPC = NPCBeingTalkedTo.GetGlobalNPC<TownGlobalNPC>();
        TownNPCMoodModule moodModule = globalNPC.MoodModule;

        priceModifierTextNumber.SetText(Main.ShopHelper._currentPriceAdjustment.ToString("P1"));
        happinessBarZone.ReformatText(new {
            Numerator = moodModule.CurrentMood,
            Denominator = TownNPCMoodModule.MaxMoodValue
        });
        happinessBar.Width.Percent = Utils.Clamp(moodModule.CurrentMood / TownNPCMoodModule.MaxMoodValue, 0f, 1f);

        modifierList.Clear();
        foreach (MoodModifierInstance instance in moodModule.CurrentStaticMoodModifiers.Concat(moodModule.CurrentDynamicMoodModifiers)) {
            modifierList.Add(new UIMoodModifier(instance));
        }
    }

    public void SetStateToNPC(NPC npc) {
        NPCBeingTalkedTo = npc;
        modifierScrollbar.ViewPosition = 0f;

        RecalculateChildren();
    }
}