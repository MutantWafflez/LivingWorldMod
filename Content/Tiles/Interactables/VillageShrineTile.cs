using LivingWorldMod.Common.Systems.UI;
using LivingWorldMod.Content.TileEntities.Interactables;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.Map;
using Terraria.ObjectData;
using Terraria.UI;

namespace LivingWorldMod.Content.Tiles.Interactables;

[LegacyName("HarpyShrineTile")]
public class VillageShrineTile : BasePylon {
    /// <summary>
    /// The tile width of Village Shrines. Used for tile entity placement/destroying calculations.
    /// </summary>
    public const int FullTileWidth = 3;

    public override Color? TileColorOnMap => Color.Yellow;

    /// <summary>
    /// The displacement for which the tile is placed, used for tile entity shenanigans.
    /// </summary>
    public readonly Point16 tileOrigin = new(1, 2);

    public Asset<Texture2D> shrineIcons;

    public override void Load() {
        shrineIcons = ModContent.Request<Texture2D>($"{LWM.SpritePath}MapIcons/ShrineIcons");
    }

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileTable[Type] = false;
        Main.tileLighted[Type] = true;
        Main.tileWaterDeath[Type] = false;
        Main.tileLavaDeath[Type] = false;

        TileObjectData.newTile.WaterDeath = false;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
        TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;

        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.Origin = tileOrigin;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16, 16 };
        TileObjectData.newTile.Height = 5;
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.Width = FullTileWidth;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, FullTileWidth, 0);

        VillageShrineEntity shrineEntity = ModContent.GetInstance<VillageShrineEntity>();
        TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(shrineEntity.PlacementPreviewHook_CheckIfCanPlace, 1, 0, true);
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(shrineEntity.Hook_AfterPlacement, -1, 0, false);

        TileObjectData.addTile(Type);

        TileID.Sets.InteractibleByNPCs[Type] = true;
        TileID.Sets.PreventsSandfall[Type] = true;

        AddToArray(ref TileID.Sets.CountsAsPylon);

        AnimationFrameHeight = 90;
    }


    //TODO: Re-add tile animation once reputation system re-implemented
    /*
    public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
        Point topLeft = Utilities.GetCornerOfMultiTile(Framing.GetTileSafely(i, j), i, j, Utilities.CornerType.TopLeft);
        Tile topLeftTile = Framing.GetTileSafely(topLeft);
    }*/

    //Since these "pylons" aren't a traditional vanilla pylon (with no visual crystal), we override the base implementation to prevent it.
    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) { }

    public override void KillMultiTile(int i, int j, int frameX, int frameY) {
        VillageShrineUISystem shrineUISystem = ModContent.GetInstance<VillageShrineUISystem>();
        if (shrineUISystem.correspondingUIState.EntityPosition == new Point16(i, j)) {
            shrineUISystem.CloseShrineState();
        }

        ModContent.GetInstance<VillageShrineEntity>().Kill(i, j);
    }

    public override bool RightClick(int i, int j) {
        Point topLeft = Utilities.GetCornerOfMultiTile(Framing.GetTileSafely(i, j), i, j, Utilities.CornerType.TopLeft);

        if (!Utilities.TryFindModEntity(topLeft.X, topLeft.Y, out VillageShrineEntity entity)) {
            return false;
        }

        entity.RightClicked();

        return true;
    }

    public override bool ValidTeleportCheck_NPCCount(TeleportPylonInfo pylonInfo, int defaultNecessaryNPCCount) {
        //There must be at least 2 villagers within the village zone (by default, granted that defaulNecessaryNPCCount doesn't change) in order to teleport.
        if (Utilities.TryFindModEntity(pylonInfo.PositionInTiles.X, pylonInfo.PositionInTiles.Y, out VillageShrineEntity entity)) {
            return entity.CurrentHousedVillagersCount >= defaultNecessaryNPCCount;
        }

        return false;
    }

    public override bool CanPlacePylon() => true;

    public override void DrawMapIcon(ref MapOverlayDrawContext context, ref string mouseOverText, TeleportPylonInfo pylonInfo, bool isNearPylon, Color drawColor, float deselectedScale, float selectedScale) {
        if (!Utilities.TryFindModEntity(pylonInfo.PositionInTiles.X, pylonInfo.PositionInTiles.Y, out VillageShrineEntity foundEntity) || !IsShrineVisibleOnMap(foundEntity.shrineType)) {
            return;
        }

        bool mouseOver = context.Draw(
                                    shrineIcons.Value,
                                    pylonInfo.PositionInTiles.ToVector2() + new Vector2(2f, 2.5f),
                                    drawColor,
                                    new SpriteFrame(1, 1, 0, (byte)foundEntity.shrineType),
                                    deselectedScale,
                                    selectedScale,
                                    Alignment.Center)
                                .IsMouseOver;
        DefaultMapClickHandle(mouseOver, pylonInfo, $"Mods.LivingWorldMod.MapInfo.Shrines.{foundEntity.shrineType}", ref mouseOverText);
    }

    /// <summary>
    /// Returns whether or not the given village type will have their shrine icon visible at all on the map.
    /// </summary>
    /// <param name="type"> The type of the village who's shrine we are referring to. </param>
    private bool IsShrineVisibleOnMap(VillagerType type) {
        switch (type) {
            case VillagerType.Harpy:
                return Main.BestiaryTracker.Chats.GetWasChatWith($"{nameof(LivingWorldMod)}/HarpyVillager");
            default:
                ModContent.GetInstance<LWM>().Logger.Error($"Villager Type of {type} is not valid for shrine visibility.");
                return false;
        }
    }
}