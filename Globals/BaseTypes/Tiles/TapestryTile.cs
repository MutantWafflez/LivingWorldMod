using LivingWorldMod.Globals.Sets;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.ObjectData;

namespace LivingWorldMod.Globals.BaseTypes.Tiles;

/// <summary>
/// Generic tile class that serves as the template for all tapestry tile types.
/// Should be added by the item that places it, in <see cref="ModItem.IsLoadingEnabled"/>.
/// </summary>
[Autoload(false)]
public class TapestryTile : BaseTile {
    public override string Texture => _parentItem.Texture;

    public override string Name {
        get;
    }

    public override Color? TileColorOnMap {
        get;
    }

    private readonly ModItem _parentItem;

    public TapestryTile(ModItem parentItem, Color? mapColor) {
        _parentItem = parentItem;
        Name = parentItem.Name.Replace("Item", "Tile");
        TileColorOnMap = mapColor;
    }

    public override void SetStaticDefaults() {
        Main.tileSolid[Type] = false;
        Main.tileNoSunLight[Type] = false;
        Main.tileNoAttach[Type] = true;
        Main.tileWaterDeath[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileFrameImportant[Type] = true;

        TileSets.NeedsAdvancedWindSway[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top);
        TileObjectData.newTile.Width = 3;
        TileObjectData.newTile.Height = 3;
        TileObjectData.newTile.Origin = new Point16(1, 0);
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
        TileObjectData.addTile(Type);
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        Tile tile = Main.tile[i, j];
        if (tile is { TileFrameX: 0, TileFrameY: 0 }) {
            LWMUtils.AddSpecialPoint.DynamicInvoke(i, j, 5 /* MultiTileVine */);
        }

        return false;
    }
}