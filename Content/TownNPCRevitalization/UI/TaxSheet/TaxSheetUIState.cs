using System;
using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.PacketHandlers;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Players;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems.UI;
using LivingWorldMod.Globals.UIElements;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using CoinDrawStyle = LivingWorldMod.Globals.UIElements.UICoinDisplay.CoinDrawStyle;
using CoinDrawCondition = LivingWorldMod.Globals.UIElements.UICoinDisplay.CoinDrawCondition;

namespace LivingWorldMod.Content.TownNPCRevitalization.UI.TaxSheet;

/// <summary>
///     UIState for the Tax Sheet UI added to the Tax Collector.
/// </summary>
public class TaxSheetUIState : UIState {
    private sealed class SelectableNPCHeadElement : UIElement {
        public readonly int npcType;
        public readonly int indexInGrid;

        private readonly UITooltipElement _headTooltipZone;
        private readonly UIPanel _npcHeadPanel;

        public SelectableNPCHeadElement(string npcName, Asset<Texture2D> headAsset, int npcType, int indexInGrid) {
            Width.Set(40f, 0);
            Height.Set(40f, 0);

            _headTooltipZone = new UITooltipElement(npcName) { Width = StyleDimension.Fill, Height = StyleDimension.Fill };
            Append(_headTooltipZone);

            _npcHeadPanel = new UIPanel { Width = StyleDimension.Fill, Height = StyleDimension.Fill, IgnoresMouseInteraction = true };
            _npcHeadPanel.SetPadding(0);
            _headTooltipZone.Append(_npcHeadPanel);

            UIImage npcHead = new(headAsset) { HAlign = 0.5f, VAlign = 0.5f, IgnoresMouseInteraction = true };
            _npcHeadPanel.Append(npcHead);

            this.npcType = npcType;
            this.indexInGrid = indexInGrid;

            _headTooltipZone.OnLeftClick += OnHeadElementLeftClick;
            _headTooltipZone.OnMouseOver += (_, _) => SoundEngine.PlaySound(SoundID.MenuTick);

            return;

            void OnHeadElementLeftClick(UIMouseEvent uiMouseEvent, UIElement uiElement) {
                TaxSheetUIState state = TaxSheetUISystem.Instance.UIState;
                ref int? selectedNPCGridIndex = ref state._selectedNPCGridIndex;

                if (selectedNPCGridIndex == this.indexInGrid) {
                    selectedNPCGridIndex = null;

                    ChangePanelSelectionColors(false);
                    state.RefreshSelectedNPCDisplay();

                    SoundEngine.PlaySound(SoundID.MenuClose);
                    return;
                }

                ChangePanelSelectionColors(true);
                if (selectedNPCGridIndex is { } oldSelectedIndex) {
                    ((SelectableNPCHeadElement)state._npcGrid._items[oldSelectedIndex]).ChangePanelSelectionColors(false);
                }

                selectedNPCGridIndex = indexInGrid;
                state.RefreshSelectedNPCDisplay();

                SoundEngine.PlaySound(SoundID.MenuOpen);
            }
        }

        public void ChangePanelSelectionColors(bool isSelected) {
            if (isSelected) {
                _npcHeadPanel.BackgroundColor = LWMUtils.YellowErrorTextColor * 0.7f;
                _npcHeadPanel.BorderColor = Color.Yellow;
            }
            else {
                _npcHeadPanel.BackgroundColor = LWMUtils.UIPanelBackgroundColor;
                _npcHeadPanel.BorderColor = Color.Black;
            }
        }
    }

    private const float BackPanelSideLength = 200f;
    private const float LeftSidePanelsSideLength = 32f;
    private const float PaddingBetweenPanels = 4f;
    private const float SelectedNPCBackPanelWidth = 130f;
    private const float CoinDisplayPadding = 40f;
    private const float ButtonsSideLength = 30f;
    private const float ChangeButtonXDrawPos = SelectedNPCBackPanelWidth / 2f - ButtonsSideLength - 34f;
    private const float DefaultSalesTaxButtonChangeAmount = 0.01f;

