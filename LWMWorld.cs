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
        public static int[] villageGiftCooldown = new int[(int)VillagerType.VillagerTypeCount];

        public override void Initialize()
        {
            //Main.maxTilesX/Y change from world to world and needs to be updated
            iterationsPerTick = Main.maxTilesX * Main.maxTilesY / 54000;
        }

        #region Reputation
        /// <summary>
        /// Changes the inputted VillagerType's reputation by amount.
        /// </summary>
        /// <param name="villagerType">Enum of VillagerType</param>
        /// <param name="amount">Amount by which the reputation is changed</param>
        public static void ModifyReputation(VillagerType villagerType, int amount, bool wasGift = false)
        {
            if (villagerType >= 0 && villagerType < VillagerType.VillagerTypeCount)
            {
                villageReputation[(int)villagerType] += amount;
                if (wasGift)
                {
                    villageGiftCooldown[(int)villagerType] = LivingWorldMod.villageGiftCooldownTime;
                }
            }
        }

        /// <summary>
        /// Changes the inputted VillagerType's reputation by amount, and creates a combat text by the changed amount at combatTextPosition.
        /// </summary>
        /// <param name="villagerType">Enum of VillagerType</param>
        /// <param name="amount">Amount by which the reputation is changed</param>
        /// <param name="combatTextPosition">Location of the combat text created to signify the changed reputation amount</param>
        public static void ModifyReputation(VillagerType villagerType, int amount, Rectangle combatTextPosition, bool wasGift = false)
        {
            if (villagerType >= 0 && villagerType < VillagerType.VillagerTypeCount)
            {
                villageReputation[(int)villagerType] += amount;
                if (wasGift)
                    villageGiftCooldown[(int)villagerType] = LivingWorldMod.villageGiftCooldownTime;

                Color combatTextColor = Color.Lime;
                if (amount < 0)
                    combatTextColor = Color.Red;
                else if (amount == 0)
                    combatTextColor = Color.Gray;
                CombatText.NewText(combatTextPosition, combatTextColor, (amount >= 0 ? "+" : "") + amount + " Reputation", true);
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
                {"VillagerData", villagerData },
                {"VillageGiftCooldown", villageGiftCooldown }
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
            villageGiftCooldown = tag.GetIntArray("VillageGiftCooldown");
        }
        #endregion

        #region World Gen
		//https://github.com/tModLoader/tModLoader/wiki/Vanilla-World-Generation-Steps
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

            int tileArray1D = Main.maxTilesX * Main.maxTilesY;

            for (int k = 0; k < tileArray1D; k++)
            {
                progress.Set((float)k / tileArray1D);

                int i = k % Main.maxTilesX;
                int j = k / Main.maxTilesX;

                if (CanSpawnSpiderSac(i, j))
                    WorldGen.PlaceTile(i, j, TileType<SpiderSacTile>(), forced: true);
            }

            progress.Set(1f);
            progress.End();
        }

        private bool CanSpawnSpiderSac(int i, int j)
        {
            //Making sure we don't go out of world bounds
            if (i > Main.maxTilesX || i - 1 < 0 || j > Main.maxTilesY || j - 1 < 0) return false;

            Point16[] tileCoords = {
                    //Top Left/Right, Mid Left/Right, Bottom Left/Right
                    new Point16(i, j - 1), new Point16(i + 1, j - 1),
                    new Point16(i, j), new Point16(i + 1, j),
                    new Point16(i, j + 1), new Point16(i + 1, j + 1)
                };

            //Making sure all of the tiles are within a Spider Cave
            foreach (Point16 coord in tileCoords)
                if (Framing.GetTileSafely(coord).wall != WallID.SpiderUnsafe) return false;

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
                    if (Framing.GetTileSafely(n, m).type == TileType<SpiderSacTile>())
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

        int iterationsPerTick = 0;
        int startingIteration = 0;
        int currentIteration = 0;
        int spiderSacsPlaced = 0;
        public override void PostUpdate()
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
                { currentIteration += iterationsPerTick - i; break; }

                int x = currentIteration % Main.maxTilesX;
                int y = currentIteration / Main.maxTilesX;

                if (CanSpawnSpiderSac(x, y) && WorldGen.PlaceTile(x, y, TileType<SpiderSacTile>(), forced: true))
                    spiderSacsPlaced++;

                currentIteration++;
            }

            for (int i = 0; i < villageGiftCooldown.Length; i++)
            {
                if (--villageGiftCooldown[i] < 0)
                {
                    villageGiftCooldown[i] = 0;
                }
            }
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
