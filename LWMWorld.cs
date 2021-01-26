using LivingWorldMod.Buffs.PotionBuffs;
using LivingWorldMod.NPCs.Villagers;
using LivingWorldMod.Tiles.Interactables;
using LivingWorldMod.Tiles.Ores;
using LivingWorldMod.Tiles.WorldGen;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.World.Generation;

namespace LivingWorldMod
{
    public class LWMWorld : ModWorld
    {
        //After reaching max gifts on the shrine at stage 5, give 20 reputation

        //Village arrays
        internal static int[] reputation = new int[(int)VillagerType.VillagerTypeCount];

        internal static int[] reputationAbsorption = new int[(int)VillagerType.VillagerTypeCount];
        internal static int[] giftProgress = new int[(int)VillagerType.VillagerTypeCount];
        internal static int[] shrineStage = Enumerable.Repeat(3, (int)VillagerType.VillagerTypeCount).ToArray();
        internal static int[] itemGiftAmount = new int[ItemLoader.ItemCount * (int)VillagerType.VillagerTypeCount];
        internal static Vector2[] shrineCoords = new Vector2[(int)VillagerType.VillagerTypeCount];

        public override void Initialize()
        {
            //Main.maxTilesX/Y change from world to world and needs to be updated
            iterationsPerTick = Main.maxTilesX * Main.maxTilesY / 54000;
        }

        #region World Array Modifications

        //TODO: Multiplayer Compat. for the modification methods
        //TODO: reinitialize stuff in case there's no save file (fresh world)

        /// <summary>
        /// Returns reputation value of the given villager type.
        /// Also includes the reputation absorption of the given villager type, if applicable.
        /// </summary>
        /// <param name="villagerType">Villager type to get the reputation of.</param>
        public static int GetReputation(VillagerType villagerType) => Utils.Clamp(reputation[(int)villagerType] + reputationAbsorption[(int)villagerType], 0, LivingWorldMod.maximumReputationValue);

        /// <summary>
        /// Changes the inputted VillagerType's reputation by amount.
        /// </summary>
        /// <param name="villagerType">Enum of VillagerType</param>
        /// <param name="amount">Amount by which the reputation is changed</param>
        public static void SetReputation(VillagerType villagerType, int amount)
        {
            amount = Utils.Clamp(amount, 0, LivingWorldMod.maximumReputationValue);
            reputation[(int)villagerType] = amount;
        }

        /// <summary>
        /// Returns reputation absorption value of the given villager type.
        /// </summary>
        /// <param name="villagerType">Villager type to get the reputation absorption of.</param>
        public static int GetReputationAbsorption(VillagerType villagerType) => reputationAbsorption[(int)villagerType];

        /// <summary>
        /// Changes the inputted VillagerType's reputation absorption by amount.
        /// </summary>
        /// <param name="villagerType">Enum of VillagerType</param>
        /// <param name="amount">Amount by which the absorption is changed</param>
        public static void SetReputationAbsorption(VillagerType villagerType, int amount)
        {
            reputationAbsorption[(int)villagerType] = amount;
        }

        /// <summary>
        /// Returns the gifting progress of the given villager type.
        /// </summary>
        /// <param name="villagerType">Villager type to get the gifting progress of.</param>
        public static int GetGiftProgress(VillagerType villagerType) => giftProgress[(int)villagerType];

        /// <summary>
        /// Modifies the gifting progress of the given villager type.
        /// </summary>
        /// <param name="villagerType">Villager type to modify the gifting progress of.</param>
        /// <param name="amount">The amount to change the gifting progress by.</param>
        public static void SetGiftProgress(VillagerType villagerType, int amount)
        {
            amount = Utils.Clamp(amount, 0, 100);
            giftProgress[(int)villagerType] = amount;
        }

        /// <summary>
        /// Returns the shrine stage of the given villager type shrine.
        /// </summary>
        /// <param name="villagerType">Villager type to get the shrine stage of.</param>
        public static int GetShrineStage(VillagerType villagerType) => shrineStage[(int)villagerType];

        /// <summary>
        /// Modifies the shrine stage of the given villager type.
        /// </summary>
        /// <param name="villagerType">Villager type to modify the shrine stage of.</param>
        /// <param name="amount">The amount to change the shrine stage by.</param>
        public static void SetShrineStage(VillagerType villagerType, int amount)
        {
            amount = Utils.Clamp(amount, 0, 5);
            shrineStage[(int)villagerType] = amount;
        }

