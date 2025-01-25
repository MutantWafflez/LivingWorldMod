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
    public UIPanel backPanel;

    public UIPanel itemPanel;

    public UIBetterItemIcon respawnItemDisplay;

    public UIBetterText respawnItemCount;

    public UIPanelButton addRespawnButton;

    public UIPanelButton takeRespawnButton;

    public UIElement respawnTimerZone;

    public UIBetterText respawnTimerHeader;

    public UIBetterText respawnTimer;

    public UIImage houseIcon;

    public UIBetterText houseCountText;

    public UIImage takenHouseIcon;

    public UIBetterText takenHouseCountText;

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

        backPanel = new UIPanel(vanillaPanelBackground, gradientPanelBorder) { BackgroundColor = new Color(59, 97, 203), BorderColor = Color.White };
        backPanel.Width = backPanel.Height = new StyleDimension(194f, 0f);
        Append(backPanel);

        itemPanel = new UIPanel(vanillaPanelBackground, shadowedPanelBorder) { BackgroundColor = new Color(46, 46, 159), BorderColor = new Color(22, 29, 107) };
        itemPanel.Width = itemPanel.Height = new StyleDimension(48f, 0f);
        itemPanel.SetPadding(0f);
        backPanel.Append(itemPanel);

        respawnItemDisplay = new UIBetterItemIcon(new Item(), 48f, true) { overrideDrawColor = Color.White * 0.45f };
        respawnItemDisplay.Width = respawnItemDisplay.Height = itemPanel.Width;
        itemPanel.Append(respawnItemDisplay);

        respawnItemCount = new UIBetterText("0") { horizontalTextConstraint = itemPanel.Width.Pixels, HAlign = 0.5f, VAlign = 0.85f };
        itemPanel.Append(respawnItemCount);

        addRespawnButton = new UIPanelButton(vanillaPanelBackground, gradientPanelBorder, text: "UI.Shrine.AddText".Localized()) {
            BackgroundColor = backPanel.BackgroundColor, BorderColor = Color.White, Width = itemPanel.Width, preventItemUsageWhileHovering = true
        };
        addRespawnButton.Height.Set(30f, 0f);
        addRespawnButton.Top.Set(itemPanel.Height.Pixels + 4f, 0f);
        addRespawnButton.ProperOnClick += AddRespawnItem;
        backPanel.Append(addRespawnButton);

        takeRespawnButton = new UIPanelButton(vanillaPanelBackground, gradientPanelBorder, text: "UI.Shrine.TakeText".Localized()) {
            BackgroundColor = backPanel.BackgroundColor,
            BorderColor = Color.White,
            Width = addRespawnButton.Width,
            Height = addRespawnButton.Height,
            preventItemUsageWhileHovering = true
        };
        takeRespawnButton.Top.Set(addRespawnButton.Top.Pixels + addRespawnButton.Height.Pixels + 4f, 0f);
        takeRespawnButton.ProperOnClick += TakeRespawnItem;
        backPanel.Append(takeRespawnButton);

        respawnTimerZone = new UIElement();
        respawnTimerZone.Left.Set(itemPanel.Width.Pixels + 4f, 0f);
        respawnTimerZone.Width.Set(backPanel.Width.Pixels - backPanel.PaddingLeft - backPanel.PaddingRight - itemPanel.Width.Pixels - 4f, 0f);
        respawnTimerZone.Height.Set(itemPanel.Height.Pixels, 0f);
        backPanel.Append(respawnTimerZone);

        respawnTimerHeader = new UIBetterText("UI.Shrine.HarpyCountdown".Localized()) { HAlign = 0.5f, horizontalTextConstraint = respawnTimerZone.Width.Pixels };
        respawnTimerZone.Append(respawnTimerHeader);

        respawnTimer = new UIBetterText("00:00", 0.67f, true) { HAlign = 0.5f, horizontalTextConstraint = respawnTimerZone.Width.Pixels };
        respawnTimer.Top.Set(respawnTimerHeader.Height.Pixels + 12f, 0f);
        respawnTimerZone.Append(respawnTimer);

        houseIcon = new UIImage(Main.Assets.Request<Texture2D>("Images/UI/DisplaySlots_4", AssetRequestMode.ImmediateLoad));
        houseIcon.Top.Set(takeRespawnButton.Top.Pixels + takeRespawnButton.Height.Pixels + 2f, 0f);
        backPanel.Append(houseIcon);

        houseCountText = new UIBetterText("0");
        houseCountText.Top.Set(houseIcon.Top.Pixels + 4f, 0f);
        houseCountText.Left.Set(houseIcon.Width.Pixels + 4f, 0f);
        backPanel.Append(houseCountText);

        takenHouseIcon = new UIImage(Main.Assets.Request<Texture2D>("Images/UI/DisplaySlots_7", AssetRequestMode.ImmediateLoad));
        takenHouseIcon.Top.Set(houseIcon.Top.Pixels + houseIcon.Height.Pixels + 2f, 0f);
        takenHouseIcon.Left.Set(-2f, 0f);
        backPanel.Append(takenHouseIcon);

        takenHouseCountText = new UIBetterText("0");
        takenHouseCountText.Top.Set(takenHouseIcon.Top.Pixels + 6f, 0f);
        takenHouseCountText.Left.Set(takenHouseIcon.Width.Pixels + 2f, 0f);
        backPanel.Append(takenHouseCountText);

        Asset<Texture2D> showVillageRadiusButtonTexture = Main.Assets.Request<Texture2D>("Images/UI/ButtonFavoriteInactive", AssetRequestMode.ImmediateLoad);
        showVillageRadiusButtonZone = new UITooltipElement("UI.Shrine.EnableVillageRadius".Localized()) {
            HAlign = 1f, VAlign = 1f, Width = StyleDimension.FromPixels(showVillageRadiusButtonTexture.Width()), Height = StyleDimension.FromPixels(showVillageRadiusButtonTexture.Height())
        };
        backPanel.Append(showVillageRadiusButtonZone);

        showVillageRadiusButton = new UIBetterImageButton(showVillageRadiusButtonTexture);
        showVillageRadiusButton.ProperOnClick += ClickVillagerRadiusButton;
        showVillageRadiusButtonZone.Append(showVillageRadiusButton);

        Asset<Texture2D> forceShrineSyncButtonTexture = Main.Assets.Request<Texture2D>("Images/UI/ButtonCloudActive", AssetRequestMode.ImmediateLoad);
        forceShrineSyncButtonZone = new UITooltipElement("UI.Shrine.ForceShrineButtonText".Localized()) {
            HAlign = 1f,
            VAlign = 1f,
            Width = StyleDimension.FromPixels(forceShrineSyncButtonTexture.Width()),
            Height = StyleDimension.FromPixels(forceShrineSyncButtonTexture.Height()),
            Left = StyleDimension.FromPixels(-showVillageRadiusButtonZone.Width.Pixels - 4)
        };
        backPanel.Append(forceShrineSyncButtonZone);

        forceShrineSyncButton = new UIBetterImageButton(forceShrineSyncButtonTexture);
        forceShrineSyncButton.ProperOnClick += ClickShrineSyncButton;
        forceShrineSyncButtonZone.Append(forceShrineSyncButton);

        Asset<Texture2D> pauseVillagerSpawningButtonTexture = Main.Assets.Request<Texture2D>("Images/UI/ButtonPlay", AssetRequestMode.ImmediateLoad);
        pauseVillagerSpawningButtonZone = new UITooltipElement("UI.Shrine.PauseVillagerSpawning".Localized()) {
            HAlign = 1f,
            VAlign = 1f,
            Width = StyleDimension.FromPixels(pauseVillagerSpawningButtonTexture.Width()),
            Height = StyleDimension.FromPixels(pauseVillagerSpawningButtonTexture.Height()),
            Top = StyleDimension.FromPixels(-showVillageRadiusButtonZone.Width.Pixels - 4)
        };
        backPanel.Append(pauseVillagerSpawningButtonZone);

        pauseVillagerSpawningButton = new UIBetterImageButton(pauseVillagerSpawningButtonTexture) { HAlign = 1f, VAlign = 1f };
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
        addRespawnButton.isVisible = currentEntity.remainingRespawnItems < currentEntity.CurrentValidHouses;
        takeRespawnButton.isVisible = currentEntity.remainingRespawnItems > 0;

        houseCountText.SetText(currentEntity.CurrentValidHouses.ToString());
        takenHouseCountText.SetText(currentEntity.CurrentHousedVillagersCount.ToString());
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