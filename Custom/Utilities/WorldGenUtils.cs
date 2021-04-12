using LivingWorldMod.Content.Tiles.Interactables;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Custom.Utilities
{
    public static class WorldGenUtils
    {
        /// <summary>
        /// Finds the top left tile coordinate of a multi tile.
        /// </summary>
        /// <param name="i"> Horizontal tile coordinates </param>
        /// <param name="j"> Vertical tile coordinates </param>
        /// <param name="tileType"> Multi tile's tile type. </param>
        /// <returns> </returns>
        public static Vector2 FindMultiTileTopLeft(int i, int j, int tileType)
        {
            Vector2 topLeftPos = new Vector2(i, j);
            //Finding top left corner
            for (int k = 0; k < 4; k++)
            {
                for (int l = 0; l < 5; l++)
                {
                    if (Framing.GetTileSafely(i - k, j - l).type == ModContent.TileType<HarpyShrineTile>())
                        topLeftPos = new Vector2(i - k, j - l);
                    else continue;
                }
            }

            return topLeftPos;
        }

        /// <summary>
        /// Returns whether or not the space defined by the rectangle <paramref name="zone"/> has
        /// any tiles in it. Make sure zone's position is defined in terms of TILES!
        /// </summary>
        /// <param name="zone"> The zone to check. </param>
        /// <param name="ignoreLiquid">
        /// Whether or not liquid will make this space not "free" space.
        /// </param>
        public static bool CheckForFreeSpace(Rectangle zone, bool ignoreLiquid = true)
        {
            for (int i = 0; i < zone.Width; i++)
            {
                for (int j = 0; j < zone.Height; j++)
                {
                    Tile tileInQuestion = Framing.GetTileSafely((int)zone.TopLeft().X + i, (int)zone.TopLeft().Y + j);
                    if (!(tileInQuestion.type == 0) && (ignoreLiquid || tileInQuestion.liquid == 0))
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}