    private NPCTaxValues _startingTaxValues;

    // "Temp" because these are only used client-side for the UI, and will not be exported to the actual Tax System unless the player submits them
    private int _tempPropertyTaxValue;
    private float _tempSalesTaxValue;

    private UIPanel _backPanel;
    private UIModifiedText _titleText;
    private UIPanel _npcGridSubPanel;
    private UIBetterScrollbar _npcGridScrollBar;
    private UIGrid _npcGrid;

    private UIVisibilityElement _selectedNPCVisibilityElement;
    private UIPanel _selectedNPCBackPanel;
    private UIModifiedText _selectedNPCName;

    private UIModifiedText _propertyTaxText;
    private UICoinDisplay _propertyTaxDisplay;
    private UIBetterImageButton[,] _propertyTaxChangeButtons;

    private UIVisibilityElement _salesTaxVisibilityElement;
    private UIModifiedText _salesTaxText;
    private UIModifiedText _salesTaxDisplay;
    private UIBetterImageButton[] _salesTaxChangeButtons;

    private UIVisibilityElement _confirmationButtonsVisibilityElement;
    private UIBetterImageButton _denyNewTaxesButton;
    private UITooltipElement _denyNewTaxesButtonTooltip;
    private UIBetterImageButton _acceptNewTaxesButton;
    private UITooltipElement _acceptNewTaxesButtonTooltip;

    private UIPanel _helpIconPanel;
    private UITooltipElement _helpIconTooltipZone;
    private UIImage _helpIcon;

    private UIPanel _directDepositPanel;
    private UITooltipElement _directDepositTooltipZone;
    private UIImage _directDepositIcon;

    private int? _selectedNPCGridIndex;

    public NPC NPCBeingTalkedTo {
        get;
        private set;
    }

    private int ButtonChangeAmountMultiplier  {
        get {
            // Oh no! A variable being set based on two different, but not mutually exclusive, conditions! Pack it up I'm a bad programmer 
            int multiplier = 1;
            if (Main.keyState.PressingShift()) {
                multiplier = 5;
            }

            if (Main.keyState.PressingControl()) {
                multiplier = 25;
            }

            return multiplier;
        }
    }

    private SelectableNPCHeadElement SelectedNPCElement => _selectedNPCGridIndex is { } index ? (SelectableNPCHeadElement)_npcGrid._items[index] : null;

    private static void PlayTickSound(UIMouseEvent evt, UIElement listeningElement) {
        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    public override void OnInitialize() {
        Asset<Texture2D> vanillaPanelBackground = Main.Assets.Request<Texture2D>("Images/UI/PanelBackground");
        Asset<Texture2D> gradientPanelBorder = ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Elements/GradientPanelBorder");
        Asset<Texture2D> shadowedPanelBorder = ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Elements/ShadowedPanelBorder");

        InitializeBackPanel(vanillaPanelBackground, gradientPanelBorder, shadowedPanelBorder);
        InitializeSelectedNPCPanel(vanillaPanelBackground, gradientPanelBorder);
        InitializeLeftSidePanels(vanillaPanelBackground, gradientPanelBorder);
    }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);

        if (_backPanel.IsMouseHovering || (_selectedNPCVisibilityElement.IsMouseHovering && _selectedNPCVisibilityElement.IsVisible)) {
            Main.LocalPlayer.mouseInterface = true;
        }

