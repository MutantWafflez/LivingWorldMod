using System.Collections.Generic;
using System.IO;
using System.Reflection;
using LivingWorldMod.Custom.Structs;
using LivingWorldMod.Custom.Utilities;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Common.Systems {

    /// <summary>
    /// System that handles the INITIAL world generation steps. This system does NOT handle world
    /// events that occur AFTER the world is created.
    /// </summary>
    public class WorldCreationSystem : ModSystem {

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight) {
            //Harpy Villages
            int microBiomeIndex = tasks.FindIndex(pass => pass.Name.Equals("Micro Biomes"));
            if (microBiomeIndex != -1) {
                tasks.Insert(microBiomeIndex + 1, new PassLegacy("Harpy Village", GenerateHarpyVillage));
            }
        }

        private void GenerateHarpyVillage(GenerationProgress progress, GameConfiguration gameConfig) {
            progress.Message = "Generating Structures... Harpy Village";
            progress.Set(0f);

            StructureData harpyVillageData = IOUtilities.GetStructureFromFile(LivingWorldMod.LWMStructurePath + "/Villages/Harpy/FullHarpyVillage.struct");

            int startingX = Main.spawnTileX - (harpyVillageData.structureWidth / 2);
            int startingY = Main.rand.Next((int)(Main.maxTilesY * 0.0125f), (int)(Main.maxTilesY * 0.05f));

            WorldGenUtilities.GenerateStructure(harpyVillageData, startingX, startingY, ref progress);
        }
    }
}