using System.Collections.Generic;
using Terraria;
using LivingWorldMod.Common.ModTypes;
using LivingWorldMod.Custom.Structs;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Generation;
using Terraria.ID;
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

        //Don't load for the time being while in INDEV
        public override bool IsLoadingEnabled(Mod mod) => LivingWorldMod.IsDebug;

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

            //Search for pyramid location
            for (int i = (int)(Main.maxTilesX * 0.1f); i < Main.maxTilesY * 0.9f; i++) {
                progress.Set(i / (Main.maxTilesY * 0.9f));
                for (int j = (int)(Main.worldSurface * 0.5); j < Main.worldSurface * 1.25f; j++) {
                    Dictionary<ushort, int> tileData = new Dictionary<ushort, int>();
                    WorldUtils.Gen(new Point(i, j), new Shapes.Rectangle(51, 51), new Actions.TileScanner(TileID.Sand).Output(tileData));

                    //If this area contains 500 blocks of sand, it is a valid spot; generate here
                    if (tileData[TileID.Sand] > 500) {
                        StructureData pyramidStructure = IOUtils.GetStructureFromFile(LivingWorldMod.LWMStructurePath + "Dungeons/OverworldPyramid.struct");

                        WorldGenUtils.GenerateStructure(pyramidStructure, i, j);
                        return;
                    }
                }
            }

            //If we got this far, then nothing valid was found; drop warning in console
            ModContent.GetInstance<LivingWorldMod>().Logger.Warn("Revamped Pyramid unable to generate due to no valid placement.");
        }
    }
}