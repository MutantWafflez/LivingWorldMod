using Terraria.Localization;
using Terraria.Utilities;

namespace LivingWorldMod.Custom.Utilities {

    /// <summary>
    /// Utilities class which holds methods that deals with localization.
    /// </summary>
    public static class LocalizationUtilities {

        /// <summary>
        /// Shortcut for getting LivingWorldMod localization strings, where the "key" is the
        /// direction within the Mods.LivingWorldMod. For example, to get a Villager name
        /// localization string, you would normally have to use the key
        /// "Mods.LivingWorldMod.VillagerType.Harpy", but with this method all you need to use for
        /// the key is "VillagerType.Harpy".
        /// </summary>
        /// <param name="key">
        /// The key for the specified localization string starting at the LivingWorldMod base directory.
        /// </param>
        public static string GetLWMTextValue(string key, params object[] args) => Language.GetTextValue("Mods.LivingWorldMod." + key, args);

        /// <summary>
        /// Retrieves a list of strings from a category within the localization file. The
        /// "categoryPath" parameter should be the path of the category starting from
        /// "LivingWorldMod" category in the .hjson file. For example, if you want to retrieve the
        /// all of the dialogue strings for Harpy Villagers' Dislike dialogue, the path would be "VillagerDialogue.Harpy.Dislike"
        /// </summary>
        /// <param name="categoryPath">
        /// The path of the dialogue category, with the "LivingWorldMod" category as a base.
        /// </param>
        /// <returns> </returns>
        public static WeightedRandom<string> GetAllStringsFromCategory(string categoryPath) {
            string path = $"Mods.LivingWorldMod.{categoryPath}";

            WeightedRandom<string> list = new WeightedRandom<string>();

            int index = 0;

            while (true) {
                if (Language.GetTextValue($"{path}.{index}") == $"{path}.{index}" &&
                    Language.GetTextValue($"{path}.{index}.Text") == $"{path}.{index}.Text") {
                    break;
                }

                string potentialWeight = Language.GetTextValue($"{path}.{index}.Weight");

                if (potentialWeight != $"{path}.{index}.Weight") {
                    if (double.TryParse(potentialWeight, out double result)) {
                        list.Add(Language.GetTextValue($"{path}.{index}.Text"), result);
                    }
                    else {
                        list.Add(Language.GetTextValue($"{path}.{index}.Text"));
                    }
                }
                else {
                    list.Add(Language.GetTextValue($"{path}.{index}"));
                }

                index++;
            }

            return list;
        }
    }
}