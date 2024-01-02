using System.IO;
using Hjson;

namespace LivingWorldMod.Custom.Utilities;

// Utilities class that assists with JSON files.
public static partial class Utilities {
    /// <summary>
    /// Gets and returns the json data from the specified file path. This is for specifically
    /// LWM, so the file path does not need to include "LivingWorldMod".
    /// </summary>
    public static JsonValue GetJSONFromFile(string filePath) {
        Stream jsonStream = ModContent.GetInstance<LWM>().GetFileStream(filePath);
        JsonValue jsonData = JsonValue.Load(jsonStream);
        jsonStream.Close();

        return jsonData;
    }
}