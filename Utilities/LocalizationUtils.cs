﻿using Terraria.Localization;

namespace LivingWorldMod.Utilities;

// Utilities class which holds methods that deals with localization.
public static partial class LWMUtils {
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
    /// Extension for strings that will turn the provided string into its <see cref="LocalizedText"/> equivalent, assuming the
    /// provided string is the key for a <see cref="LWM"/> localization file text value.
    /// </summary>
    public static LocalizedText Localized(this string key) => ModContent.GetInstance<LWM>().GetLocalization(key);
}