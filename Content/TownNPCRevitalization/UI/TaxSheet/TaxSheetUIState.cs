using System;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Globals.UIElements;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace LivingWorldMod.Content.TownNPCRevitalization.UI.TaxSheet;

/// <summary>
///     UIState for the Tax Sheet UI added to the Tax Collector.
/// </summary>
public class TaxSheetUIState : UIState {
    private const float BackPanelSideLength = 200f;
    private const float HelpPanelSideLength = 32f;
    private const float PaddingBetweenPanels = 4f;
    private const float SelectedNPCBackPanelWidth = 130f;
    private const float CoinDisplayPadding = 40f;
    private const float TaxChangeButtonsSideLength = 30f;

    // TODO: Use actual tax system number
    private long _propertyTaxValue;
    private long _salesTaxValue;

    private UIPanel _backPanel;
    private UIBetterText _titleText;
    private UIPanel _npcGridSubPanel;
    private UIBetterScrollbar _npcGridScrollBar;
    private UIGrid _npcGrid;

    private UIPanel _selectedNPCBackPanel;
    private UIBetterText _selectedNPCName;
    private UIBetterText _propertyTaxText;
    private UIBetterImageButton[,] _propertyTaxChangeButtons;
    private UICoinDisplay _propertyTaxDisplay;
    private UIBetterText _salesTaxText;
    private UIBetterImageButton[,] _salesTaxChangeButtons;
    private UICoinDisplay _salesTaxDisplay;

    private UIPanel _helpIconPanel;
    private UITooltipElement _helpIconTooltipZone;
    private UIImage _helpIcon;

    public NPC NPCBeingTalkedTo {
        get;
        private set;
    }

    public override void OnInitialize() {
        Asset<Texture2D> vanillaPanelBackground = Main.Assets.Request<Texture2D>("Images/UI/PanelBackground");
        Asset<Texture2D> gradientPanelBorder = ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Elements/GradientPanelBorder");
        Asset<Texture2D> shadowedPanelBorder = ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Elements/ShadowedPanelBorder");
        Asset<Texture2D>[] changeButtonTextures = [
            ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/ButtonIcons/PlusButton"), ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/ButtonIcons/MinusButton")
        ];

        _backPanel = new UIPanel(vanillaPanelBackground, gradientPanelBorder) {
            BackgroundColor = LWMUtils.LWMCustomUIPanelBackgroundColor,
            BorderColor = Color.White,
            HAlign = 0.5f,
            VAlign = 0.5f,
            Width = StyleDimension.FromPixels(BackPanelSideLength),
            Height = StyleDimension.FromPixels(BackPanelSideLength)
        };
        Append(_backPanel);

        _titleText = new UIBetterText("UI.TaxSheet.Title".Localized(), 1.25f) { HAlign = 0.5f };
        _backPanel.Append(_titleText);

        _npcGridSubPanel = new UIPanel(vanillaPanelBackground, shadowedPanelBorder) {
            BackgroundColor = LWMUtils.LWMCustomUISubPanelBackgroundColor,
            BorderColor = LWMUtils.LWMCustomUISubPanelBorderColor,
            VAlign = 1f,
            Width = StyleDimension.Fill,
            Height = StyleDimension.FromPixels(145f)
        };
        _backPanel.Append(_npcGridSubPanel);

        _npcGridScrollBar = new UIBetterScrollbar { Left = StyleDimension.FromPixels(PaddingBetweenPanels), HAlign = 1f, Width = StyleDimension.FromPixels(24f), Height = StyleDimension.Fill };
        _npcGridSubPanel.Append(_npcGridScrollBar);

        _npcGrid = new UIGrid { Width = StyleDimension.FromPixelsAndPercent(-24f, 1), Height = StyleDimension.Fill, ListPadding = PaddingBetweenPanels };
        _npcGrid.SetScrollbar(_npcGridScrollBar);
        _npcGridSubPanel.Append(_npcGrid);

