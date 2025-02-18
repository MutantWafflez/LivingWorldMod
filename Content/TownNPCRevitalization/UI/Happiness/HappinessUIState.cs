using System;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs.TownNPCModules;
using LivingWorldMod.DataStructures.Structs;
using LivingWorldMod.Globals.UIElements;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;
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

            DynamicLocalizedText templatedFlavorText = instance.duration >= LWMUtils.RealLifeSecond
                ? new DynamicLocalizedText (
                    "UI.DurationMoodFlavorText".Localized(),
                    new { FlavorText = instance.flavorText.ToString(), Time = Lang.LocalizedDuration(new TimeSpan(0, 0, instance.duration / LWMUtils.RealLifeSecond), true, true) }
                )
                : new DynamicLocalizedText("UI.MoodFlavorText".Localized(), new { FlavorText = instance.flavorText.ToString() });
            tooltipElement = new UITooltipElement(templatedFlavorText) { Width = StyleDimension.Fill, Height = StyleDimension.Fill };
            Append(tooltipElement);

            backPanel = new UIPanel(Main.Assets.Request<Texture2D>("Images/UI/PanelBackground"), ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Elements/GradientPanelBorder")) {
                Width = StyleDimension.Fill, Height = StyleDimension.Fill
            };
            tooltipElement.Append(backPanel);

            moodDescriptionText = new UIBetterText(instance.descriptionText.SubstitutedText) { Height = StyleDimension.Fill, VAlign = 0.5f };
            backPanel.Append(moodDescriptionText);

            moodOffsetText = new UIBetterText(instance.moodOffset.ToString("#.##")) {
                Height = StyleDimension.Fill, HAlign = 1f, VAlign = 0.5f, TextColor = instance.moodOffset < 0f ? Color.Red : Color.Lime
            };
            backPanel.Append(moodOffsetText);
        }
    }

    public UIPanel moodBackPanel;

    public UIElement npcInfoAndPriceZone;

    public UIImage npcHeadIcon;

    public UIBetterText npcName;

    public UIBetterText priceModifierTextNumber;

    public UIImage moneyBagIcon;

    public UITooltipElement moneyBagTooltipElement;

    public UITooltipElement happinessBarZone;

    public UISquarePanel happinessBarBackPanel;

    public UISimpleRectangle happinessBar;

    public UIImage moodLowIcon;

    public UIImage moodMidIcon;

    public UIImage moodHighIcon;

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

        moodBackPanel = new UIPanel(vanillaPanelBackground, gradientPanelBorder) {
            BackgroundColor = LWMUtils.LWMCustomUIPanelBackgroundColor,
            BorderColor = Color.White,
            HAlign = 0.5f,
            VAlign = 0.5f,
            Width = StyleDimension.FromPixels(350f),
            Height = StyleDimension.FromPixels(300f)
        };
        moodBackPanel.SetPadding(12f);

        npcInfoAndPriceZone = new UIElement { Width = StyleDimension.Fill, Height = StyleDimension.FromPixels(32f) } ;
        moodBackPanel.Append(npcInfoAndPriceZone);

        npcHeadIcon = new UIImage(TextureAssets.MagicPixel) { VAlign = 0.5f };
        npcInfoAndPriceZone.Append(npcHeadIcon);

        npcName = new UIBetterText("NPC Name", 0.75f, true) { Left = StyleDimension.FromPixels(36f), VAlign = 0.5f, horizontalTextConstraint = 150f };
        npcInfoAndPriceZone.Append(npcName);

        moneyBagIcon = new UIImage(ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Icons/MoneyBag")) { HAlign = 1f, Width = StyleDimension.FromPixels(32), Height = StyleDimension.FromPixels(32) };
        npcInfoAndPriceZone.Append(moneyBagIcon);

        moneyBagTooltipElement = new UITooltipElement("UI.NPCHappiness.PriceModifier".Localized()) { Width = StyleDimension.Fill, Height = StyleDimension.Fill };
        moneyBagIcon.Append(moneyBagTooltipElement);

        priceModifierTextNumber = new UIBetterText("", 0.75f, true) { VAlign = 0.5f, Left = StyleDimension.FromPixelsAndPercent(-8f, 1f) };
        npcInfoAndPriceZone.Append(priceModifierTextNumber);

        happinessBarZone = new UITooltipElement(new DynamicLocalizedText("UI.Fraction".Localized(), new { Numerator = 0, Denominator = 0 })) {
            Width = StyleDimension.Fill, Height = StyleDimension.FromPixels(50f), Top = StyleDimension.FromPixels(40f)
        };
        moodBackPanel.Append(happinessBarZone);

        happinessBarBackPanel = new UISquarePanel(LWMUtils.LWMCustomUISubPanelBorderColor, LWMUtils.LWMCustomUISubPanelBackgroundColor) { Width = StyleDimension.Fill, Height = StyleDimension.Fill };
        happinessBarZone.Append(happinessBarBackPanel);

        happinessBar = new UISimpleRectangle(Color.White) { Height = StyleDimension.FromPercent(0.75f), Width = StyleDimension.FromPercent(0.5f), VAlign = 0.5f };
        happinessBarBackPanel.innerRectangle.Append(happinessBar);

        moodLowIcon = new UIImage(ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Icons/TownNPCMoodLow")) {
            Top = StyleDimension.FromPixelsAndPercent(4f, 1f), Width = StyleDimension.FromPixels(32), Height = StyleDimension.FromPercent(32)
        };
        happinessBarZone.Append(moodLowIcon);

        moodMidIcon = new UIImage(ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Icons/TownNPCMoodMid")) {
            Top = StyleDimension.FromPixelsAndPercent(4f, 1f), HAlign = 0.5f, Width = StyleDimension.FromPixels(32), Height = StyleDimension.FromPercent(32)
        };
        happinessBarZone.Append(moodMidIcon);

        moodHighIcon = new UIImage(ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Icons/TownNPCMoodHigh")) {
            Top = StyleDimension.FromPixelsAndPercent(4f, 1f), HAlign = 1f, Width = StyleDimension.FromPixels(32), Height = StyleDimension.FromPercent(32)
        };
        happinessBarZone.Append(moodHighIcon);


        modifierListBackPanel = new UIPanel(vanillaPanelBackground, shadowedPanelBorder) {
            BackgroundColor = LWMUtils.LWMCustomUISubPanelBackgroundColor,
            BorderColor = LWMUtils.LWMCustomUISubPanelBorderColor,
            Width = StyleDimension.Fill,
            Height = StyleDimension.FromPixels(140f),
            VAlign = 1f
        };
        moodBackPanel.Append(modifierListBackPanel);

        modifierScrollbar = new UIBetterScrollbar { Left = StyleDimension.FromPixelsAndPercent(-20f, 1f), Height = StyleDimension.Fill };
        modifierListBackPanel.Append(modifierScrollbar);

        modifierList = new UIList { Width = StyleDimension.FromPixels(270f), Height = StyleDimension.Fill };
        modifierList.SetScrollbar(modifierScrollbar);
        modifierListBackPanel.Append(modifierList);

        Append(moodBackPanel);
    }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);
        TownNPCMoodModule moodModule = NPCBeingTalkedTo.GetGlobalNPC<TownNPCMoodModule>();

        priceModifierTextNumber.SetText(Main.ShopHelper._currentPriceAdjustment.ToString("0.#%"));
        priceModifierTextNumber.Left.Set(-40f - priceModifierTextNumber.GetDimensions().Width, 1f);

        happinessBarZone.ReformatText(new { Numerator = moodModule.CurrentMood, Denominator = TownNPCMoodModule.MaxMoodValue });
        happinessBar.Width.Percent = Utils.Clamp(moodModule.CurrentMood / TownNPCMoodModule.MaxMoodValue, 0f, 1f);

        modifierList.Clear();
        foreach (MoodModifierInstance instance in moodModule.CurrentMoodModifiers) {
            modifierList.Add(new UIMoodModifier(instance));
        }
    }

    public void SetStateToNPC(NPC npc) {
        NPCBeingTalkedTo = npc;
        modifierScrollbar.ViewPosition = 0f;
        npcHeadIcon.SetImage(TextureAssets.NpcHead[TownNPCProfiles.GetHeadIndexSafe(npc)]);
        npcName.SetText(npc.GivenOrTypeName);

        RecalculateChildren();
    }

    public void ClearState() {
        NPCBeingTalkedTo = null;
    }
}