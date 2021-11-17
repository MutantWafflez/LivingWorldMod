using LivingWorldMod.Common.ModTypes;
using LivingWorldMod.Custom.Enums;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Common.Systems {
    /// <summary>
    /// System that handles the INITIAL world generation steps. This system does NOT handle world
    /// events that occur AFTER the world is created.
    /// </summary>
    public class WorldCreationSystem : ModSystem {
        /// <summary>
        /// List of all the areas which are the "zones" belonging to each village. Different for
        /// each world.
        /// </summary>
        public Rectangle[] villageZones;

        public override void Load() {
            villageZones = new Rectangle[(int)VillagerType.TypeCount];
        }

        public override void Unload() { }

        public override void SaveWorldData(TagCompound tag) {
            tag["VillageZones"] = villageZones.ToList();
        }

        public override void LoadWorldData(TagCompound tag) {
            villageZones = tag.Get<List<Rectangle>>("VillageZones").ToArray();
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight) {
            List<WorldGenFeature> listOfFeatures = ModContent.GetContent<WorldGenFeature>().ToList();

            foreach (WorldGenFeature feature in listOfFeatures) {
                feature.ModifyTaskList(tasks);
            }

            foreach (WorldGenFeature feature in listOfFeatures) {
                int passIndex = tasks.FindIndex(pass => pass.Name == feature.InsertionPassNameForFeature);
                if (passIndex != -1) {
                    tasks.Insert(passIndex + feature.PlaceBeforeInsertionPoint.ToInt(), new PassLegacy(feature.InternalGenerationName, feature.Generate));
                }
                else {
                    tasks.Add(new PassLegacy(feature.InternalGenerationName, feature.Generate));
                    ModContent.GetInstance<LivingWorldMod>().Logger.Warn($"Pass Named {feature.InsertionPassNameForFeature} not found, feature {feature.InternalGenerationName} placed at end of task list!");
                }
            }
        }

        public override void PostWorldGen() {
            List<WorldGenFeature> listOfFeatures = ModContent.GetContent<WorldGenFeature>().ToList();

            foreach (WorldGenFeature feature in listOfFeatures) {
                feature.PostWorldGenAction();
            }
        }
    }
}