        _selectedNPCBackPanel = new UIPanel(vanillaPanelBackground, gradientPanelBorder) {
            BackgroundColor = LWMUtils.LWMCustomUIPanelBackgroundColor,
            BorderColor = Color.White,
            VAlign = 0.5f,
            Left = StyleDimension.FromPixels(_backPanel.GetDimensions().X + BackPanelSideLength + PaddingBetweenPanels),
            Width = StyleDimension.FromPixels(SelectedNPCBackPanelWidth),
            Height = StyleDimension.FromPixels(BackPanelSideLength + 110f)
        };
        Append(_selectedNPCBackPanel);

        _selectedNPCName = new UIBetterText("Guide", 1.25f) { HAlign = 0.5f, horizontalTextConstraint = SelectedNPCBackPanelWidth - PaddingBetweenPanels * 2f };
        _selectedNPCBackPanel.Append(_selectedNPCName);

        _propertyTaxText = new UIBetterText("Property Tax", 0.9f) { HAlign = 0.5f, Top = StyleDimension.FromPixels(30f) };
        _selectedNPCBackPanel.Append(_propertyTaxText);

        GenerateChangeButtons(
            _propertyTaxChangeButtons = new UIBetterImageButton[3, 2] ,
            new Vector2(SelectedNPCBackPanelWidth / 2f - TaxChangeButtonsSideLength - 38f, 48f),
            CoinDisplayPadding,
            Item.gold,
            true
        );

        _propertyTaxDisplay = new UICoinDisplay(
            _propertyTaxValue,
            [
                new UICoinDisplay.CoinDrawStyle (UICoinDisplay.CoinDrawCondition.Default),
                new UICoinDisplay.CoinDrawStyle (UICoinDisplay.CoinDrawCondition.Default),
                new UICoinDisplay.CoinDrawStyle (UICoinDisplay.CoinDrawCondition.Default),
                new UICoinDisplay.CoinDrawStyle (UICoinDisplay.CoinDrawCondition.DoNotDraw)
            ],
            CoinDisplayPadding
        ) { HAlign = 0.5f, Top = StyleDimension.FromPixels(75f) };
        _selectedNPCBackPanel.Append(_propertyTaxDisplay);

        _salesTaxText = new UIBetterText("Sales Tax", 0.9f) { HAlign = 0.5f, Top = StyleDimension.FromPixels(140f) };
        _selectedNPCBackPanel.Append(_salesTaxText);

        GenerateChangeButtons(
            _salesTaxChangeButtons = new UIBetterImageButton[2, 2],
            new Vector2(SelectedNPCBackPanelWidth / 2f - TaxChangeButtonsSideLength - 18f, 158f),
            CoinDisplayPadding,
            Item.silver,
            false
        );

        _salesTaxDisplay = new UICoinDisplay(
            _salesTaxValue,
            [
                new UICoinDisplay.CoinDrawStyle(UICoinDisplay.CoinDrawCondition.Default),
                new UICoinDisplay.CoinDrawStyle(UICoinDisplay.CoinDrawCondition.Default),
                new UICoinDisplay.CoinDrawStyle(UICoinDisplay.CoinDrawCondition.DoNotDraw),
                new UICoinDisplay.CoinDrawStyle(UICoinDisplay.CoinDrawCondition.DoNotDraw)
            ],
            CoinDisplayPadding
        ) { HAlign = 0.5f, Top = StyleDimension.FromPixels(185f) };
        _selectedNPCBackPanel.Append(_salesTaxDisplay);

        _helpIconPanel = new UIPanel(vanillaPanelBackground, gradientPanelBorder) {
            BackgroundColor = LWMUtils.LWMCustomUIPanelBackgroundColor,
            BorderColor = Color.White,
            Width = StyleDimension.FromPixels(HelpPanelSideLength),
            Height = StyleDimension.FromPixels(HelpPanelSideLength),
            Left = StyleDimension.FromPixels(-_backPanel.PaddingLeft - HelpPanelSideLength - PaddingBetweenPanels),
            Top = StyleDimension.FromPixels(-_backPanel.PaddingLeft + 2)
        };
        _helpIconPanel.SetPadding(0f);
        _backPanel.Append(_helpIconPanel);

