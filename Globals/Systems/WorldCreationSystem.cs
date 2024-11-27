using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.DataStructures.Interfaces;
using LivingWorldMod.DataStructures.Structs;
using LivingWorldMod.Globals.BaseTypes.Systems;
using LivingWorldMod.Globals.ModTypes;
using Terraria.GameContent.Generation;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Globals.Systems;

/// <summary>
///     System that handles the INITIAL world generation steps. This system does NOT handle world
///     events that occur AFTER the world is created.
/// </summary>
public class WorldCreationSystem : BaseModSystem<WorldCreationSystem> {
    /// <summary>
    ///     List of values that exist temporarily during the world generation process.
    /// </summary>
    public List<ITemporaryGenValue> tempWorldGenValues;

    public override void Load() {
        tempWorldGenValues = [];
    }

    public override void Unload() { }

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
        List<WorldGenFeature> listOfFeatures = ModContent.GetContent<WorldGenFeature>().ToList();

        listOfFeatures.ForEach(feature => feature.ModifyTaskList(tasks));

        foreach (WorldGenFeature feature in listOfFeatures) {
            int passIndex = tasks.FindIndex(pass => pass.Name == feature.InsertionPassNameForFeature);
            if (passIndex != -1) {
                tasks.Insert(passIndex + feature.PlaceBeforeInsertionPoint.ToInt(), new PassLegacy(feature.InternalGenerationName, feature.Generate));
            }
            else {
                tasks.Add(new PassLegacy(feature.InternalGenerationName, feature.Generate));
                LWM.Instance.Logger.Warn($"Pass Named {feature.InsertionPassNameForFeature} not found, feature {feature.InternalGenerationName} placed at end of task list!");
            }
        }
    }

    public override void PreWorldGen() {
        tempWorldGenValues = [];
    }

    public override void PostWorldGen() {
        ModContent.GetContent<WorldGenFeature>().ToList().ForEach(feature => feature.PostWorldGenAction());

        tempWorldGenValues.Clear();
    }

    /// <summary>
    ///     Shorthand method that will search and return the value of the specified type
    ///     with the passed in name. Returns null if nothing is found.
    /// </summary>
    /// <typeparam name="T"> The type of the value you are looking for. </typeparam>
    /// <param name="name"> The value of <seealso cref="ITemporaryGenValue.Name" />. </param>
    /// <returns></returns>
    public T GetTempWorldGenValue<T>(string name) {
        return tempWorldGenValues.OfType<TemporaryGenValue<T>>().FirstOrDefault(value => value.Name == name).value;
    }
}