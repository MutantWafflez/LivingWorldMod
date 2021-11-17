using System.Collections.Generic;
using LivingWorldMod.Common.ModTypes;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Content.WorldGenFeatures.Dungeons {
    /// <summary>
    /// The completely revamped pyramid structure, now with curses!
    /// </summary>
    public class RevampedPyramid : WorldGenFeature {
        public override string InternalGenerationName => "Revamped Pyramid";

        public override string InsertionPassNameForFeature => "Pyramids";

        public override bool PlaceBeforeInsertionPoint => false;

        public override void ModifyTaskList(List<GenPass> tasks) {
            //Decided that vanilla pyramids will be unable to generate. We must "nullify" the pass here, by removing and re-adding it in an
            // empty pass in its place, to preserve mod compatibility.
            int pyramidIndex = tasks.FindIndex(pass => pass.Name == "Pyramids");
            if (pyramidIndex != -1) {
                //Remove vanilla pass
                tasks.RemoveAt(pyramidIndex);
                //Replace with empty pass
                tasks.Insert(pyramidIndex, new PassLegacy("Pyramids", (progress, configuration) => { }));
            }
            else {
                ModContent.GetInstance<LivingWorldMod>().Logger.Warn("Pyramid pass not found. Generating revamped pyramid at end of the task list!");
            }
        }

        public override void Generate(GenerationProgress progress, GameConfiguration gameConfig) {
            progress.Message = "Cursing the Pyramid";
            progress.Set(0f);
        }
    }
}