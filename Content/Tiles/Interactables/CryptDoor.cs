using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ObjectData;

namespace LivingWorldMod.Content.Tiles.Interactables {
    /// <summary>
    /// Not an actual door in a traditional sense; it looks like one, but right clicking doesn't
    /// actually change the tile itself, it's permanently 2x3 (or "open"). Allows entrance into the
    /// Revamped Pyramid Subworlds.
    /// </summary>
    public class CryptDoor : BaseTile {
        public override void SetStaticDefaults() {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;

            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 2, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
            TileObjectData.addTile(Type);
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
            effectOnly = true;
            fail = true;
        }
    }
}