using System.IO;
using Hjson;

namespace LivingWorldMod.Utilities;

// Utilities class that assists with JSON files.
public static partial class LWMUtils {
    /// <summary>
    ///     Gets and returns the json data from the specified file path. This is for specifically
    ///     LWM, so the file path does not need to include "LivingWorldMod".
    /// </summary>
    public static JsonValue GetJSONFromFile(string filePath) {
        Stream jsonStream = LWM.Instance.GetFileStream(filePath);
        JsonValue jsonData = JsonValue.Load(jsonStream);
        jsonStream.Close();

        return jsonData;
    }
}