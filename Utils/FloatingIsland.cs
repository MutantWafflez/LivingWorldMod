using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;

namespace LivingWorldMod.Utils
{
    public class FloatingIsland
    {
        public readonly List<Point16> islandCoords;
        public readonly int xMax;
        public readonly int xMin;

        public FloatingIsland(List<Point16> list)
        {
            islandCoords = list;

            //Finding Min/Max x coords in the island list of coords
            xMax = 0;
            xMin = Main.maxTilesX;
            foreach (Point16 coord in islandCoords)
            {
                if (coord.X > xMax)
                    xMax = coord.X;

                if (coord.X < xMin)
                    xMin = coord.X;
            }
        }

        public int GetYAverage()
        {
            int result = 0;
            islandCoords.ForEach(coord => result += coord.Y);
            return result / islandCoords.Count;
        }
    }
}