        _confirmationButtonsVisibilityElement.SetVisibility(
            _selectedNPCVisibilityElement.IsVisible && (_startingTaxValues.PropertyTax != _tempPropertyTaxValue || Math.Abs(_startingTaxValues.SalesTax - _tempSalesTaxValue) > 0.00005f)
        );
    }

    public void SetStateToNPC(NPC npc) {
        NPCBeingTalkedTo = npc;

        RefreshStateWithCurrentNPC();
    }

    public void RefreshStateWithCurrentNPC() {
        _selectedNPCGridIndex = null;
        _npcGridScrollBar.ViewPosition = 0f;
        _npcGrid.Clear();
        _selectedNPCVisibilityElement.SetVisibility(false);

        PopulateNPCGrid();
    }

    private void InitializeBackPanel(Asset<Texture2D> vanillaPanelBackground, Asset<Texture2D> gradientPanelBorder, Asset<Texture2D> shadowedPanelBorder) {
        _backPanel = new UIPanel(vanillaPanelBackground, gradientPanelBorder) {
            BackgroundColor = LWMUtils.LWMCustomUIPanelBackgroundColor,
            BorderColor = Color.White,
            HAlign = 0.5f,
            VAlign = 0.5f,
            Width = StyleDimension.FromPixels(BackPanelSideLength),
            Height = StyleDimension.FromPixels(BackPanelSideLength)
        };
        Append(_backPanel);

        _titleText = new UIModifiedText("UI.TaxSheet.Title".Localized(), 1.25f) { HAlign = 0.5f };
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
    }

    private void InitializeSelectedNPCPanel(Asset<Texture2D> vanillaPanelBackground, Asset<Texture2D> gradientPanelBorder) {
        Asset<Texture2D>[] changeButtonTextures = [
            ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/ButtonIcons/PlusButton"), ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/ButtonIcons/MinusButton")
        ];

        _selectedNPCVisibilityElement = new UIVisibilityElement {
            VAlign = 0.5f,
            Left = StyleDimension.FromPixels(_backPanel.GetDimensions().X + BackPanelSideLength + PaddingBetweenPanels),
            Width = StyleDimension.FromPixels(SelectedNPCBackPanelWidth),
            Height = StyleDimension.FromPixels(BackPanelSideLength + 80f)
        };
        Append(_selectedNPCVisibilityElement);

        _selectedNPCBackPanel = new UIPanel(vanillaPanelBackground, gradientPanelBorder) {
            BackgroundColor = LWMUtils.LWMCustomUIPanelBackgroundColor,
            BorderColor = Color.White,
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill,
            PaddingLeft = 8f,
            PaddingRight = 8f
        };
        _selectedNPCVisibilityElement.Append(_selectedNPCBackPanel);

        _selectedNPCName = new UIModifiedText("Guide", 1.25f) { HAlign = 0.5f, HorizontalTextConstraint = SelectedNPCBackPanelWidth - PaddingBetweenPanels * 2f };
        _selectedNPCBackPanel.Append(_selectedNPCName);

        _propertyTaxText = new UIModifiedText("Property Tax", 0.9f) { HAlign = 0.5f, Top = StyleDimension.FromPixels(30f) };
        _selectedNPCBackPanel.Append(_propertyTaxText);

        _propertyTaxDisplay = new UICoinDisplay(
            _tempPropertyTaxValue,
            [
                new CoinDrawStyle (CoinDrawCondition.Default),
                new CoinDrawStyle (CoinDrawCondition.Default),
                new CoinDrawStyle (CoinDrawCondition.Default),
                new CoinDrawStyle (CoinDrawCondition.DoNotDraw)
            ],
            CoinDisplayPadding
        ) { HAlign = 0.5f, Top = StyleDimension.FromPixels(80f) };
        _selectedNPCBackPanel.Append(_propertyTaxDisplay);

        GenerateChangeButtons(
            _propertyTaxChangeButtons = new UIBetterImageButton[3, 2],
            new Vector2(ChangeButtonXDrawPos, 48f),
            CoinDisplayPadding,
            Item.gold,
            _propertyTaxDisplay
        );

        _salesTaxVisibilityElement = new UIVisibilityElement { Width = StyleDimension.Fill, Height = StyleDimension.FromPixels(50f), Top = StyleDimension.FromPixels(140f) };
        _selectedNPCBackPanel.Append(_salesTaxVisibilityElement);

        _salesTaxText = new UIModifiedText("Sales Tax", 0.9f) { HAlign = 0.5f };
        _salesTaxVisibilityElement.Append(_salesTaxText);

        _salesTaxDisplay = new UIModifiedText("0%", 0.5f, true) { HAlign = 0.5f, Top = StyleDimension.FromPixels(25f) };
        _salesTaxVisibilityElement.Append(_salesTaxDisplay);

        _salesTaxChangeButtons = new UIBetterImageButton[2];
        for (int i = 0; i < 2; i++) {
            float changeAmount = DefaultSalesTaxButtonChangeAmount * (i == 0 ? 1 : -1);

            UIBetterImageButton button = new (changeButtonTextures[i]) {
                Width = StyleDimension.FromPixels(ButtonsSideLength), Height = StyleDimension.FromPixels(ButtonsSideLength), HAlign = i, Top = StyleDimension.FromPixels(18f)
            };
            button.OnLeftClick += (_, _) => {
                float oldTalesTax = _tempSalesTaxValue;
                _tempSalesTaxValue = Utils.Clamp(_tempSalesTaxValue + changeAmount * ButtonChangeAmountMultiplier, 0f, TaxesSystem.MaxSalesTax);
                _salesTaxDisplay.DesiredText = $"{(int)Math.Round(_tempSalesTaxValue * 100)}%";

                SoundEngine.PlaySound(Math.Abs(oldTalesTax - _tempSalesTaxValue) > 0.000005 ? SoundID.Coins : SoundID.Tink);
            };
            _salesTaxVisibilityElement.Append(button);

            _salesTaxChangeButtons[i] = button;
        }

        _confirmationButtonsVisibilityElement = new UIVisibilityElement { HAlign = 0.5f, VAlign = 1f, Width = StyleDimension.Fill, Height = StyleDimension.FromPixels(ButtonsSideLength) };
        _selectedNPCBackPanel.Append(_confirmationButtonsVisibilityElement);

        _denyNewTaxesButton = new UIBetterImageButton(ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/ButtonIcons/DenyButton")) {
            Width = StyleDimension.FromPixels(ButtonsSideLength), Height = StyleDimension.FromPixels(ButtonsSideLength)
        };
        _denyNewTaxesButton.OnLeftClick += (_, _) =>  {
            RefreshTaxValueDisplays(SelectedNPCElement.npcType);

            SoundEngine.PlaySound(SoundID.Tink);
        };
        _confirmationButtonsVisibilityElement.Append(_denyNewTaxesButton);

        _denyNewTaxesButtonTooltip = new UITooltipElement("UI.TaxSheet.DenyButton".Localized()) { Width = StyleDimension.Fill, Height = StyleDimension.Fill };
        _denyNewTaxesButton.Append(_denyNewTaxesButtonTooltip);

        _acceptNewTaxesButton = new UIBetterImageButton(ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/ButtonIcons/AcceptButton")) {
            HAlign = 1f, Width = StyleDimension.FromPixels(ButtonsSideLength), Height = StyleDimension.FromPixels(ButtonsSideLength), Left = StyleDimension.FromPixels(-2f)
        };
        _acceptNewTaxesButton.OnLeftClick += (_, _) => {
            int npcType = SelectedNPCElement.npcType;

            if (Main.netMode == NetmodeID.MultiplayerClient) {
                ModPacket packet = ModContent.GetInstance<TaxesPacketHandler>().GetPacket();

                packet.Write(npcType);
                packet.Write(_tempPropertyTaxValue);
                packet.Write(_tempSalesTaxValue);

                packet.Send();
            }
            else {
                TaxesSystem.Instance.SubmitNewTaxValues(npcType, new NPCTaxValues(_tempPropertyTaxValue, _tempSalesTaxValue));
            }

            SelectedNPCElement.ChangePanelSelectionColors(false);
            _selectedNPCGridIndex = null;
            RefreshSelectedNPCDisplay();

            SoundEngine.PlaySound(SoundID.Coins);
        };
        _confirmationButtonsVisibilityElement.Append(_acceptNewTaxesButton);

        _acceptNewTaxesButtonTooltip = new UITooltipElement("UI.TaxSheet.AcceptButton".Localized()) { Width = StyleDimension.Fill, Height = StyleDimension.Fill };
        _acceptNewTaxesButton.Append(_acceptNewTaxesButtonTooltip);

        return;

        void GenerateChangeButtons(UIBetterImageButton[,] buttonArray, Vector2 drawPos, float coinPadding, int highestCoinValue, UICoinDisplay coinDisplay) {
            int buttonChangeValue = highestCoinValue;
            for (int i = 0; i < buttonArray.GetLength(1); i++) {
                float xDrawPos = drawPos.X;
                for (int j = 0; j < buttonArray.GetLength(0); j++) {
                    int capturedButtonChangeValue = buttonChangeValue;

                    UIBetterImageButton button = new (changeButtonTextures[i]) {
                        Width = StyleDimension.FromPixels(ButtonsSideLength),
                        Height = StyleDimension.FromPixels(ButtonsSideLength),
                        Left = StyleDimension.FromPixels(xDrawPos),
                        Top = StyleDimension.FromPixels(drawPos.Y)
                    };

                    button.OnLeftClick += (_, _) => {
                        int oldPropertyValue = _tempPropertyTaxValue;

                        _tempPropertyTaxValue = Utils.Clamp(_tempPropertyTaxValue + capturedButtonChangeValue * ButtonChangeAmountMultiplier, 0, TaxesSystem.MaxPropertyTax);
                        coinDisplay.SetNewCoinValues(_tempPropertyTaxValue);

                        SoundEngine.PlaySound(oldPropertyValue != _tempPropertyTaxValue ? SoundID.Coins : SoundID.Tink);
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

    private void InitializeLeftSidePanels(Asset<Texture2D> vanillaPanelBackground, Asset<Texture2D> gradientPanelBorder) {
        CalculatedStyle backPanelDimensions = _backPanel.GetDimensions();
        float leftPanelXPos = backPanelDimensions.X - LeftSidePanelsSideLength - PaddingBetweenPanels;

        _helpIconTooltipZone = new UITooltipElement("UI.TaxSheet.Help".Localized()) {
            Width = StyleDimension.FromPixels(LeftSidePanelsSideLength),
            Height = StyleDimension.FromPixels(LeftSidePanelsSideLength),
            Left = StyleDimension.FromPixels(leftPanelXPos),
            Top = StyleDimension.FromPixels(backPanelDimensions.Y)
        };
        _helpIconTooltipZone.OnMouseOver += PlayTickSound;
        Append(_helpIconTooltipZone);

        _helpIconPanel = new UIPanel(vanillaPanelBackground, gradientPanelBorder) {
            BackgroundColor = LWMUtils.LWMCustomUIPanelBackgroundColor, BorderColor = Color.White, Width = StyleDimension.Fill, Height = StyleDimension.Fill
        };
        _helpIconPanel.SetPadding(0f);
        _helpIconTooltipZone.Append(_helpIconPanel);

        _helpIcon = new UIImage(TextureAssets.NpcHead[NPCHeadID.HousingQuery]) { VAlign = 0.5f, HAlign = 0.5f, ImageScale = 0.75f };
        _helpIconTooltipZone.Append(_helpIcon);

        _directDepositTooltipZone = new UITooltipElement("UI.TaxSheet.EnableDirectDeposit".Localized()) {
            Width = StyleDimension.FromPixels(LeftSidePanelsSideLength),
            Height = StyleDimension.FromPixels(LeftSidePanelsSideLength),
            Left = StyleDimension.FromPixels(leftPanelXPos),
            Top = StyleDimension.FromPixels(backPanelDimensions.Y + LeftSidePanelsSideLength + 4f)
        };
        _directDepositTooltipZone.OnMouseOver += PlayTickSound;
        _directDepositTooltipZone.OnLeftClick += (_, _) => {
            TaxesPlayer taxesPlayer = Main.LocalPlayer.GetModPlayer<TaxesPlayer>();

            RefreshDirectDepositButton(taxesPlayer.directDeposit = !taxesPlayer.directDeposit, true);
        };
        Append(_directDepositTooltipZone);

        _directDepositPanel = new UIPanel(vanillaPanelBackground, gradientPanelBorder) {
            BackgroundColor = LWMUtils.LWMCustomUIPanelBackgroundColor, BorderColor = Color.White, Width = StyleDimension.Fill, Height = StyleDimension.Fill
        };
        _directDepositTooltipZone.Append(_directDepositPanel);

        _directDepositIcon = new UIImage(TextureAssets.Item[ItemID.PiggyBank]) { VAlign = 0.5f, HAlign = 0.5f, ImageScale = 0.75f };
        _directDepositTooltipZone.Append(_directDepositIcon);
    }

    private void RefreshTaxValueDisplays(int npcType) {
        NPCTaxValues selectedTaxValues = TaxesSystem.Instance.GetTaxValuesOrDefault(npcType);
        _startingTaxValues = selectedTaxValues;
        _tempPropertyTaxValue = selectedTaxValues.PropertyTax;
        _tempSalesTaxValue = selectedTaxValues.SalesTax;

        _propertyTaxDisplay.SetNewCoinValues(_tempPropertyTaxValue);
        _salesTaxDisplay.DesiredText = $"{(int)Math.Round(_tempSalesTaxValue * 100)}%";
    }

    private void RefreshSelectedNPCDisplay() {
        if (_selectedNPCGridIndex is null) {
            _selectedNPCVisibilityElement.SetVisibility(false);
            return;
        }

        int selectedNPCType = SelectedNPCElement.npcType;
        RefreshTaxValueDisplays(selectedNPCType);
        RefreshDirectDepositButton(Main.LocalPlayer.GetModPlayer<TaxesPlayer>().directDeposit);

        _selectedNPCName.DesiredText = Lang.GetNPCName(selectedNPCType).Value;
        _selectedNPCVisibilityElement.SetVisibility(true);

        _salesTaxVisibilityElement.SetVisibility(
            NPCShopDatabase.TryGetNPCShop(NPCShopDatabase.GetShopName(selectedNPCType), out AbstractNPCShop npcShop)
            && npcShop.ActiveEntries.Any(entry => entry.Item.shopSpecialCurrency == CustomCurrencyID.None)
        );
    }

    private void RefreshDirectDepositButton(bool directDepositEnabled, bool playSound = false) {
        if (directDepositEnabled) {
            _directDepositPanel.BackgroundColor = LWMUtils.YellowErrorTextColor;
            _directDepositPanel.BorderColor = Color.Yellow;
        }
        else {
            _directDepositPanel.BackgroundColor = LWMUtils.LWMCustomUIPanelBackgroundColor;
            _directDepositPanel.BorderColor = Color.White;
        }

        _directDepositTooltipZone.SetText($"UI.TaxSheet.{(directDepositEnabled ? "Disable" : "Enable")}DirectDeposit".Localized());

        if (playSound) {
            SoundEngine.PlaySound(SoundID.Item59);
        }
    }

    private void PopulateNPCGrid() {
        HashSet<int> addedNPCTypes = [];
        int gridIndex = 0;
        foreach (NPC npc in Main.ActiveNPCs) {
            if (addedNPCTypes.Contains(npc.type) || !TaxesSystem.IsNPCValidForTaxes(npc, out int headIndex)) {
                continue;
            }

            _npcGrid.Add(new SelectableNPCHeadElement(npc.GivenName, TextureAssets.NpcHead[headIndex], npc.type, gridIndex++));
            addedNPCTypes.Add(npc.type);
        }
    }
}