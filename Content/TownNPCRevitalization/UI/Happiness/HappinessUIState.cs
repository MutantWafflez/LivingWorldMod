using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
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

        public UIMoodModifier(MoodModifierInstance instance) {
            Height = StyleDimension.FromPixels(40f);
            Width = StyleDimension.Fill;

            DynamicLocalizedText flavorText = new("UI.MoodFlavorText".Localized(), new { FlavorText = instance.FlavorText.ToString() });
            UITooltipElement tooltipElement = new (flavorText) { Width = StyleDimension.Fill, Height = StyleDimension.Fill };
            Append(tooltipElement);

            UIPanel backPanel = new (Main.Assets.Request<Texture2D>("Images/UI/PanelBackground"), ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Elements/GradientPanelBorder")) {
                Width = StyleDimension.Fill, Height = StyleDimension.Fill
            };
            tooltipElement.Append(backPanel);

            UIModifiedText moodDescriptionText = new (instance.DescriptionText.SubstitutedText) { Height = StyleDimension.Fill, VAlign = 0.5f };
            backPanel.Append(moodDescriptionText);

            UIModifiedText moodOffsetText = new (instance.MoodOffset.ToString("#.##")) {
                Height = StyleDimension.Fill, HAlign = 1f, VAlign = 0.5f, TextColor = instance.MoodOffset < 0f ? Color.Red : Color.Lime
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

    private UIPanel _moodBackPanel;

    private UIElement _npcInfoAndPriceZone;

    private UIImage _npcHeadIcon;

    private UIModifiedText _npcName;

    private UIModifiedText _priceModifierTextNumber;

    private UIImage _moneyBagIcon;

    private UITooltipElement _moneyBagTooltipElement;

    private UITooltipElement _happinessBarZone;

    private UISquarePanel _happinessBarBackPanel;

    private UISimpleRectangle _happinessBar;

    private UIImage _moodLowIcon;

    private UIImage _moodMidIcon;

    private UIImage _moodHighIcon;

    private UIPanel _modifierListBackPanel;

    private UIBetterScrollbar _modifierScrollbar;

    private UIList _modifierList;

    public NPC NPCBeingTalkedTo {
        get;
        private set;
    }

    public override void OnInitialize() {
        Asset<Texture2D> vanillaPanelBackground = Main.Assets.Request<Texture2D>("Images/UI/PanelBackground");
        Asset<Texture2D> gradientPanelBorder = ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Elements/GradientPanelBorder");
        Asset<Texture2D> shadowedPanelBorder = ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Elements/ShadowedPanelBorder");

        _moodBackPanel = new UIPanel(vanillaPanelBackground, gradientPanelBorder) {
            BackgroundColor = LWMUtils.LWMCustomUIPanelBackgroundColor,
            BorderColor = Color.White,
            HAlign = 0.5f,
            VAlign = 0.5f,
            Width = StyleDimension.FromPixels(MoodBackPanelWidth),
            Height = StyleDimension.FromPixels(MoodBackPanelHeight)
        };
        Append(_moodBackPanel);

        _npcInfoAndPriceZone = new UIElement { Width = StyleDimension.Fill, Height = StyleDimension.FromPixels(32f) } ;
        _moodBackPanel.Append(_npcInfoAndPriceZone);

        _npcHeadIcon = new UIImage(TextureAssets.MagicPixel) { VAlign = 0.5f };
        _npcInfoAndPriceZone.Append(_npcHeadIcon);

        _npcName = new UIModifiedText("NPC Name", 0.75f, true) { Left = StyleDimension.FromPixels(NPCNameXPos), VAlign = 0.5f, horizontalTextConstraint = NPCNameTextConstraint };
        _npcInfoAndPriceZone.Append(_npcName);

        _moneyBagIcon = new UIImage(ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Icons/MoneyBag")) {
            HAlign = 1f, Width = StyleDimension.FromPixels(IconSideLength), Height = StyleDimension.FromPixels(IconSideLength)
        };
        _npcInfoAndPriceZone.Append(_moneyBagIcon);

        _moneyBagTooltipElement = new UITooltipElement("UI.NPCHappiness.PriceModifier".Localized()) { Width = StyleDimension.Fill, Height = StyleDimension.Fill };
        _moneyBagIcon.Append(_moneyBagTooltipElement);

        _priceModifierTextNumber = new UIModifiedText("", 0.75f, true) { VAlign = 0.5f, Left = StyleDimension.FromPixelsAndPercent(PriceModifierTextXOffset, 1f) };
        _npcInfoAndPriceZone.Append(_priceModifierTextNumber);

        _happinessBarZone = new UITooltipElement(new DynamicLocalizedText("UI.Fraction".Localized(), new { Numerator = 0, Denominator = 0 })) {
            Width = StyleDimension.Fill, Height = StyleDimension.FromPixels(HappinessBarZoneHeight), Top = StyleDimension.FromPixels(HappinessBarZoneYPos)
        };
        _moodBackPanel.Append(_happinessBarZone);

        _happinessBarBackPanel = new UISquarePanel(LWMUtils.LWMCustomUISubPanelBorderColor, LWMUtils.LWMCustomUISubPanelBackgroundColor) { Width = StyleDimension.Fill, Height = StyleDimension.Fill };
        _happinessBarZone.Append(_happinessBarBackPanel);

        _happinessBar = new UISimpleRectangle(Color.White) { Height = StyleDimension.FromPercent(0.75f), Width = StyleDimension.FromPercent(0.5f), VAlign = 0.5f };
        _happinessBarBackPanel.innerRectangle.Append(_happinessBar);

        _moodLowIcon = new UIImage(ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Icons/TownNPCMoodLow")) {
            Top = StyleDimension.FromPixelsAndPercent(MoodIconsYOffset, 1f), Width = StyleDimension.FromPixels(IconSideLength), Height = StyleDimension.FromPercent(IconSideLength)
        };
        _happinessBarZone.Append(_moodLowIcon);

        _moodMidIcon = new UIImage(ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Icons/TownNPCMoodMid")) {
            Top = StyleDimension.FromPixelsAndPercent(MoodIconsYOffset, 1f), HAlign = 0.5f, Width = StyleDimension.FromPixels(IconSideLength), Height = StyleDimension.FromPercent(IconSideLength)
        };
        _happinessBarZone.Append(_moodMidIcon);

        _moodHighIcon = new UIImage(ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Icons/TownNPCMoodHigh")) {
            Top = StyleDimension.FromPixelsAndPercent(MoodIconsYOffset, 1f), HAlign = 1f, Width = StyleDimension.FromPixels(IconSideLength), Height = StyleDimension.FromPercent(IconSideLength)
        };
        _happinessBarZone.Append(_moodHighIcon);

        _modifierListBackPanel = new UIPanel(vanillaPanelBackground, shadowedPanelBorder) {
            BackgroundColor = LWMUtils.LWMCustomUISubPanelBackgroundColor,
            BorderColor = LWMUtils.LWMCustomUISubPanelBorderColor,
            Width = StyleDimension.Fill,
            Height = StyleDimension.FromPixels(ModifierListBackPanelHeight),
            VAlign = 1f
        };
        _moodBackPanel.Append(_modifierListBackPanel);

        _modifierScrollbar = new UIBetterScrollbar { Left = StyleDimension.FromPixelsAndPercent(ModifierScrollbarXOffset, 1f), Height = StyleDimension.Fill };
        _modifierListBackPanel.Append(_modifierScrollbar);

        _modifierList = new UIList { Width = StyleDimension.FromPixels(ModifierListWidth), Height = StyleDimension.Fill };
        _modifierList.SetScrollbar(_modifierScrollbar);
        _modifierListBackPanel.Append(_modifierList);
    }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);

        if (_moodBackPanel.IsMouseHovering) {
            Main.LocalPlayer.mouseInterface = true;
        }
    }

    public void SetStateToNPC(NPC npc) {
        NPCBeingTalkedTo = npc;
        _modifierScrollbar.ViewPosition = 0f;
        _npcHeadIcon.SetImage(TextureAssets.NpcHead[TownNPCProfiles.GetHeadIndexSafe(npc)]);
        _npcName.SetText(npc.GivenOrTypeName);

        RefreshMoodModifierList();
    }

    public void RefreshModifierList() {
        if (NPCBeingTalkedTo is null) {
            return;
        }

        RefreshMoodModifierList();
    }

    public void ClearState() {
        NPCBeingTalkedTo = null;
    }

    private void RefreshMoodModifierList() {
        TownNPCMoodModule moodModule = NPCBeingTalkedTo.GetGlobalNPC<TownNPCMoodModule>();

        _priceModifierTextNumber.SetText(Main.ShopHelper._currentPriceAdjustment.ToString("0.#%"));
        _priceModifierTextNumber.Left.Set(PriceModifierTextXOffset - IconSideLength - _priceModifierTextNumber.GetDimensions().Width, 1f);

        _happinessBarZone.ReformatText(new { Numerator = moodModule.CurrentMood, Denominator = TownNPCMoodModule.MaxMoodValue });
        _happinessBar.Width.Percent = Utils.Clamp(moodModule.CurrentMood / TownNPCMoodModule.MaxMoodValue, 0f, 1f);

        _modifierList.Clear();
        foreach (MoodModifierInstance instance in moodModule.CurrentMoodModifiers) {
            _modifierList.Add(new UIMoodModifier(instance));
        }

        RecalculateChildren();
    }
}