        /// <summary>
        /// Adds an item's gift value to the current gift progess.
        /// </summary>
        /// <param name="villagerType">Villager type to get the gifting progress of.</param>
        /// <param name="itemType">Gift's item type.</param>
        public static void AddGiftToProgress(VillagerType villagerType, int itemType)
        {
            int giftValue = LivingWorldMod.GetGiftValue(villagerType, itemType);
            int giftProgress = GetGiftProgress(villagerType);
            int shrineStage = GetShrineStage(villagerType);

            if (giftProgress + giftValue >= 100)
            {
                int remaining = (giftProgress + giftValue) - 100;

                if (shrineStage < 5)
                {
                    SetShrineStage(villagerType, shrineStage + 1);
                    SetGiftProgress(villagerType, remaining);
                }
            }
            else if (giftProgress + giftValue < 0)
            {
                int remaining = 0 - (giftProgress + giftValue);

                if (shrineStage >= 0)
                {
                    SetShrineStage(villagerType, shrineStage - 1);
                    SetGiftProgress(villagerType, 100 - remaining);
                }
            }
            else SetGiftProgress(villagerType, giftProgress + giftValue);

            //Setting default values and increasing reputation if it reaches max gift progress on last stage
            if (shrineStage == 5 && giftProgress + giftProgress >= 100)
            {
                SetGiftProgress(villagerType, 0);
                SetShrineStage(villagerType, 3);
                SetReputation(villagerType, GetReputation(villagerType) + 20);
            }

            //Increasing the gifted amount
            IncreaseGiftAmount(villagerType, itemType);
        }

        /// <summary>
        /// Returns the item's gifted amount.
        /// </summary>
        /// <param name="villagerType">Villager type to get the gifting progress of.</param>
        /// <param name="itemType">Gift's item type.</param>
        public static int GetGiftAmount(VillagerType villagerType, int itemType)
        {
            int index = (int)villagerType * ItemLoader.ItemCount + itemType;

            return itemGiftAmount[index];
        }

        /// <summary>
        /// Increases the item's gifted amount by one.
        /// </summary>
        /// <param name="villagerType">Villager type to get the gifting progress of.</param>
        /// <param name="itemType">Gift's item type.</param>
        public static void IncreaseGiftAmount(VillagerType villagerType, int itemType)
        {
            int index = (int)villagerType * ItemLoader.ItemCount + itemType;

            itemGiftAmount[index]++;
        }

        /// <summary>
        /// Returns whether the item was already gifted once or more.
        /// </summary>
        /// <param name="villagerType">Villager type to get the gifting progress of.</param>
        /// <param name="itemType">Gift's item type.</param>
        public static bool IsGiftDiscovered(VillagerType villagerType, int itemType)
        {
            int index = ItemLoader.ItemCount * itemType + (int)villagerType;

            return GetGiftAmount(villagerType, itemType) >= 1;
        }

        /// <summary>
        /// Returns the TILE coordinates of the given villager type's shrine.
        /// </summary>
        /// <param name="villagerType">Type of villager to get the shrine coords of.</param>
        public static Vector2 GetShrineTilePosition(VillagerType villagerType)
        {
            return shrineCoords[(int)villagerType];
        }

        /// <summary>
        /// Returns the WORLD (Tile Pos * 16) coordinates of the given villager type's shrine.
        /// </summary>
        /// <param name="villagerType">Type of villager to get the shrine coords of.</param>
        public static Vector2 GetShrineWorldPosition(VillagerType villagerType)
        {
            return shrineCoords[(int)villagerType] * 16;
        }

        #endregion World Array Modifications

        #region Update Methods

        public override void PostUpdate()
        {
            SpiderSacRegen();
            UpdateReputationAbsorption();
            
            if(Main.dayTime && Main.time == 0)
                RefreshDailyShops();
        }

