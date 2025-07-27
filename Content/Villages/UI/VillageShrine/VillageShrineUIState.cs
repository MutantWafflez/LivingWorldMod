using System;
using LivingWorldMod.Content.Villages.DataStructures.Enums;
using LivingWorldMod.Content.Villages.Globals.PacketHandlers;
using LivingWorldMod.Content.Villages.HarpyVillage.Tiles.Furniture;
using LivingWorldMod.Globals.UIElements;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace LivingWorldMod.Content.Villages.UI.VillageShrine;

/// <summary>
///     UIState that handles the UI for the village shrine for each type of village.
/// </summary>
public class VillageShrineUIState : UIState {
    private const float DefaultUIPanelPadding = 12f;

    private const float BackPanelSideLength = 194f;

    private const float ItemPanelSideLength = 48f;
    private const float AddTakeButtonsHeight = 30f;
    private const float AddRespawnButtonZoneYPos = ItemPanelSideLength + 4f;
    private const float TakeRespawnButtonYPos = AddRespawnButtonZoneYPos + AddTakeButtonsHeight + 4f;

    private const float RespawnTimerZoneXPos = ItemPanelSideLength + 4f;

    private const float RespawnTimerTextYPos = RespawnTimerTextHeight + 12f;
    private const float RespawnTimerZoneWidth = BackPanelSideLength - DefaultUIPanelPadding * 2 - ItemPanelSideLength - 4f;
    private const float RespawnTimerTextHeight = 14.2933331f;

    private const float VanillaButtonIconSideLength = 22f;
    private const float DisplaySlots4IconWidth = 22f;
    private const float DisplaySlots4IconHeight = 24f;
    private const float DisplaySlots7IconWidth = 26f;

    private const float HouseIconYPos = TakeRespawnButtonYPos + AddTakeButtonsHeight + 2f;
    private const float HouseCountTextXPos = DisplaySlots4IconWidth + 4f;
    private const float HouseCountTextYPos = HouseIconYPos + 4f;

    private const float TakenHouseIconXPos = -2f;
    private const float TakenHouseIconYPos = HouseIconYPos + DisplaySlots4IconHeight + 2f;
    private const float TakenHouseCountTextXPos = TakenHouseIconXPos + DisplaySlots7IconWidth + 2f;
    private const float TakenHouseCountTextYPos = TakenHouseIconYPos + 6f;

    private const float ForceShrineSyncButtonZoneXPos = -VanillaButtonIconSideLength - 4;
    private const float PauseVillagerSpawningZoneYPos = -VanillaButtonIconSideLength - 4f;

    private UIPanel _backPanel;

    private UIPanel _itemPanel;

    private UIBetterItemIcon _respawnItemDisplay;

    private UIModifiedText _respawnItemCount;

    private UIVisibilityElement _addRespawnButtonZone;
    private UIPanelButton _addRespawnButton;

    private UIVisibilityElement _takeRespawnButtonZone;
    private UIPanelButton _takeRespawnButton;

    private UIElement _respawnTimerZone;

    private UIModifiedText _respawnTimerHeader;

    private UIModifiedText _respawnTimer;

    private UIImage _houseIcon;

    private UIModifiedText _houseCountText;

    private UIImage _takenHouseIcon;

    private UIModifiedText _takenHouseCountText;

    private UITooltipElement _showVillageRadiusButtonZone;

    private UIBetterImageButton _showVillageRadiusButton;

    private UITooltipElement _forceShrineSyncButtonZone;

    private UIBetterImageButton _forceShrineSyncButton;

    private UITooltipElement _pauseVillagerSpawningButtonZone;

    private UIBetterImageButton _pauseVillagerSpawningButton;

    public VillageShrineEntity CurrentEntity {
        get {
            if (TileEntity.ByPosition.TryGetValue(EntityPosition, out TileEntity value) && value is VillageShrineEntity entity) {
                return entity;
            }

            return null;
        }
    }

    public Point16 EntityPosition {
        get;
        private set;
    }

    public bool ShowVillageRadius {
        get;
        private set;
    }