        _helpIconTooltipZone = new UITooltipElement("UI.TaxSheet.Help".Localized()) { Width = StyleDimension.Fill, Height = StyleDimension.Fill };
        _helpIconPanel.Append(_helpIconTooltipZone);

        _helpIcon = new UIImage(TextureAssets.NpcHead[NPCHeadID.HousingQuery]) { VAlign = 0.5f, HAlign = 0.5f, ImageScale = 0.75f };
        _helpIconPanel.Append(_helpIcon);

        return;

        void GenerateChangeButtons(UIBetterImageButton[,] buttonArray, Vector2 drawPos, float coinPadding, long highestCoinValue, bool propertyTax) {
            long buttonChangeValue = highestCoinValue;
            for (int i = 0; i < buttonArray.GetLength(1); i++) {
                float xDrawPos = drawPos.X;
                for (int j = 0; j < buttonArray.GetLength(0); j++) {
                    long capturedButtonChangeValue = buttonChangeValue;

                    UIBetterImageButton button = new (changeButtonTextures[i]) {
                        Width = StyleDimension.FromPixels(TaxChangeButtonsSideLength),
                        Height = StyleDimension.FromPixels(TaxChangeButtonsSideLength),
                        Left = StyleDimension.FromPixels(xDrawPos),
                        Top = StyleDimension.FromPixels(drawPos.Y)
                    };

                    // Surely a POINTER would be a great idea here! (This is a joke).
                    // No capturing ref's in lambdas. Hard-coded and scuffed, but it is what it is
                    button.ProperOnClick += (_, _) => {
                        ref long taxValue = ref _propertyTaxValue;
                        if (!propertyTax) {
                            taxValue = ref _salesTaxValue;
                        }

                        taxValue = Utils.Clamp(taxValue + capturedButtonChangeValue, 0, long.MaxValue);
                    };

                    buttonArray[j, i] = button;
                    _selectedNPCBackPanel.Append(button);

                    xDrawPos += coinPadding;
                    buttonChangeValue /= 100;
                }

                drawPos.Y += 56f;
                buttonChangeValue = -highestCoinValue;
            }
        }
    }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);

        RemoveAllChildren();
        OnInitialize();

        if (_backPanel.IsMouseHovering) {
            Main.LocalPlayer.mouseInterface = true;
        }
    }

    public void SetStateToNPC(NPC npc) {
        NPCBeingTalkedTo = npc;

        _npcGridScrollBar.ViewPosition = 0f;
        _npcGrid.Clear();
        PopulateNPCGrid();
    }

    private void PopulateNPCGrid() {
        foreach (NPC npc in Main.ActiveNPCs) {
            int headIndex;
            if (!TownGlobalNPC.EntityIsValidTownNPC(npc, true) || (headIndex = TownNPCProfiles.GetHeadIndexSafe(npc)) < 0) {
                continue;
            }

            UITooltipElement headTooltipZone = new(npc.GivenName) { Width = StyleDimension.FromPixels(40), Height = StyleDimension.FromPixels(40) };
            headTooltipZone.Recalculate();

            UIPanel npcHeadPanel = new() { Width = StyleDimension.Fill, Height = StyleDimension.Fill, IgnoresMouseInteraction = true };
            npcHeadPanel.SetPadding(0);
            headTooltipZone.Append(npcHeadPanel);

            UIImage npcHead = new (TextureAssets.NpcHead[headIndex]) { HAlign = 0.5f, VAlign = 0.5f, IgnoresMouseInteraction = true };
            npcHeadPanel.Append(npcHead);

            _npcGrid.Add(headTooltipZone);
        }
    }
}