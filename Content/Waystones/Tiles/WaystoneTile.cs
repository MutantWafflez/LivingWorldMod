using System.IO;
using LivingWorldMod.Content.Waystones.DataStructures.Classes;
using LivingWorldMod.Content.Waystones.DataStructures.Enums;
using LivingWorldMod.Content.Waystones.Globals.PacketHandlers;
using LivingWorldMod.Content.Waystones.Globals.Systems;
using LivingWorldMod.Globals.BaseTypes.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.Map;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.UI;

namespace LivingWorldMod.Content.Waystones.Tiles;

/// <summary>
///     Class for Waystone tiles, which are basically Pylons but in the wild.
/// </summary>
public class WaystoneTile : BasePylon {
    public Asset<Texture2D> waystoneMapIcons;
    public override Color? TileColorOnMap => Color.White;

    public override void Load() {
        waystoneMapIcons = Assets.MapIcons.WaystoneIcons;
    }

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLighted[Type] = true;
        Main.tileSpelunker[Type] = true;

        TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
        TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;
        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.FriendlyFairyCanLureTo[Type] = true;

        TileObjectData.newTile.WaterDeath = false;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.WaterPlacement = LiquidPlacement.Allowed;
        TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.Origin = Point16.Zero;
        TileObjectData.newTile.Height = 3;
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.DrawYOffset = 2;

        WaystoneEntity waystoneEntity = ModContent.GetInstance<WaystoneEntity>();
        TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(waystoneEntity.PlacementPreviewHook_CheckIfCanPlace, 1, 0, true);
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(waystoneEntity.Hook_AfterPlacement, -1, 0, false);

        TileObjectData.addTile(Type);

        TileID.Sets.InteractibleByNPCs[Type] = TileID.Sets.PreventsSandfall[Type] = TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;

        AddToArray(ref TileID.Sets.CountsAsPylon);

        AnimationFrameHeight = 54;
    }

    public override bool CanKillTile(int i, int j, ref bool blockDamaged) => LWM.IsDebug;

    public override bool CanExplode(int i, int j) => LWM.IsDebug;

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

    public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
        Point topLeft = LWMUtils.GetCornerOfMultiTile(Framing.GetTileSafely(i, j), i, j, LWMUtils.CornerType.TopLeft);

        if (LWMUtils.TryFindModEntity(topLeft.X, topLeft.Y, out WaystoneEntity foundEntity) && foundEntity.isActivated) {
            frameYOffset += AnimationFrameHeight;
        }
    }

    //Since these "pylons" aren't a traditional vanilla pylon (with no visual crystal), we override the base implementation to prevent it.
    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) { }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        // Lightly glow while activated
        Point topLeft = LWMUtils.GetCornerOfMultiTile(Framing.GetTileSafely(i, j), i, j, LWMUtils.CornerType.TopLeft);

        if (LWMUtils.TryFindModEntity(topLeft.X, topLeft.Y, out WaystoneEntity foundEntity) && foundEntity.isActivated) {
            Color waystoneColor = foundEntity.WaystoneColor;

            r = waystoneColor.R / 255f;
            g = waystoneColor.G / 255f;
            b = waystoneColor.B / 255f;
        }
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY) {
        ModContent.GetInstance<WaystoneEntity>().Kill(i, j);
    }

    public override bool RightClick(int i, int j) {
        Point topLeft = LWMUtils.GetCornerOfMultiTile(Framing.GetTileSafely(i, j), i, j, LWMUtils.CornerType.TopLeft);

        if (!LWMUtils.TryFindModEntity(topLeft.X, topLeft.Y, out WaystoneEntity entity) || entity.isActivated || entity.DoingActivationVFX) {
            return false;
        }

        SoundEngine.PlaySound(SoundID.MenuTick);
        WaystoneSystem.Instance.AddNewActivationEntity(topLeft.ToWorldCoordinates(16, 16), entity.WaystoneColor);
        switch (Main.netMode) {
            case NetmodeID.MultiplayerClient:
                ModPacket packet = ModContent.GetInstance<WaystonePacketHandler>().GetPacket();

                packet.Write((int)entity.Position.X);
                packet.Write((int)entity.Position.Y);
                packet.Send();
                break;
            case NetmodeID.SinglePlayer:
                entity.ActivateWaystoneEntity();
                break;
        }

        return true;
    }

    public override bool ValidTeleportCheck_NPCCount(TeleportPylonInfo pylonInfo, int defaultNecessaryNPCCount) => true;

    public override bool CanPlacePylon() => true;

    public override void DrawMapIcon(
        ref MapOverlayDrawContext context,
        ref string mouseOverText,
        TeleportPylonInfo pylonInfo,
        bool isNearPylon,
        Color drawColor,
        float deselectedScale,
        float selectedScale
    ) {
        if (!LWMUtils.TryFindModEntity(pylonInfo.PositionInTiles.X, pylonInfo.PositionInTiles.Y, out WaystoneEntity foundEntity) || !foundEntity.isActivated) {
            return;
        }

        bool mouseOver = context.Draw(
                waystoneMapIcons.Value,
                pylonInfo.PositionInTiles.ToVector2() + new Vector2(1f, 1.5f),
                drawColor,
                new SpriteFrame(1, 5, 0, (byte)foundEntity.waystoneType),
                deselectedScale,
                selectedScale,
                Alignment.Center
            )
            .IsMouseOver;
        DefaultMapClickHandle(mouseOver, pylonInfo, $"Mods.LivingWorldMod.MapInfo.Waystones.{foundEntity.waystoneType}", ref mouseOverText);
    }
}