    public override void OnInitialize() {
        Asset<Texture2D> vanillaPanelBackground = Main.Assets.Request<Texture2D>("Images/UI/PanelBackground");
        Asset<Texture2D> gradientPanelBorder = ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Elements/GradientPanelBorder");
        Asset<Texture2D> shadowedPanelBorder = ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Elements/ShadowedPanelBorder");

        _backPanel = new UIPanel(vanillaPanelBackground, gradientPanelBorder) {
            Width = StyleDimension.FromPixels(BackPanelSideLength), Height = StyleDimension.FromPixels(BackPanelSideLength), BackgroundColor = new Color(59, 97, 203), BorderColor = Color.White
        };
        Append(_backPanel);

        _itemPanel = new UIPanel(vanillaPanelBackground, shadowedPanelBorder) {
            Width = StyleDimension.FromPixels(ItemPanelSideLength),
            Height = StyleDimension.FromPixels(ItemPanelSideLength),
            BackgroundColor = new Color(46, 46, 159),
            BorderColor = new Color(22, 29, 107)
        };
        _itemPanel.SetPadding(0f);
        _backPanel.Append(_itemPanel);

        _respawnItemDisplay = new UIBetterItemIcon(new Item(), ItemPanelSideLength, true) {
            Width = StyleDimension.FromPixels(ItemPanelSideLength), Height = StyleDimension.FromPixels(ItemPanelSideLength), overrideDrawColor = Color.White * 0.45f
        };
        _itemPanel.Append(_respawnItemDisplay);

        _respawnItemCount = new UIModifiedText("0") { horizontalTextConstraint = ItemPanelSideLength, HAlign = 0.5f, VAlign = 0.85f };
        _itemPanel.Append(_respawnItemCount);

        _addRespawnButtonZone = new UIVisibilityElement {
            Height = StyleDimension.FromPixels(AddTakeButtonsHeight), Width = StyleDimension.FromPixels(ItemPanelSideLength), Top = StyleDimension.FromPixels(AddRespawnButtonZoneYPos)
        };
        _backPanel.Append(_addRespawnButtonZone);

        _addRespawnButton = new UIPanelButton(vanillaPanelBackground, gradientPanelBorder, text: "UI.Shrine.AddText".Localized()) {
            BackgroundColor = _backPanel.BackgroundColor,
            BorderColor = Color.White,
            Height = StyleDimension.Fill,
            Width = StyleDimension.Fill,
            preventItemUsageWhileHovering = true
        };
        _addRespawnButton.OnLeftClick += AddRespawnItem;
        _addRespawnButtonZone.Append(_addRespawnButton);

        _takeRespawnButtonZone = new UIVisibilityElement {
            Height = StyleDimension.FromPixels(AddTakeButtonsHeight), Width = StyleDimension.FromPixels(ItemPanelSideLength), Top = StyleDimension.FromPixels(TakeRespawnButtonYPos)
        };
        _backPanel.Append(_takeRespawnButtonZone);

        _takeRespawnButton = new UIPanelButton(vanillaPanelBackground, gradientPanelBorder, text: "UI.Shrine.TakeText".Localized()) {
            BackgroundColor = _backPanel.BackgroundColor,
            BorderColor = Color.White,
            Height = StyleDimension.Fill,
            Width = StyleDimension.Fill,
            preventItemUsageWhileHovering = true
        };
        _takeRespawnButton.OnLeftClick += TakeRespawnItem;
        _takeRespawnButtonZone.Append(_takeRespawnButton);

        _respawnTimerZone = new UIElement {
            Left = StyleDimension.FromPixels(RespawnTimerZoneXPos), Width = StyleDimension.FromPixels(RespawnTimerZoneWidth), Height = StyleDimension.FromPixels(ItemPanelSideLength)
        };
        _backPanel.Append(_respawnTimerZone);

        _respawnTimerHeader = new UIModifiedText("UI.Shrine.HarpyCountdown".Localized()) { HAlign = 0.5f, horizontalTextConstraint = RespawnTimerZoneWidth };
        _respawnTimerZone.Append(_respawnTimerHeader);

        _respawnTimer = new UIModifiedText("00:00", 0.67f, true) { HAlign = 0.5f, horizontalTextConstraint = RespawnTimerZoneWidth, Top = StyleDimension.FromPixels(RespawnTimerTextYPos) };
        _respawnTimerZone.Append(_respawnTimer);

        _houseIcon = new UIImage(Main.Assets.Request<Texture2D>("Images/UI/DisplaySlots_4")) { Top = StyleDimension.FromPixels(HouseIconYPos) };
        _backPanel.Append(_houseIcon);

        _houseCountText = new UIModifiedText("0") { Left = StyleDimension.FromPixels(HouseCountTextXPos), Top = StyleDimension.FromPixels(HouseCountTextYPos) };
        _backPanel.Append(_houseCountText);

        _takenHouseIcon = new UIImage(Main.Assets.Request<Texture2D>("Images/UI/DisplaySlots_7")) {
            Left = StyleDimension.FromPixels(TakenHouseIconXPos), Top = StyleDimension.FromPixels(TakenHouseIconYPos)
        };
        _backPanel.Append(_takenHouseIcon);

        _takenHouseCountText = new UIModifiedText("0") { Left = StyleDimension.FromPixels(TakenHouseCountTextXPos), Top = StyleDimension.FromPixels(TakenHouseCountTextYPos) };
        _backPanel.Append(_takenHouseCountText);

        _showVillageRadiusButtonZone = new UITooltipElement("UI.Shrine.EnableVillageRadius".Localized()) {
            HAlign = 1f, VAlign = 1f, Width = StyleDimension.FromPixels(VanillaButtonIconSideLength), Height = StyleDimension.FromPixels(VanillaButtonIconSideLength)
        };
        _backPanel.Append(_showVillageRadiusButtonZone);

        _showVillageRadiusButton = new UIBetterImageButton(Main.Assets.Request<Texture2D>("Images/UI/ButtonFavoriteInactive")) { Width = StyleDimension.Fill, Height = StyleDimension.Fill };
        _showVillageRadiusButton.OnLeftClick += ClickVillagerRadiusButton;
        _showVillageRadiusButtonZone.Append(_showVillageRadiusButton);

        _forceShrineSyncButtonZone = new UITooltipElement("UI.Shrine.ForceShrineButtonText".Localized()) {
            HAlign = 1f,
            VAlign = 1f,
            Width = StyleDimension.FromPixels(VanillaButtonIconSideLength),
            Height = StyleDimension.FromPixels(VanillaButtonIconSideLength),
            Left = StyleDimension.FromPixels(ForceShrineSyncButtonZoneXPos)
        };
        _backPanel.Append(_forceShrineSyncButtonZone);

        _forceShrineSyncButton = new UIBetterImageButton(Main.Assets.Request<Texture2D>("Images/UI/ButtonCloudActive")) { Width = StyleDimension.Fill, Height = StyleDimension.Fill };
        _forceShrineSyncButton.OnLeftClick += ClickShrineSyncButton;
        _forceShrineSyncButtonZone.Append(_forceShrineSyncButton);

        _pauseVillagerSpawningButtonZone = new UITooltipElement("UI.Shrine.PauseVillagerSpawning".Localized()) {
            HAlign = 1f,
            VAlign = 1f,
            Width = StyleDimension.FromPixels(VanillaButtonIconSideLength),
            Height = StyleDimension.FromPixels(VanillaButtonIconSideLength),
            Top = StyleDimension.FromPixels(PauseVillagerSpawningZoneYPos)
        };
        _backPanel.Append(_pauseVillagerSpawningButtonZone);

        _pauseVillagerSpawningButton = new UIBetterImageButton(Main.Assets.Request<Texture2D>("Images/UI/ButtonDelete")) { Width = StyleDimension.Fill, Height = StyleDimension.Fill };
        _pauseVillagerSpawningButton.OnLeftClick += ClickPauseVillagerSpawning;
        _pauseVillagerSpawningButtonZone.Append(_pauseVillagerSpawningButton);
    }

