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
            string lwmPath = ModLoader.ModPath.Replace("Mods", "LivingWorldMod");

            if (!Directory.Exists(lwmPath)) {
                Directory.CreateDirectory(lwmPath);
            }

            return lwmPath;
        }

        /// <summary>
        /// Shorthand method for getting a <seealso cref="TagSerializable"/> instance from a file. The path parameter
        /// should not include the mod folder. For example, in the path
        /// "LivingWorldMod/Content/Structures/ExampeStructure.struct" it should not include the
        /// "LivngWorldMod" part, it just needs the "Content/Structures/ExampleStructure.struct" part.
        /// </summary>
        /// <param name="path"> The path in the LivingWorldMod folder to go to. </param>
        /// <returns> </returns>
        public static T GetTagFromFile<T>(string path)
            where T : TagSerializable {
            LivingWorldMod modInstance = ModContent.GetInstance<LivingWorldMod>();

            Stream fileStream = modInstance.GetFileStream(path);

            T structureData = TagIO.FromStream(fileStream).Get<T>(typeof(T).Name);

            fileStream.Close();

            return structureData;
        }
    }
}