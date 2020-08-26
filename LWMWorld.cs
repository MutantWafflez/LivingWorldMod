using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.World.Generation;
using Terraria.GameContent.Generation;
using LivingWorldMod.Tiles.WorldGen;
using LivingWorldMod.NPCs.Villagers;
using System.Collections.Generic;
using static Terraria.ModLoader.ModContent;

namespace LivingWorldMod
{
    public class LWMWorld : ModWorld
    {
        public static int[] villageReputation = new int[(int)VillagerType.VillagerTypeCount];

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

            for (int j = 0; j < Main.maxTilesY; j++)
            {
                for (int i = 0; i < Main.maxTilesX; i++)
                {
                    Tile tile = Framing.GetTileSafely(i, j);
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
        }
    }
}