    public override void Update(GameTime gameTime) {
        VillageShrineEntity currentEntity = CurrentEntity;

        if (Main.netMode == NetmodeID.MultiplayerClient) {
            currentEntity.clientTimer++;
        }

        _respawnTimer.SetText(
            currentEntity.remainingRespawnItems < currentEntity.CurrentValidHouses
                ? new TimeSpan((long)(TimeSpan.TicksPerSecond / (double)60 * (currentEntity.remainingRespawnTime - currentEntity.clientTimer))).ToString(@"mm\:ss")
                : "\u221E"
        );

        base.Update(gameTime);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        CalculatedStyle panelDimensions = _backPanel.GetDimensions();
        VillageShrineEntity currentEntity = CurrentEntity;
        Vector2 centerOfEntity = EntityPosition.ToWorldCoordinates(32f, 0f);

        _backPanel.Left.Set(centerOfEntity.X - panelDimensions.Width / 2f - Main.screenPosition.X, 0f);
        _backPanel.Top.Set(centerOfEntity.Y - panelDimensions.Height - Main.screenPosition.Y, 0f);

        _respawnItemCount.SetText(currentEntity.remainingRespawnItems.ToString());
        _addRespawnButtonZone.SetVisibility(currentEntity.remainingRespawnItems < currentEntity.CurrentValidHouses);
        _takeRespawnButtonZone.SetVisibility(currentEntity.remainingRespawnItems > 0);

        _houseCountText.SetText(currentEntity.CurrentValidHouses.ToString());
        _takenHouseCountText.SetText(currentEntity.CurrentHousedVillagersCount.ToString());
    }

