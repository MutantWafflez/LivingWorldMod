using LivingWorldMod.Items.Placeable.Misc;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace LivingWorldMod.Tiles.Furniture.Misc
{
    public class SkyBudPlanterBox : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileSolidTop[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileMergeDirt[Type] = false;
            Main.tileNoAttach[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinatePadding = 1;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.addTile(Type);

            AddMapEntry(Color.Brown);
            dustType = DustID.Dirt;
            drop = ModContent.ItemType<SkyBudPlanterBoxItem>();
            disableSmartCursor = true;
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            if (Framing.GetTileSafely(i - 1, j).type == type && Framing.GetTileSafely(i + 1, j).type == type)
            {
                frameXOffset = 18;
            }
            else if (Framing.GetTileSafely(i - 1, j).type != type && Framing.GetTileSafely(i + 1, j).type == type)
            {
                frameXOffset = 0;
            }
            else if (Framing.GetTileSafely(i - 1, j).type == type && Framing.GetTileSafely(i + 1, j).type != type)
            {
                frameXOffset = 36;
            }
            else
            {
                frameXOffset = 54;
            }
        }
    }
}