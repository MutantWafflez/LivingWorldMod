using LivingWorldMod.Custom.Structs;
using System.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Custom.Utilities {

    /// <summary>
    /// Utilities class that handles reading, writing, and general I/O management.
    /// </summary>
    public static class IOUtils {

        /// <summary>
        /// Returns the LWM File path in the ModLoader folder, and creates the directory if it does
        /// not exist.
        /// </summary>
        /// <returns> </returns>
        public static string GetLWMFilePath() {
            string LWMPath = ModLoader.ModPath.Replace("Mods", "LivingWorldMod");

            if (!Directory.Exists(LWMPath)) {
                Directory.CreateDirectory(LWMPath);
            }

            return LWMPath;
        }

        /// <summary>
        /// Shorthand method for getting a StructureData instance from a file. The path parameter
        /// should not include the mod folder. For example, in the path
        /// "LivingWorldMod/Content/Structures/ExampeStructure.struct" it should not include the
        /// "LivngWorldMod" part, it just needs the "Content/Structures/ExampleStructure.struct" part.
        /// </summary>
        /// <param name="structurePath"> The path in the LivingWorldMod folder to go to. </param>
        /// <returns> </returns>
        public static StructureData GetStructureFromFile(string path) {
            LivingWorldMod modInstance = ModContent.GetInstance<LivingWorldMod>();

            Stream fileStream = modInstance.GetFileStream(path);

            StructureData structureData = TagIO.FromStream(fileStream).Get<StructureData>("structureData");

            fileStream.Close();

            return structureData;
        }
    }
}