    public void SetVillagerPauseStatus() {
        _pauseVillagerSpawningButton.SetImage(Main.Assets.Request<Texture2D>("Images/UI/Button" + (CurrentEntity.pausedRespawns ? "Play" : "Delete")));
        _pauseVillagerSpawningButtonZone.SetText(("UI.Shrine." + (CurrentEntity.pausedRespawns ? "Resume" : "Pause") + "VillagerSpawning").Localized());
    }

    /// <summary>
    ///     Regenerates this UI state with the new passed in shrine entity.
    /// </summary>
    /// <param name="entityPos"> The position of the new entity to bind to. </param>
    public void RegenState(Point16 entityPos) {
        EntityPosition = entityPos;
        VillagerType shrineType = CurrentEntity.shrineType;

        _respawnItemDisplay.SetItem(LWMUtils.VillagerTypeToRespawnItemType(shrineType));
        _respawnTimerHeader.SetText($"UI.Shrine.{shrineType}Countdown".Localized());
        SetVillagerPauseStatus();
    }

    private void ClickPauseVillagerSpawning(UIMouseEvent evt, UIElement listeningElement) {
        VillageShrineEntity currentEntity = CurrentEntity;
        switch (Main.netMode) {
            case NetmodeID.MultiplayerClient:
                ModPacket packet = ModContent.GetInstance<ShrinePacketHandler>().GetPacket(ShrinePacketHandler.ToggleVillagerRespawning);
                packet.WriteVector2(currentEntity.Position.ToVector2());
                packet.Send();
                break;
            case NetmodeID.SinglePlayer:
                currentEntity.pausedRespawns = !currentEntity.pausedRespawns;
                SetVillagerPauseStatus();
                break;
        }
    }

    private void AddRespawnItem(UIMouseEvent evt, UIElement listeningElement) {
        VillageShrineEntity currentEntity = CurrentEntity;
        int respawnItemType = LWMUtils.VillagerTypeToRespawnItemType(currentEntity.shrineType);
        Player player = Main.LocalPlayer;

        if (Main.netMode == NetmodeID.MultiplayerClient) {
            if (player.HasItem(respawnItemType)) {
                player.inventory[player.FindItem(respawnItemType)].stack--;

                ModPacket packet = ModContent.GetInstance<ShrinePacketHandler>().GetPacket();
                packet.WriteVector2(currentEntity.Position.ToVector2());
                packet.Send();
            }
        }
        else {
            if (player.HasItem(respawnItemType)) {
                player.inventory[player.FindItem(respawnItemType)].stack--;

                currentEntity.remainingRespawnItems++;
            }
        }
    }

    private void TakeRespawnItem(UIMouseEvent evt, UIElement listeningElement) {
        VillageShrineEntity currentEntity = CurrentEntity;
        Player player = Main.LocalPlayer;
        Item respawnItem = new(LWMUtils.VillagerTypeToRespawnItemType(currentEntity.shrineType));

        if (Main.netMode == NetmodeID.MultiplayerClient) {
            if (player.CanAcceptItemIntoInventory(respawnItem)) {
                ModPacket packet = ModContent.GetInstance<ShrinePacketHandler>().GetPacket(ShrinePacketHandler.TakeRespawnItem);
                packet.WriteVector2(currentEntity.Position.ToVector2());
                packet.Send();
            }
        }
        else {
            if (player.CanAcceptItemIntoInventory(respawnItem) && currentEntity.remainingRespawnItems < currentEntity.CurrentValidHouses) {
                player.QuickSpawnItem(new EntitySource_TileEntity(currentEntity), respawnItem);

                currentEntity.remainingRespawnItems--;
            }
        }
    }

    private void ClickVillagerRadiusButton(UIMouseEvent evt, UIElement listeningElement) {
        ShowVillageRadius = !ShowVillageRadius;

        _showVillageRadiusButton.SetImage(Main.Assets.Request<Texture2D>("Images/UI/ButtonFavorite" + (ShowVillageRadius ? "Active" : "Inactive"), AssetRequestMode.ImmediateLoad));
        _showVillageRadiusButtonZone.SetText(("UI.Shrine." + (ShowVillageRadius ? "Disable" : "Enable") + "VillageRadius").Localized());
    }

    private void ClickShrineSyncButton(UIMouseEvent evt, UIElement listeningElement) {
        switch (Main.netMode) {
            case NetmodeID.MultiplayerClient:
                ModPacket packet = ModContent.GetInstance<ShrinePacketHandler>().GetPacket(ShrinePacketHandler.TriggerForceSync);
                packet.WriteVector2(CurrentEntity.Position.ToVector2());
                packet.Send();
                break;
            case NetmodeID.SinglePlayer:
                CurrentEntity.ForceRecalculateAndSync();
                break;
        }
    }
}