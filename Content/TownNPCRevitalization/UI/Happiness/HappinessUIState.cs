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

        public readonly UIModifiedText moodDescriptionText;
        public readonly UIModifiedText moodOffsetText;

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

            moodDescriptionText = new UIModifiedText(instance.descriptionText.SubstitutedText) { Height = StyleDimension.Fill, VAlign = 0.5f };
            backPanel.Append(moodDescriptionText);

            moodOffsetText = new UIModifiedText(instance.moodOffset.ToString("#.##")) {
                Height = StyleDimension.Fill, HAlign = 1f, VAlign = 0.5f, TextColor = instance.moodOffset < 0f ? Color.Red : Color.Lime
            };
            backPanel.Append(moodOffsetText);
        }
    }

    private const float MoodBackPanelWidth = 350f;
    private const float MoodBackPanelHeight = 300f;

    private const float NPCNameXPos = 36f;
    private const float NPCNameTextConstraint = 150f;

    private const float IconSideLength = 32f;

    private const float PriceModifierTextXOffset = -8f;

    private const float HappinessBarZoneHeight = 50f;
    private const float HappinessBarZoneYPos = 40f;

    private const float MoodIconsYOffset = 4f;

    private const float ModifierListBackPanelHeight = 140f;
    private const float ModifierScrollbarXOffset = -20f;
    private const float ModifierListWidth = MoodBackPanelWidth - 80f;

    public UIPanel moodBackPanel;

    public UIElement npcInfoAndPriceZone;

    public UIImage npcHeadIcon;

    public UIModifiedText npcName;

    public UIModifiedText priceModifierTextNumber;

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
            BackgroundColor = new Color(59, 97, 203),
            BorderColor = Color.White,
            HAlign = 0.5f,
            VAlign = 0.5f,
            Width = StyleDimension.FromPixels(MoodBackPanelWidth),
            Height = StyleDimension.FromPixels(MoodBackPanelHeight)
        };

        npcInfoAndPriceZone = new UIElement { Width = StyleDimension.Fill, Height = StyleDimension.FromPixels(32f) } ;
        moodBackPanel.Append(npcInfoAndPriceZone);

        npcHeadIcon = new UIImage(TextureAssets.MagicPixel) { VAlign = 0.5f };
        npcInfoAndPriceZone.Append(npcHeadIcon);

        npcName = new UIModifiedText("NPC Name", 0.75f, true) { Left = StyleDimension.FromPixels(NPCNameXPos), VAlign = 0.5f, horizontalTextConstraint = NPCNameTextConstraint };
        npcInfoAndPriceZone.Append(npcName);

        moneyBagIcon = new UIImage(ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Icons/MoneyBag")) {
            HAlign = 1f, Width = StyleDimension.FromPixels(IconSideLength), Height = StyleDimension.FromPixels(IconSideLength)
        };
        npcInfoAndPriceZone.Append(moneyBagIcon);

        moneyBagTooltipElement = new UITooltipElement("UI.NPCHappiness.PriceModifier".Localized()) { Width = StyleDimension.Fill, Height = StyleDimension.Fill };
        moneyBagIcon.Append(moneyBagTooltipElement);

        priceModifierTextNumber = new UIModifiedText("", 0.75f, true) { VAlign = 0.5f, Left = StyleDimension.FromPixelsAndPercent(PriceModifierTextXOffset, 1f) };
        npcInfoAndPriceZone.Append(priceModifierTextNumber);

        happinessBarZone = new UITooltipElement(new DynamicLocalizedText("UI.Fraction".Localized(), new { Numerator = 0, Denominator = 0 })) {
            Width = StyleDimension.Fill, Height = StyleDimension.FromPixels(HappinessBarZoneHeight), Top = StyleDimension.FromPixels(HappinessBarZoneYPos)
        };
        moodBackPanel.Append(happinessBarZone);

        happinessBarBackPanel = new UISquarePanel(new Color(22, 29, 107), new Color(46, 46, 159)) { Width = StyleDimension.Fill, Height = StyleDimension.Fill };
        happinessBarZone.Append(happinessBarBackPanel);

        happinessBar = new UISimpleRectangle(Color.White) { Height = StyleDimension.FromPercent(0.75f), Width = StyleDimension.FromPercent(0.5f), VAlign = 0.5f };
        happinessBarBackPanel.innerRectangle.Append(happinessBar);

        moodLowIcon = new UIImage(ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Icons/TownNPCMoodLow")) {
            Top = StyleDimension.FromPixelsAndPercent(MoodIconsYOffset, 1f), Width = StyleDimension.FromPixels(IconSideLength), Height = StyleDimension.FromPercent(IconSideLength)
        };
        happinessBarZone.Append(moodLowIcon);

        moodMidIcon = new UIImage(ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Icons/TownNPCMoodMid")) {
            Top = StyleDimension.FromPixelsAndPercent(MoodIconsYOffset, 1f), HAlign = 0.5f, Width = StyleDimension.FromPixels(IconSideLength), Height = StyleDimension.FromPercent(IconSideLength)
        };
        happinessBarZone.Append(moodMidIcon);

        moodHighIcon = new UIImage(ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Icons/TownNPCMoodHigh")) {
            Top = StyleDimension.FromPixelsAndPercent(MoodIconsYOffset, 1f), HAlign = 1f, Width = StyleDimension.FromPixels(IconSideLength), Height = StyleDimension.FromPercent(IconSideLength)
        };
        happinessBarZone.Append(moodHighIcon);

        modifierListBackPanel = new UIPanel(vanillaPanelBackground, shadowedPanelBorder) {
            BackgroundColor = new Color(46, 46, 159),
            BorderColor = new Color(22, 29, 107),
            Width = StyleDimension.Fill,
            Height = StyleDimension.FromPixels(ModifierListBackPanelHeight),
            VAlign = 1f
        };
        moodBackPanel.Append(modifierListBackPanel);

        modifierScrollbar = new UIBetterScrollbar { Left = StyleDimension.FromPixelsAndPercent(ModifierScrollbarXOffset, 1f), Height = StyleDimension.Fill };
        modifierListBackPanel.Append(modifierScrollbar);

        modifierList = new UIList { Width = StyleDimension.FromPixels(ModifierListWidth), Height = StyleDimension.Fill };
        modifierList.SetScrollbar(modifierScrollbar);
        modifierListBackPanel.Append(modifierList);

        Append(moodBackPanel);
    }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);
        TownNPCMoodModule moodModule = NPCBeingTalkedTo.GetGlobalNPC<TownNPCMoodModule>();

        priceModifierTextNumber.SetText(Main.ShopHelper._currentPriceAdjustment.ToString("0.#%"));
        priceModifierTextNumber.Left.Set(PriceModifierTextXOffset - IconSideLength - priceModifierTextNumber.GetDimensions().Width, 1f);

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