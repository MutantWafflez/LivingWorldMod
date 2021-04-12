using System.Collections.Generic;
using LivingWorldMod.Custom.Utilities;
using Terraria;
using Terraria.DataStructures;

namespace LivingWorldMod.Custom.Classes.WorldGen {

    public class TileNode {
        public readonly Point16 position;

        public TileNode(Point16 position) => this.position = position;

        public List<TileNode> GetChildren() {
            List<TileNode> childNodes = new List<TileNode>();

            EvaluatePoint(position.Add(-1, 0), ref childNodes); //left
            EvaluatePoint(position.Add(0, -1), ref childNodes); //top
            EvaluatePoint(position.Add(1, 0), ref childNodes);  //right
            EvaluatePoint(position.Add(0, 1), ref childNodes);  //bottom

            return childNodes;
        }

        private void EvaluatePoint(Point16 coord, ref List<TileNode> childNodes) {
            //Making sure the coordinate is within world bounds
            if (coord.X > Main.maxTilesX || coord.X < 0 || coord.Y > Main.maxTilesY || coord.Y < 0) return;

            childNodes.Add(new TileNode(coord));
        }
    }
}