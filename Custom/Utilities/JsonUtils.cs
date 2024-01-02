using System.IO;
using Hjson;
using Terraria.ModLoader;

namespace LivingWorldMod.Custom.Utilities;

/// <summary>
/// Utilities class that assists with JSON files.
/// </summary>
public static class JsonUtils {
    /// <summary>
    /// Gets and returns the json data from the specified file path. This is for specifically
    /// LWM, so the file path does not need to include "LivingWorldMod".
    /// </summary>
    public static JsonValue GetJSONFromFile(string filePath) {
        Stream jsonStream = ModContent.GetInstance<LivingWorldMod>().GetFileStream(filePath);
        JsonValue jsonData = JsonValue.Load(jsonStream);
        jsonStream.Close();

        return jsonData;
    }
}