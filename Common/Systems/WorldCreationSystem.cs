using LivingWorldMod.Common.ModTypes;
using LivingWorldMod.Common.Systems.BaseSystems;
using LivingWorldMod.Custom.Interfaces;
using LivingWorldMod.Custom.Structs;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Common.Systems {
    /// <summary>
    /// System that handles the INITIAL world generation steps. This system does NOT handle world
    /// events that occur AFTER the world is created.
    /// </summary>
    public class WorldCreationSystem : BaseModSystem<WorldCreationSystem> {
        /// <summary>
        /// List of values that exist temporarily during the world generation process.
        /// </summary>
        public List<ITemporaryValue> tempWorldGenValues;

        public override void Load() {
            tempWorldGenValues = new List<ITemporaryValue>();
        }

        public override void Unload() { }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight) {
            List<WorldGenFeature> listOfFeatures = ModContent.GetContent<WorldGenFeature>().ToList();

            listOfFeatures.ForEach(feature => feature.ModifyTaskList(tasks));

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

        public override void PreWorldGen() {
            tempWorldGenValues = new List<ITemporaryValue>();
        }

        public override void PostWorldGen() {
            ModContent.GetContent<WorldGenFeature>().ToList().ForEach(feature => feature.PostWorldGenAction());

            tempWorldGenValues.Clear();
        }

        /// <summary>
        /// Shorthand method that will search and return the value of the specified type
        /// with the passed in name. Returns null if nothing is found.
        /// </summary>
        /// <typeparam name="T"> The type of the value you are looking for. </typeparam>
        /// <param name="name"> The value of <seealso cref="ITemporaryValue.Name"/>. </param>
        /// <returns></returns>
        public T GetTempWorldGenValue<T>(string name) {
            return tempWorldGenValues.OfType<TemporaryValue<T>>().FirstOrDefault(value => value.Name == name).value;
        }
    }
}