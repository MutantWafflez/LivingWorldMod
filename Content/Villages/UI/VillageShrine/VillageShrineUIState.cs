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

    public UIPanel backPanel;

    public UIPanel itemPanel;

    public UIBetterItemIcon respawnItemDisplay;

    public UIModifiedText respawnItemCount;

    public UIVisibilityElement addRespawnButtonZone;
    public UIPanelButton addRespawnButton;

    public UIVisibilityElement takeRespawnButtonZone;
    public UIPanelButton takeRespawnButton;

    public UIElement respawnTimerZone;

    public UIModifiedText respawnTimerHeader;

    public UIModifiedText respawnTimer;

    public UIImage houseIcon;

    public UIModifiedText houseCountText;

    public UIImage takenHouseIcon;

    public UIModifiedText takenHouseCountText;

    public UITooltipElement showVillageRadiusButtonZone;

    public UIBetterImageButton showVillageRadiusButton;

    public UITooltipElement forceShrineSyncButtonZone;

    public UIBetterImageButton forceShrineSyncButton;

    public UITooltipElement pauseVillagerSpawningButtonZone;

    public UIBetterImageButton pauseVillagerSpawningButton;

    private TimeSpan _timeConverter;

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

        backPanel = new UIPanel(vanillaPanelBackground, gradientPanelBorder) {
            Width = StyleDimension.FromPixels(BackPanelSideLength), Height = StyleDimension.FromPixels(BackPanelSideLength), BackgroundColor = new Color(59, 97, 203), BorderColor = Color.White
        };
        Append(backPanel);

        itemPanel = new UIPanel(vanillaPanelBackground, shadowedPanelBorder) {
            Width = StyleDimension.FromPixels(ItemPanelSideLength),
            Height = StyleDimension.FromPixels(ItemPanelSideLength),
            BackgroundColor = new Color(46, 46, 159),
            BorderColor = new Color(22, 29, 107)
        };
        itemPanel.SetPadding(0f);
        backPanel.Append(itemPanel);

        respawnItemDisplay = new UIBetterItemIcon(new Item(), ItemPanelSideLength, true) {
            Width = StyleDimension.FromPixels(ItemPanelSideLength), Height = StyleDimension.FromPixels(ItemPanelSideLength), overrideDrawColor = Color.White * 0.45f
        };
        itemPanel.Append(respawnItemDisplay);

        respawnItemCount = new UIModifiedText("0") { horizontalTextConstraint = ItemPanelSideLength, HAlign = 0.5f, VAlign = 0.85f };
        itemPanel.Append(respawnItemCount);

        addRespawnButtonZone = new UIVisibilityElement {
            Height = StyleDimension.FromPixels(AddTakeButtonsHeight), Width = StyleDimension.FromPixels(ItemPanelSideLength), Top = StyleDimension.FromPixels(AddRespawnButtonZoneYPos)
        };
        backPanel.Append(addRespawnButtonZone);

        addRespawnButton = new UIPanelButton(vanillaPanelBackground, gradientPanelBorder, text: "UI.Shrine.AddText".Localized()) {
            BackgroundColor = backPanel.BackgroundColor,
            BorderColor = Color.White,
            Height = StyleDimension.Fill,
            Width = StyleDimension.Fill,
            preventItemUsageWhileHovering = true
        };
        addRespawnButton.OnLeftClick += AddRespawnItem;
        addRespawnButtonZone.Append(addRespawnButton);

        takeRespawnButtonZone = new UIVisibilityElement {
            Height = StyleDimension.FromPixels(AddTakeButtonsHeight), Width = StyleDimension.FromPixels(ItemPanelSideLength), Top = StyleDimension.FromPixels(TakeRespawnButtonYPos)
        };
        backPanel.Append(takeRespawnButtonZone);

        takeRespawnButton = new UIPanelButton(vanillaPanelBackground, gradientPanelBorder, text: "UI.Shrine.TakeText".Localized()) {
            BackgroundColor = backPanel.BackgroundColor,
            BorderColor = Color.White,
            Height = StyleDimension.Fill,
            Width = StyleDimension.Fill,
            preventItemUsageWhileHovering = true
        };
        takeRespawnButton.OnLeftClick += TakeRespawnItem;
        takeRespawnButtonZone.Append(takeRespawnButton);

        respawnTimerZone = new UIElement {
            Left = StyleDimension.FromPixels(RespawnTimerZoneXPos), Width = StyleDimension.FromPixels(RespawnTimerZoneWidth), Height = StyleDimension.FromPixels(ItemPanelSideLength)
        };
        backPanel.Append(respawnTimerZone);

        respawnTimerHeader = new UIModifiedText("UI.Shrine.HarpyCountdown".Localized()) { HAlign = 0.5f, horizontalTextConstraint = RespawnTimerZoneWidth };
        respawnTimerZone.Append(respawnTimerHeader);

        respawnTimer = new UIModifiedText("00:00", 0.67f, true) { HAlign = 0.5f, horizontalTextConstraint = RespawnTimerZoneWidth, Top = StyleDimension.FromPixels(RespawnTimerTextYPos) };
        respawnTimerZone.Append(respawnTimer);

        houseIcon = new UIImage(Main.Assets.Request<Texture2D>("Images/UI/DisplaySlots_4")) { Top = StyleDimension.FromPixels(HouseIconYPos) };
        backPanel.Append(houseIcon);

        houseCountText = new UIModifiedText("0") { Left = StyleDimension.FromPixels(HouseCountTextXPos), Top = StyleDimension.FromPixels(HouseCountTextYPos) };
        backPanel.Append(houseCountText);

        takenHouseIcon = new UIImage(Main.Assets.Request<Texture2D>("Images/UI/DisplaySlots_7")) {
            Left = StyleDimension.FromPixels(TakenHouseIconXPos), Top = StyleDimension.FromPixels(TakenHouseIconYPos)
        };
        backPanel.Append(takenHouseIcon);

        takenHouseCountText = new UIModifiedText("0") { Left = StyleDimension.FromPixels(TakenHouseCountTextXPos), Top = StyleDimension.FromPixels(TakenHouseCountTextYPos) };
        backPanel.Append(takenHouseCountText);

        showVillageRadiusButtonZone = new UITooltipElement("UI.Shrine.EnableVillageRadius".Localized()) {
            HAlign = 1f, VAlign = 1f, Width = StyleDimension.FromPixels(VanillaButtonIconSideLength), Height = StyleDimension.FromPixels(VanillaButtonIconSideLength)
        };
        backPanel.Append(showVillageRadiusButtonZone);

        showVillageRadiusButton = new UIBetterImageButton(Main.Assets.Request<Texture2D>("Images/UI/ButtonFavoriteInactive")) { Width = StyleDimension.Fill, Height = StyleDimension.Fill };
        showVillageRadiusButton.OnLeftClick += ClickVillagerRadiusButton;
        showVillageRadiusButtonZone.Append(showVillageRadiusButton);

        forceShrineSyncButtonZone = new UITooltipElement("UI.Shrine.ForceShrineButtonText".Localized()) {
            HAlign = 1f,
            VAlign = 1f,
            Width = StyleDimension.FromPixels(VanillaButtonIconSideLength),
            Height = StyleDimension.FromPixels(VanillaButtonIconSideLength),
            Left = StyleDimension.FromPixels(ForceShrineSyncButtonZoneXPos)
        };
        backPanel.Append(forceShrineSyncButtonZone);

        forceShrineSyncButton = new UIBetterImageButton(Main.Assets.Request<Texture2D>("Images/UI/ButtonCloudActive")) { Width = StyleDimension.Fill, Height = StyleDimension.Fill };
        forceShrineSyncButton.OnLeftClick += ClickShrineSyncButton;
        forceShrineSyncButtonZone.Append(forceShrineSyncButton);

        pauseVillagerSpawningButtonZone = new UITooltipElement("UI.Shrine.PauseVillagerSpawning".Localized()) {
            HAlign = 1f,
            VAlign = 1f,
            Width = StyleDimension.FromPixels(VanillaButtonIconSideLength),
            Height = StyleDimension.FromPixels(VanillaButtonIconSideLength),
            Top = StyleDimension.FromPixels(PauseVillagerSpawningZoneYPos)
        };
        backPanel.Append(pauseVillagerSpawningButtonZone);

        pauseVillagerSpawningButton = new UIBetterImageButton(Main.Assets.Request<Texture2D>("Images/UI/ButtonDelete")) { Width = StyleDimension.Fill, Height = StyleDimension.Fill };
        pauseVillagerSpawningButton.OnLeftClick += ClickPauseVillagerSpawning;
        pauseVillagerSpawningButtonZone.Append(pauseVillagerSpawningButton);
    }

    public override void Update(GameTime gameTime) {
        VillageShrineEntity currentEntity = CurrentEntity;

        if (Main.netMode == NetmodeID.MultiplayerClient) {
            currentEntity.clientTimer++;
        }

        if (currentEntity.remainingRespawnItems < currentEntity.CurrentValidHouses) {
            _timeConverter = new TimeSpan((long)(TimeSpan.TicksPerSecond / (double)60 * (currentEntity.remainingRespawnTime - currentEntity.clientTimer)));

            respawnTimer.SetText(_timeConverter.ToString(@"mm\:ss"));
        }
        else {
            respawnTimer.SetText("\u221E");
        }

        base.Update(gameTime);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        CalculatedStyle panelDimensions = backPanel.GetDimensions();
        VillageShrineEntity currentEntity = CurrentEntity;
        Vector2 centerOfEntity = EntityPosition.ToWorldCoordinates(32f, 0f);

        backPanel.Left.Set(centerOfEntity.X - panelDimensions.Width / 2f - Main.screenPosition.X, 0f);
        backPanel.Top.Set(centerOfEntity.Y - panelDimensions.Height - Main.screenPosition.Y, 0f);

        respawnItemCount.SetText(currentEntity.remainingRespawnItems.ToString());
        addRespawnButtonZone.SetVisibility(currentEntity.remainingRespawnItems < currentEntity.CurrentValidHouses);
        takeRespawnButtonZone.SetVisibility(currentEntity.remainingRespawnItems > 0);

        houseCountText.SetText(currentEntity.CurrentValidHouses.ToString());
        takenHouseCountText.SetText(currentEntity.CurrentHousedVillagersCount.ToString());
    }

    public void SetVillagerPauseStatus() {
        pauseVillagerSpawningButton.SetImage(Main.Assets.Request<Texture2D>("Images/UI/Button" + (CurrentEntity.pausedRespawns ? "Play" : "Delete")));
        pauseVillagerSpawningButtonZone.SetText(("UI.Shrine." + (CurrentEntity.pausedRespawns ? "Resume" : "Pause") + "VillagerSpawning").Localized());
    }

    /// <summary>
    ///     Regenerates this UI state with the new passed in shrine entity.
    /// </summary>
    /// <param name="entityPos"> The position of the new entity to bind to. </param>
    public void RegenState(Point16 entityPos) {
        EntityPosition = entityPos;
        VillagerType shrineType = CurrentEntity.shrineType;

        respawnItemDisplay.SetItem(LWMUtils.VillagerTypeToRespawnItemType(shrineType));
        respawnTimerHeader.SetText($"UI.Shrine.{shrineType}Countdown".Localized());
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

        showVillageRadiusButton.SetImage(Main.Assets.Request<Texture2D>("Images/UI/ButtonFavorite" + (ShowVillageRadius ? "Active" : "Inactive"), AssetRequestMode.ImmediateLoad));
        showVillageRadiusButtonZone.SetText(("UI.Shrine." + (ShowVillageRadius ? "Disable" : "Enable") + "VillageRadius").Localized());
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