        public void UpdateReputationAbsorption()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < reputationAbsorption.Length; i++)
                {
                    reputationAbsorption[i] = 0;
                }
                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    if (Main.LocalPlayer.HasBuff(ModContent.BuffType<Charmed>()))
                    {
                        reputationAbsorption[(int)VillagerType.Harpy] += 20;
                    }
                }
                else
                {
                    if (Main.player.Any(player => player.HasBuff(ModContent.BuffType<Charmed>())))
                    {
                        reputationAbsorption[(int)VillagerType.Harpy] += 20;
                    }
                }
            }
        }
        
        public void RefreshDailyShops()
        {
            // fetch each villager and refresh their shop
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.modNPC is Villager villager)
                    villager.RefreshDailyShop();
            }
        }

        #endregion Update Methods

        #region I/O

        public override TagCompound Save()
        {
            IList<TagCompound> villagerData = new List<TagCompound>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npcAtIndex = Main.npc[i];
                if(npcAtIndex.modNPC is Villager villager)
                {
                    villagerData.Add(villager.Save());
                }
            }

            return new TagCompound
            {
                {"VillageReputation", reputation},
                {"VillageGiftProgress", giftProgress},
                {"VillageShrineStage", shrineStage},
                {"VillageShrineCoords", shrineCoords.ToList() },
                {"VillagerData", villagerData},
            };
        }

        public override void Load(TagCompound tag)
        {
            reputation = tag.GetIntArray("VillageReputation");
            giftProgress = tag.GetIntArray("VillageGiftProgress");
            shrineStage = tag.GetIntArray("VillageShrineStage");
            shrineCoords = tag.GetList<Vector2>("VillageShrineCoords").ToArray();
            IList<TagCompound> villagerData = tag.GetList<TagCompound>("VillagerData");
            for (int i = 0; i < villagerData.Count; i++)
            {
                Villager.LoadVillager(villagerData[i]);
            }
        }

        #endregion I/O

        #region World Gen

        //https://github.com/tModLoader/tModLoader/wiki/Vanilla-World-Generation-Steps
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight)
        {
            // int pyramids = tasks.FindIndex(task => task.Name.Equals("Pyramids"));
            // tasks.RemoveAt(pyramids);

            int spiderCavesIndex = tasks.FindIndex(task => task.Name.Equals("Spider Caves"));
            if (spiderCavesIndex != -1)
                tasks.Insert(spiderCavesIndex + 1, new PassLegacy("Spider Sac Tiles", CustomSpiderCavernGenTask));

            int structureGenTask = tasks.FindIndex(task => task.Name.Equals("Micro Biomes"));
            if (structureGenTask != -1)
                tasks.Insert(structureGenTask + 1, new PassLegacy("Sky Village", SkyVillageGenTask));

            int roseQuartzTask = tasks.FindIndex(task => task.Name.Equals("Gems"));
            if (roseQuartzTask != -1)
                tasks.Insert(roseQuartzTask + 1, new PassLegacy("Rose Quartz", RoseQuartzGeneration));
        }

        private void CustomSpiderCavernGenTask(GenerationProgress progress)
        {
            //Message name should probably change
            progress.Message = "Spawning Extra Spider Caves Tiles";
            progress.Start(0f);

            int tileArray1D = Main.maxTilesX * Main.maxTilesY;

            for (int k = 0; k < tileArray1D; k++)
            {
                progress.Set((float)k / tileArray1D);

                int i = k % Main.maxTilesX;
                int j = k / Main.maxTilesX;

                if (CanSpawnSpiderSac(i, j))
                    WorldGen.PlaceTile(i, j, ModContent.TileType<SpiderSacTile>(), forced: true);
            }

            progress.Set(1f);
            progress.End();
        }

        private bool CanSpawnSpiderSac(int i, int j)
        {
            //Making sure we don't go out of world bounds
            if (i > Main.maxTilesX || i - 1 < 0 || j > Main.maxTilesY || j - 1 < 0) return false;

            Point16[] tileCoords =
            {
                //Top Left/Right, Mid Left/Right, Bottom Left/Right
                new Point16(i, j - 1), new Point16(i + 1, j - 1),
                new Point16(i, j), new Point16(i + 1, j),
                new Point16(i, j + 1), new Point16(i + 1, j + 1)
            };

            //Making sure all of the tiles are within a Spider Cave
            foreach (Point16 coord in tileCoords)
                if (Framing.GetTileSafely(coord).wall != WallID.SpiderUnsafe)
                    return false;

            //Coord must have a heightx2 empty space below them
            int height = 7;
            for (int l = 0; l < 2; l++)
                for (int k = j; k < j + height; k++)
                {
                    Tile tempTile = Framing.GetTileSafely(i + l, k);
                    if (tempTile.active() && (tempTile.type != TileID.Cobweb || tempTile.type != 159)) return false;
                }

            //Prevents tiles in a square of 60 tiles of lenght where current coord is the center
            int radius = 30;
            for (int m = j - radius; m < j + radius; m++) //y
                for (int n = i - radius; n < i + radius; n++) //x
                    if (Framing.GetTileSafely(n, m).type == ModContent.TileType<SpiderSacTile>())
                        return false;

            //Making sure the foundation for the Spider Sac aren't cobwebs or other non solid stuff
            for (int m = 0; m < 2; m++)
                if (Framing.GetTileSafely(tileCoords[m]).collisionType != 1)
                    return false;

            //Smoothing top tiles where Spider Sac will sit
            for (int m = 0; m < 2; m++)
            {
                Framing.GetTileSafely(tileCoords[m]).slope(0);
                Framing.GetTileSafely(tileCoords[m]).halfBrick(false);
            }

            //Removing Cobwebs where Spider Sac will be placed
            for (int m = 2; m < 6; m++)
                WorldGen.KillTile(tileCoords[m].X, tileCoords[m].Y, effectOnly: true);

            return true;
        }

        private int iterationsPerTick = 0;
        private int startingIteration = 0;
        private int currentIteration = 0;
        private int spiderSacsPlaced = 0;

        private void SpiderSacRegen()
        {
            //Progressive iteration of the world throughout the span of 15mins
            for (int i = 0; i < iterationsPerTick; i++)
            {
                //Resetting currentIteration to 0 after it reaches the max index
                if (currentIteration >= Main.maxTilesX * Main.maxTilesY)
                    currentIteration = spiderSacsPlaced = 0;

                //After a full "lap" around the world we can set a new random point to start our "lap"
                if (currentIteration == startingIteration)
                    startingIteration = currentIteration = Main.rand.Next(Main.maxTilesX * Main.maxTilesY);

                //Only allow a max of 2 Spider Sacs placed per "lap"
                if (spiderSacsPlaced >= 2)
                {
                    currentIteration += iterationsPerTick - i;
                    break;
                }

                int x = currentIteration % Main.maxTilesX;
                int y = currentIteration / Main.maxTilesX;

                if (CanSpawnSpiderSac(x, y) && WorldGen.PlaceTile(x, y, ModContent.TileType<SpiderSacTile>(), forced: true))
                    spiderSacsPlaced++;

                currentIteration++;
            }
        }

        private void SkyVillageGenTask(GenerationProgress progress)
        {
            progress.Message = "Generating Structures...Sky Village";
            progress.Start(0f);

            #region Mapping Floating Islands

            List<FloatingIsland> islands = new List<FloatingIsland>();
            int islandsZone = (int)(Main.maxTilesY * 0.18f);
            bool[] visitedCoords = new bool[Main.maxTilesX * islandsZone];

            bool validCoordinate(Point16 pos)
            {
                //Making sure the coordinate is within the visitedCoords array
                if (pos.Y >= islandsZone) return false;

                Tile tile = Framing.GetTileSafely(pos);
                if (tile.type != TileID.Cloud && tile.type != TileID.RainCloud) return false;

                return true;
            }

            //left top most visible screen coord is (41, 41)?
            for (int i = 41; i < Main.maxTilesX; i++)
            {
                for (int j = 41; j < islandsZone; j++)
                {
                    Point16 currentPos = new Point16(i, j);
                    if (!validCoordinate(currentPos)) continue;

                    List<Point16> currentIsland = new List<Point16>();

                    //Simple BFS Algorithm implementation
                    Queue<TileNode> queue = new Queue<TileNode>();
                    TileNode startingNode = new TileNode(currentPos);
                    queue.Enqueue(startingNode);

                    while (queue.Count != 0)
                    {
                        TileNode activeNode = queue.Dequeue();
                        if (!validCoordinate(activeNode.position)) continue;

                        //1D array that holds values of 2D coordinates needs a special formula for the index
                        int index = activeNode.position.Y * Main.maxTilesX + activeNode.position.X;
                        //index = row * maxColumns + column

                        if (visitedCoords[index]) continue;
                        else visitedCoords[index] = true;

                        //If current coord wasn't visited yet, add it to the current island list
                        currentIsland.Add(activeNode.position);

                        List<TileNode> childNodes = activeNode.GetChildren();
                        childNodes.ForEach(child => queue.Enqueue(child));
                    }

                    //300 tiles? I should probably make some tests and define an average/2
                    if (currentIsland.Count > 300)
                        islands.Add(new FloatingIsland(currentIsland));

                    progress.Set((i * j / visitedCoords.Length) * 0.80f);
                }
            }

            #endregion Mapping Floating Islands

            #region Finding Suitable Spot and Generating it

            int structureWidth = 160;
            int structureHeight = 92;

            //TODO: MAKE IT CHOOSE SPOTS THAT ARE NOT IN BETWEEN ISLANDS IF THEY'RE CLOSER TO THE CENTER (can happen in small worlds)
            //X
            int worldCenter = Main.maxTilesX / 2;
            int biggestDistanceBetweenIslands = 0;
            int smallestDistanceToWorldCenter = Main.maxTilesX;
            int xCoord = 0;

            for (int i = 0; i < islands.Count; i++)
            {
                //Can't do islands[i + 1] on last element
                if (i != islands.Count - 1)
                {
                    //Finding the biggest distance between two islands where the middle spot between the two is the closest to the world center
                    int distanceBetweenIslands =
                        Math.Abs(islands[i + 1].xMin - islands[i].xMax); //Math.Abs not needed since they're ordered?
                    int theoricalXCoord = islands[i].xMax + distanceBetweenIslands / 2 - structureWidth / 2;

                    if (distanceBetweenIslands > biggestDistanceBetweenIslands &&
                        Math.Abs(theoricalXCoord - worldCenter) < smallestDistanceToWorldCenter)
                    {
                        biggestDistanceBetweenIslands = distanceBetweenIslands;
                        smallestDistanceToWorldCenter = Math.Abs(theoricalXCoord - worldCenter);
                        xCoord = theoricalXCoord;
                    }
                }
            }

            progress.Set(0.85f);

            //Y
            int yAverage = 0;
            foreach (FloatingIsland island in islands)
                yAverage += island.GetYAverage();
            yAverage /= islands.Count;

            //Make sure structure y value doesn't go below 41 (world border)
            yAverage = yAverage - structureHeight > 41 ? yAverage - structureHeight : 42;

            progress.Set(0.90f);
            StructureHelper.StructureHelper.GenerateStructure("Structures/SkyVillageStructure",
                new Point16(xCoord, yAverage), mod);

            #endregion Finding Suitable Spot and Generating it

            //Finding shrine top left position
            for (int i = xCoord; i < xCoord + structureWidth; i++)
                for (int j = yAverage; j < yAverage + structureHeight; j++)
                    if (Framing.GetTileSafely(i, j).type == ModContent.TileType<HarpyShrineTile>())
                        shrineCoords[(int)VillagerType.Harpy] =
                            LWMUtils.FindMultiTileTopLeft(i, j, ModContent.TileType<HarpyShrineTile>());

            //This Gen task is in the Sky Village Gen task since the islands are mapped in this method, and we need those
            SkyBudGenTask(progress, islands);

            progress.End();
        }

        private void RoseQuartzGeneration(GenerationProgress progress)
        {
            progress.Message = "Rose quartz-ifying the sky";
            progress.Start(0f);

            for (int k = 0; k < (int)((Main.maxTilesX * Main.maxTilesY) * 0.005); k++)
            {
                int x = WorldGen.genRand.Next(0, Main.maxTilesX);
                int y = WorldGen.genRand.Next(0, (int)(Main.maxTilesY * 0.15f));

                Tile tile = Framing.GetTileSafely(x, y);
                if (tile.active() && tile.type == TileID.Dirt)
                {
                    //3rd and 4th values are strength and steps, adjust later if needed
                    WorldGen.TileRunner(x, y, WorldGen.genRand.Next(3, 5), WorldGen.genRand.Next(2, 4),
                        ModContent.TileType<RoseQuartzTile>());
                }
            }

            progress.Set(1f);
            progress.End();
        }

        private void SkyBudGenTask(GenerationProgress progress, List<FloatingIsland> islands)
        {
            progress.Message = "Planting Sky Buds";

            progress.Set(0.9f);

            //Goes through each mapped island, then tries to plant on each cloud tile with no water with a 1/15 chance
            foreach (FloatingIsland island in islands)
            {
                for (int i = island.xMin; i < island.xMax; i++)
                {
                    for (int j = 0; j < Main.maxTilesY * 0.18f; j++)
                    {
                        if ((Framing.GetTileSafely(i, j).type == TileID.Cloud || Framing.GetTileSafely(i, j).type == TileID.RainCloud) && !Framing.GetTileSafely(i, j - 1).active() && Framing.GetTileSafely(i, j - 1).liquid == 0)
                        {
                            if (Main.rand.Next(0, 16) == 0)
                            {
                                WorldGen.Place1x1(i, j - 1, ModContent.TileType<SkyBudHerb>());
                                Framing.GetTileSafely(i, j - 1).frameX = 0;
                            }
                        }
                    }
                }
            }

            progress.End();
        }

        public override void PostWorldGen()
        {
            //Spawn Villagers
            for (int i = 0; i < Main.maxTilesX; i++)
            {
                for (int j = 0; j < Main.maxTilesY; j++)
                {
                    if (Framing.GetTileSafely(i, j).type == ModContent.TileType<SkyVillagerHomeTile>())
                    {
                        int npcIndex = NPC.NewNPC(i * 16, j * 16, ModContent.NPCType<SkyVillager>());
                        NPC npcAtIndex = Main.npc[npcIndex];
                        ((Villager)npcAtIndex.modNPC).homePosition = new Vector2(i, j);
                    }
                }
            }
        }

        #endregion World Gen
    }
}