/// <summary>
///     Tile Entity for the Waystone tiles.
/// </summary>
public class WaystoneEntity : TEModdedPylon {
    public bool isActivated;

    public WaystoneType waystoneType;

    private int _activationTimer;

    public Color WaystoneColor {
        get {
            return waystoneType switch {
                WaystoneType.Desert => Color.Yellow,
                WaystoneType.Jungle => Color.LimeGreen,
                WaystoneType.Mushroom => Color.DarkBlue,
                WaystoneType.Caverns => Color.Lavender,
                WaystoneType.Ice => Color.LightBlue,
                _ => Color.White
            };
        }
    }

    public bool DoingActivationVFX {
        get;
        private set;
    }

    public override void Update() {
        // Update is only called on server; we don't need to do the visual flair for the server, so we just wait until that is done, then update all clients accordingly
        if (DoingActivationVFX) {
            if (++_activationTimer > WaystoneActivationEntity.FullActivationWaitTime) {
                DoingActivationVFX = false;
                _activationTimer = 0;
                isActivated = true;

                NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
            }
        }
    }

    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
        TileObjectData tileData = TileObjectData.GetTileData(type, style, alternate);
        int topLeftX = i - tileData.Origin.X;
        int topLeftY = j - tileData.Origin.Y;

        if (Main.netMode == NetmodeID.MultiplayerClient) {
            NetMessage.SendTileSquare(Main.myPlayer, topLeftX, topLeftY, tileData.Width, tileData.Height);
            NetMessage.SendData(MessageID.TileEntityPlacement, number: topLeftX, number2: topLeftY, number3: Type);
            return -1;
        }

        if (ManualPlace(topLeftX, topLeftY, (WaystoneType)style) && LWMUtils.TryFindModEntity(topLeftX, topLeftY, out WaystoneEntity entity)) {
            return entity.ID;
        }

        return -1;
    }

    public override void OnNetPlace() {
        NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
    }

    public override void NetSend(BinaryWriter writer) {
        writer.Write(isActivated);
        writer.Write((int)waystoneType);
    }

    public override void NetReceive(BinaryReader reader) {
        isActivated = reader.ReadBoolean();
        waystoneType = (WaystoneType)reader.ReadInt32();
    }

    public override void SaveData(TagCompound tag) {
        tag["active"] = isActivated;
        tag["type"] = (int)waystoneType;
    }

    public override void LoadData(TagCompound tag) {
        isActivated = tag.GetBool("active");
        waystoneType = (WaystoneType)tag.GetInt("type");
    }

    /// <summary>
    ///     Should be called whenever the tile that is entity is associated with is right clicked & activated in SINGLERPLAYER or
    ///     ON THE SERVER. Shouldn't
    ///     be called on any multiplayer client; we handle that with our own packets.
    /// </summary>
    public void ActivateWaystoneEntity() {
        if (isActivated || DoingActivationVFX) {
            return;
        }

        DoingActivationVFX = true;
        _activationTimer = 0;
    }

    /// <summary>
    ///     Method use for manual placing of a Waystone Entity. Primary usage is within WorldGen. Will return false
    ///     if the placement is invalid, true otherwise.
    /// </summary>
    /// <param name="i"> x location to attempt entity placement. </param>
    /// <param name="j"> y location to attempt entity placement. </param>
    /// <param name="type"> What type of waystone to place at this location </param>
    /// <param name="isActivated"> Whether or not this waystone should be activated or not. Defaults to false. </param>
    /// <returns></returns>
    public bool ManualPlace(int i, int j, WaystoneType type, bool isActivated = false) {
        // First, double check that tile is a Waystone tile
        if (Framing.GetTileSafely(i, j).TileType != ModContent.TileType<WaystoneTile>()) {
            return false;
        }

        // Then, place tile entity and assign its type
        Place(i, j);
        if (LWMUtils.TryFindModEntity(i, j, out WaystoneEntity retrievedEntity)) {
            retrievedEntity.waystoneType = type;
            retrievedEntity.isActivated = isActivated;
            return true;
        }

        return false;
    }
}