using System.Collections.Generic;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems.UI;
using LivingWorldMod.Globals.UIElements;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

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

            _npcHeadPanel = new UIPanel { Width = StyleDimension.Fill, Height = StyleDimension.Fill, IgnoresMouseInteraction = true};
            _npcHeadPanel.SetPadding(0);
            _headTooltipZone.Append(_npcHeadPanel);

            UIImage npcHead = new(headAsset) { HAlign = 0.5f, VAlign = 0.5f, IgnoresMouseInteraction = true};
            _npcHeadPanel.Append(npcHead);

            this.npcType = npcType;
            this.indexInGrid = indexInGrid;

            _headTooltipZone.OnLeftClick += (_, _) => {
                TaxSheetUIState state = TaxesUISystem.Instance.correspondingUIState;
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
            };

            _headTooltipZone.OnMouseOver += (_, _) => SoundEngine.PlaySound(SoundID.MenuTick);
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

    private UIVisibilityElement _selectedNPCVisibilityElement;
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

    private int? _selectedNPCGridIndex;

    /// <remarks>
    ///     This field needing to exist is symptomatic of big issues that very <i>old</i> LWM code causes. The short of it is that we activate and load our UIs during mod loading, instead of "on-demand."
    ///     This causes UI states to have incorrect scale values, as while loading, <see cref="Main.UIScale" /> is different than in-game. As such, anything relying on position based on the
    ///     screen size will get tripped up, and this is the first time it's been causing problems. This field will cause a recalculation to occur to band-aid fix the problem.
    /// </remarks>
    // TODO: Fix underlying UI loading code
    private bool _hasDoubleInitialized;

    public NPC NPCBeingTalkedTo {
        get;
        private set;
    }

    private SelectableNPCHeadElement SelectedNPCElement => _selectedNPCGridIndex is { } index ? (SelectableNPCHeadElement)_npcGrid._items[index] : null;

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

        _selectedNPCVisibilityElement = new UIVisibilityElement {
            VAlign = 0.5f,
            Left = StyleDimension.FromPixels(_backPanel.GetDimensions().X + BackPanelSideLength + PaddingBetweenPanels),
            Width = StyleDimension.FromPixels(SelectedNPCBackPanelWidth),
            Height = StyleDimension.FromPixels(BackPanelSideLength + 110f)
        };
        Append(_selectedNPCVisibilityElement);

        _selectedNPCBackPanel = new UIPanel(vanillaPanelBackground, gradientPanelBorder) {
            BackgroundColor = LWMUtils.LWMCustomUIPanelBackgroundColor, BorderColor = Color.White, Width = StyleDimension.Fill, Height = StyleDimension.Fill
        };
        _selectedNPCVisibilityElement.Append(_selectedNPCBackPanel);

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
        ) { HAlign = 0.5f, Top = StyleDimension.FromPixels(80f) };
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
        ) { HAlign = 0.5f, Top = StyleDimension.FromPixels(190f) };
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

        if (_backPanel.IsMouseHovering) {
            Main.LocalPlayer.mouseInterface = true;
        }
    }

    public void SetStateToNPC(NPC npc) {
        NPCBeingTalkedTo = npc;

        if (!_hasDoubleInitialized) {
            for (int i = 0; i < 2; i++) {
                Recalculate();

                RemoveAllChildren();
                OnInitialize();
            }

            _hasDoubleInitialized = true;
        }

        _npcGridScrollBar.ViewPosition = 0f;
        _npcGrid.Clear();
        _selectedNPCVisibilityElement.SetVisibility(false);

        PopulateNPCGrid();
    }

    private void RefreshSelectedNPCDisplay() {
        if (_selectedNPCGridIndex is null) {
            _selectedNPCVisibilityElement.SetVisibility(false);
            return;
        }

        _selectedNPCVisibilityElement.SetVisibility(true);
        _selectedNPCName.SetText(LWMUtils.GetNPCTypeNameOrIDName(SelectedNPCElement.npcType));
    }

    private void PopulateNPCGrid() {
        HashSet<int> addedNPCTypes = [];
        int gridIndex = 0;
        foreach (NPC npc in Main.ActiveNPCs) {
            int headIndex;
            if (addedNPCTypes.Contains(npc.type) || !TownGlobalNPC.EntityIsValidTownNPC(npc, true) || (headIndex = TownNPCProfiles.GetHeadIndexSafe(npc)) < 0) {
                continue;
            }

            _npcGrid.Add(new SelectableNPCHeadElement(npc.GivenName, TextureAssets.NpcHead[headIndex], npc.type, gridIndex++));
            addedNPCTypes.Add(npc.type);
        }
    }
}