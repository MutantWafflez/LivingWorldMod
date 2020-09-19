using LivingWorldMod.NPCs.Villagers;
using LivingWorldMod.Tiles.WorldGen;
using LivingWorldMod.Utils;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.World.Generation;
using static Terraria.ModLoader.ModContent;

namespace LivingWorldMod
{
    public class LWMWorld : ModWorld
    {
        public static int[] villageReputation = new int[(int)VillagerType.VillagerTypeCount];

        public bool[] spiderWalls = new bool[Main.maxTilesX * Main.maxTilesY];
        public bool[] visitedCoords = new bool[Main.maxTilesX * Main.maxTilesY];
        public List<List<Point16>> spiderCaves = new List<List<Point16>>();

        #region Reputation
        /// <summary>
        /// Changes the inputted VillagerType's reputation by amount.
        /// </summary>
        /// <param name="villagerType">Enum of VillagerType</param>
        /// <param name="amount">Amount by which the reputation is changed</param>
        public static void ModifyReputation(VillagerType villagerType, int amount)
        {
            if (villagerType >= 0 && villagerType < VillagerType.VillagerTypeCount)
            {
                villageReputation[(int)villagerType] += amount;
            }
        }

        /// <summary>
        /// Changes the inputted VillagerType's reputation by amount, and creates a combat text by the changed amount at combatTextPosition.
        /// </summary>
        /// <param name="villagerType">Enum of VillagerType</param>
        /// <param name="amount">Amount by which the reputation is changed</param>
        /// <param name="combatTextPosition">Location of the combat text created to signify the changed reputation amount</param>
        public static void ModifyReputation(VillagerType villagerType, int amount, Rectangle combatTextPosition)
        {
            if (villagerType >= 0 && villagerType < VillagerType.VillagerTypeCount)
            {
                villageReputation[(int)villagerType] += amount;

                Color combatTextColor = new Color(255, 0, 0);
                if (amount > 0)
                    combatTextColor = new Color(0, 255, 0);
                CombatText.NewText(combatTextPosition, combatTextColor, (amount > 0 ? "+" : "") + amount + " Reputation", true);
            }
        }
        #endregion

        #region I/O
        public override TagCompound Save()
        {
            IList<TagCompound> villagerData = new List<TagCompound>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npcAtIndex = Main.npc[i];
                if (!LWMUtils.IsTypeOfVillager(npcAtIndex))
                    continue;
                else
                {
                    TagCompound villagerDataTag = new TagCompound
                    {
                        {"type", (int)((Villager)npcAtIndex.modNPC).villagerType },
                        {"spriteVar",  ((Villager)npcAtIndex.modNPC).spriteVariation}, 
                        {"x", npcAtIndex.Center.X },
                        {"y", npcAtIndex.Center.Y },
                        {"name", npcAtIndex.GivenName },
                        {"homePos",  ((Villager)npcAtIndex.modNPC).homePosition }
                    };
                    villagerData.Add(villagerDataTag);
                }
            }
            return new TagCompound {
                {"VillageReputation", villageReputation },
                {"VillagerData", villagerData }
            };
        }

        public override void Load(TagCompound tag)
        {
            villageReputation = tag.GetIntArray("VillageReputation");
            IList<TagCompound> villagerData = tag.GetList<TagCompound>("VillagerData");
            for (int i = 0; i < villagerData.Count; i++)
            {
                int villagerType = NPCType<SkyVillager>();
                int recievedVilType = villagerData[i].GetAsInt("type");
                //if (recievedVilType == (int)VillagerType.LihzahrdVillager)
                //Lihzahrd Villager types here
                int npcIndex = NPC.NewNPC((int)villagerData[i].GetFloat("x"), (int)villagerData[i].GetFloat("y"), villagerType);
                NPC npcAtIndex = Main.npc[npcIndex];
                npcAtIndex.GivenName = villagerData[i].GetString("name");
                ((Villager)npcAtIndex.modNPC).spriteVariation = villagerData[i].GetInt("spriteVar");
                ((Villager)npcAtIndex.modNPC).homePosition = villagerData[i].Get<Vector2>("homePos");
            }
        }
        #endregion

        #region World Gen
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight)
        {
            int spiderCavesIndex = tasks.FindIndex(task => task.Name.Equals("Spider Caves"));
            if (spiderCavesIndex != -1)
                tasks.Insert(spiderCavesIndex + 1, new PassLegacy("Spider Sac Tiles", CustomSpiderCavernGenTask));
            int structureGenTask = tasks.FindIndex(task => task.Name.Equals("Micro Biomes"));
            if (structureGenTask != -1)
            {
                tasks.Insert(structureGenTask + 1, new PassLegacy("Sky Village", SkyVillageGenTask));
            }
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

        private void SkyVillageGenTask(GenerationProgress progress)
        {
            //TODO: Find better way to find space for structure
            progress.Message = "Generating Structures... Sky Village";
            progress.Start(0f);

            int xPos = WorldGen.genRand.Next((int)(Main.maxTilesX * 0.1), (int)(Main.maxTilesX * 0.9));

            int yPos = WorldGen.genRand.Next(Main.maxTilesY - (int)(Main.maxTilesY * 0.95), Main.maxTilesY - (int)(Main.maxTilesY * 0.875));
            progress.Set(0.34f);

            //Structure is 160 x 92
            bool foundFreeSpace = false;
            while (!foundFreeSpace)
            {
                bool overlap = false;
                for (int i = xPos; i < xPos + 160; i++)
                {
                    for (int j = yPos; j < yPos + 92; j++)
                    {
                        if (!Framing.GetTileSafely(i, j).active())
                            continue;
                        overlap = true;
                        break;
                    }
                    if (overlap)
                        break;
                }
                if (overlap)
                {
                    xPos = WorldGen.genRand.Next((int)(Main.maxTilesX * 0.1), (int)(Main.maxTilesX * 0.9));
                    yPos = WorldGen.genRand.Next(Main.maxTilesY - (int)(Main.maxTilesY * 0.95), Main.maxTilesY - (int)(Main.maxTilesY * 0.875));
                    continue;
                }
                else
                {
                    foundFreeSpace = true;
                }
            }
            progress.Set(0.67f);

            StructureHelper.StructureHelper.GenerateStructure("Structures/SkyVillageStructure", new Point16(xPos, yPos), mod);

            progress.Set(0.99f);

            progress.End();
        }
        public override void PostWorldGen()
        {
            //Spawn Villagers
            for (int i = 0; i < Main.maxTilesX; i++)
            {
                for (int j = 0; j < Main.maxTilesY; j++)
                {
                    if (Framing.GetTileSafely(i, j).type == TileType<SkyVillagerHomeTile>())
                    {
                        int npcIndex = NPC.NewNPC(i * 16, j * 16, NPCType<SkyVillager>());
                        NPC npcAtIndex = Main.npc[npcIndex];
                        ((Villager)npcAtIndex.modNPC).homePosition = new Vector2(i, j);
                    }
                }
            }
        }
        #endregion
    }
}
