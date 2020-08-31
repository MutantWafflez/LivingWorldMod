using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;
using Terraria.World.Generation;
using Terraria.GameContent.Generation;
using static Terraria.ModLoader.ModContent;
using LivingWorldMod.Tiles.WorldGen;
using LivingWorldMod.NPCs.Villagers;
using LivingWorldMod.Utils;

namespace LivingWorldMod
{
    public class LWMWorld : ModWorld
    {
        public static int[] villageReputation = new int[(int)VillagerType.VillagerTypeCount];
        public bool[] spiderWalls = new bool[Main.maxTilesX * Main.maxTilesY];
        public bool[] visitedCoords = new bool[Main.maxTilesX * Main.maxTilesY];
        public List<List<Point16>> spiderCaves = new List<List<Point16>>();

        public override TagCompound Save()
        {
            return new TagCompound {
                {"VillageReputation", villageReputation }
            };
        }

        public override void Load(TagCompound tag)
        {
            villageReputation = tag.GetIntArray("VillageReputation");
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight)
        {
            int spiderCavesIndex = tasks.FindIndex(task => task.Name.Equals("Spider Caves"));
            if (spiderCavesIndex != -1)
                tasks.Insert(spiderCavesIndex + 1, new PassLegacy("Spider Sac Tiles", CustomSpiderCavernGenTask));
        }

        private void CustomSpiderCavernGenTask(GenerationProgress progress)
        {
            //Message name should probably change
            progress.Message = "Spawning Extra Spider Caves Tiles";
            progress.Start(0f);

            spiderCaves = new List<List<Point16>>();

            for (int j = 0; j < Main.maxTilesY; j++)
            {
                for (int i = 0; i < Main.maxTilesX; i++)
                {
                    if (i == Main.maxTilesX / 2 && j == Main.maxTilesY) progress.Start(.25f);
                    Tile tile = Framing.GetTileSafely(i, j);
                    //Using a 1D array to store values that correspond to 2D coordinates
                    spiderWalls[j * Main.maxTilesX + i] = tile.wall == WallID.SpiderUnsafe;

                    if (tile.wall == WallID.SpiderUnsafe && !tile.active() && !Framing.GetTileSafely(i + 1, j).active()
                        && !Framing.GetTileSafely(i, j + 1).active() && !Framing.GetTileSafely(i + 1, j + 1).active())
                    {
                        //Coord must have a 5x2 empty space below them
                        int height = 5;
                        bool spaceBelow = true;
                        for (int l = 0; l < 2; l++)
                            for (int k = j; k < j + height; k++)
                                if (Framing.GetTileSafely(i + l, k).active())
                                    spaceBelow = false;

                        if (spaceBelow)
                        {
                            //Prevents tiles in a square of 60 tiles of lenght where current coord is the center
                            int radius = 30;
                            bool foundInRadius = false;

                            for (int m = j - radius; m < j + radius; m++) //y
                            {
                                if (foundInRadius) break;
                                for (int n = i - radius; n < i + radius; n++) //x
                                    if (Framing.GetTileSafely(n, m).type == TileType<SpiderSacTile>())
                                    { foundInRadius = true; break; }
                            }

                            if (!foundInRadius)
                                WorldGen.PlaceTile(i, j, TileType<SpiderSacTile>());
                        }
                    }
                }
            }

            progress.Set(.5f);
            //After one world iteration we know which coords have SpiderWalls on them
            //and we can easily check if those coords were visited already by the BFS algorithm
            for (int k = 0; k < spiderWalls.Length; k++)
            {
                if (k == spiderWalls.Length / 2) progress.Set(.75f);
                if (!spiderWalls[k] || visitedCoords[k]) continue;

                int x = k % Main.maxTilesX;
                int y = k / Main.maxTilesX;

                List<Point16> currentCave = new List<Point16>();
                Point16 currentPos = new Point16(x, y);

                //Simple BFS Algorithm implementation
                Queue<TileNode> queue = new Queue<TileNode>();
                TileNode startingNode = new TileNode(currentPos);
                queue.Enqueue(startingNode);

                while (queue.Count != 0)
                {
                    TileNode activeNode = queue.Dequeue();
                    //1D array that holds values of 2D coordinates needs a special formula for the index
                    int index = activeNode.position.Y * Main.maxTilesX + activeNode.position.X;

                    if (visitedCoords[index]) continue;
                    else visitedCoords[index] = true;

                    //If current coord wasn't visited yet, add it to the current cave list
                    currentCave.Add(activeNode.position);

                    //Node's children already filter out tiles that don't have SpiderWalls as walls
                    //allowing us to just add them all to the queue
                    List<TileNode> childNodes = activeNode.GetChildren();
                    childNodes.ForEach(child => queue.Enqueue(child));
                }
    
                //Finish cave iteration
                spiderCaves.Add(currentCave);
            }

            progress.End();
        }
    }
}
