using LivingWorldMod.Content.Items.Placeables.Building;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Tiles.Building {

    public class SunslabBlockTile : BaseTile {

        public override void SetStaticDefaults() {
            Main.tileSolid[Type] = true;
            Main.tileNoSunLight[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileMerge[Type][ModContent.TileType<StarshineBlockTile>()] = true;
            Main.tileMerge[Type][TileID.Sunplate] = true;
            Main.tileMerge[TileID.Sunplate][Type] = true;

            TileID.Sets.ForcedDirtMerging[Type] = true;

            MineResist = 1.34f;

            ItemDrop = ModContent.ItemType<SunslabBlockItem>();

            DustType = DustID.GoldCoin;
            SoundType = SoundID.Tink;

            AddMapEntry(Color.Yellow);
        }

        public override bool HasWalkDust() => true;

        //This tile needs manual framing done due to the limitations of vanilla's framing system; the pattern this tile makes is too much to handle and must be done by us
        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
            Tile thisTile = Framing.GetTileSafely(i, j);

            //Set tile's position
            Point thisTilePosition = new Point(i, j);

            //Reset frame data
            thisTile.FrameNumber = 0;
            thisTile.frameX = 0;
            thisTile.frameY = 0;

            /* Legend:
             * Order of pattern on spritesheet from left to right:
             * top left, top right, bottom left, bottom right
             * Order of merge possibilities on spritesheet in order from top to bottom:
             * No merge-able tiles in any direction
             * One merge-able tile above
             * One merge-able tile to the right
             * One merge-able tile to the left
             * One merge-able tile below
             * One merge-able tile to the right and below
             * One merge-able tile to the left and below
             * One merge-able tile to the right and above
             * One merge-able tile to the left and above
             * One merge-able tile to the left and right
             * One merge-able tile above and below
             * One merge-able tile to the left, below, and right
             * One merge-able tile above, left, and below
             * One merge-able tile above, right, and below
             * One merge-able tile to the left, above, and right
             * One merge-able tile in all directions
             */

            //Merge-able tiles. In this array, the 0th index is above, 1st index is the right, 2nd index is the left, 3rd index is below
            bool[] directionsWithMergableTiles = new bool[] {
                TileUtilities.CanMergeWithTile(thisTile.type, thisTilePosition, new Point(0, -1)),
                TileUtilities.CanMergeWithTile(thisTile.type, thisTilePosition, new Point(1, 0)),
                TileUtilities.CanMergeWithTile(thisTile.type, thisTilePosition, new Point(-1, 0)),
                TileUtilities.CanMergeWithTile(thisTile.type, thisTilePosition, new Point(0, 1))
            };

            //The corner type is determined based on this tile's position in the world, based on odds and evens.

            short cornerType;

            //If both x & y are even, corner will be "top left" or 0
            if (i % 2 == 0 && j % 2 == 0) {
                cornerType = 0;
            }
            //If x is odd & y is even, corner will be "top right" or 1
            else if (i % 2 == 1 && j % 2 == 0) {
                cornerType = 1;
            }
            //If x is even & y is odd, corner will be "bottom left" or 2
            else if (i % 2 == 0 && j % 2 == 1) {
                cornerType = 2;
            }
            //If both x & y are odd, corner will be "bottom right" or 3
            else {
                cornerType = 3;
            }

            //No merge-able tiles in any direction being frame 0
            short verticalFrame = 0;

            //One merge-able tile above
            if (directionsWithMergableTiles[0] && !directionsWithMergableTiles[1] && !directionsWithMergableTiles[2] && !directionsWithMergableTiles[3]) {
                verticalFrame = 1;
            }
            //One merge-able tile to the right
            else if (!directionsWithMergableTiles[0] && directionsWithMergableTiles[1] && !directionsWithMergableTiles[2] && !directionsWithMergableTiles[3]) {
                verticalFrame = 2;
            }
            //One merge-able tile to the left
            else if (!directionsWithMergableTiles[0] && !directionsWithMergableTiles[1] && directionsWithMergableTiles[2] && !directionsWithMergableTiles[3]) {
                verticalFrame = 3;
            }
            //One merge-able tile below
            else if (!directionsWithMergableTiles[0] && !directionsWithMergableTiles[1] && !directionsWithMergableTiles[2] && directionsWithMergableTiles[3]) {
                verticalFrame = 4;
            }
            //One merge-able tile to the right and below
            else if (!directionsWithMergableTiles[0] && directionsWithMergableTiles[1] && !directionsWithMergableTiles[2] && directionsWithMergableTiles[3]) {
                verticalFrame = 5;
            }
            //One merge-able tile to the left and below
            else if (!directionsWithMergableTiles[0] && !directionsWithMergableTiles[1] && directionsWithMergableTiles[2] && directionsWithMergableTiles[3]) {
                verticalFrame = 6;
            }
            //One merge-able tile to the right and above
            else if (directionsWithMergableTiles[0] && directionsWithMergableTiles[1] && !directionsWithMergableTiles[2] && !directionsWithMergableTiles[3]) {
                verticalFrame = 7;
            }
            //One merge-able tile to the left and above
            else if (directionsWithMergableTiles[0] && !directionsWithMergableTiles[1] && directionsWithMergableTiles[2] && !directionsWithMergableTiles[3]) {
                verticalFrame = 8;
            }
            //One merge-able tile to the left and right
            else if (!directionsWithMergableTiles[0] && directionsWithMergableTiles[1] && directionsWithMergableTiles[2] && !directionsWithMergableTiles[3]) {
                verticalFrame = 9;
            }
            //One merge-able tile above and below
            else if (directionsWithMergableTiles[0] && !directionsWithMergableTiles[1] && !directionsWithMergableTiles[2] && directionsWithMergableTiles[3]) {
                verticalFrame = 10;
            }
            //One merge-able tile to the left, below, and right
            else if (!directionsWithMergableTiles[0] && directionsWithMergableTiles[1] && directionsWithMergableTiles[2] && directionsWithMergableTiles[3]) {
                verticalFrame = 11;
            }
            //One merge-able tile above, left, and below
            else if (directionsWithMergableTiles[0] && !directionsWithMergableTiles[1] && directionsWithMergableTiles[2] && directionsWithMergableTiles[3]) {
                verticalFrame = 12;
            }
            //One merge-able tile above, right, and below
            else if (directionsWithMergableTiles[0] && directionsWithMergableTiles[1] && !directionsWithMergableTiles[2] && directionsWithMergableTiles[3]) {
                verticalFrame = 13;
            }
            //One merge-able tile to the left, above, and right
            else if (directionsWithMergableTiles[0] && directionsWithMergableTiles[1] && directionsWithMergableTiles[2] && !directionsWithMergableTiles[3]) {
                verticalFrame = 14;
            }
            //One merge-able tile in all directions
            else if (directionsWithMergableTiles[0] && directionsWithMergableTiles[1] && directionsWithMergableTiles[2] && directionsWithMergableTiles[3]) {
                verticalFrame = 15;
            }

            thisTile.frameX = (short)(18 * cornerType);
            thisTile.frameY = (short)(18 * verticalFrame);

            return false;
        }
    }
}