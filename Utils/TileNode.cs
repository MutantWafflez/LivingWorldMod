using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;

namespace LivingWorldMod.Utils
{
    public class TileNode
    {
        public Point16 position;

        public TileNode(Point16 position) => this.position = position;

        public List<TileNode> GetChildren()
        {
            List<TileNode> childNodes = new List<TileNode>();

            EvaluatePoint(position.Add(-1, 0), ref childNodes); //left
            EvaluatePoint(position.Add(0, -1), ref childNodes); //top
            EvaluatePoint(position.Add(1, 0), ref childNodes);  //right
            EvaluatePoint(position.Add(0, 1), ref childNodes);  //bottom

            return childNodes;
        }

        void EvaluatePoint(Point16 coord, ref List<TileNode> childNodes)
        {
            if (Framing.GetTileSafely(coord).wall == WallID.SpiderUnsafe)
                childNodes.Add(new TileNode(coord));
        }
    }
}