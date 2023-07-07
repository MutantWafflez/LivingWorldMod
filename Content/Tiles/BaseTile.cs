using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Tiles {
    /// <summary>
    /// Simple class that is the base class for all tiles in this mod. Has a couple pieces of functionality
    /// to make the tile creation process easier.
    /// </summary>
    public abstract class BaseTile : ModTile {
        /// <summary>
        /// What Color is used to display this tile on the Map.
        /// Return null if you wish for this tile to not be displayed
        /// on the map at all. Returns null by default.
        /// </summary>
        public virtual Color? TileColorOnMap => null;

        public override string Texture => GetType()
                                          .Namespace?
                                          .Replace($"{nameof(LivingWorldMod)}.Content.", LivingWorldMod.LWMSpritePath)
                                          .Replace('.', '/')
                                          + $"/{Name}";

        /// <summary>
        /// Allows you to override some default properties of this tile, such as Main.tileNoSunLight and Main.tileObsidianKill.
        /// </summary>
        /// <remarks>
        /// For LWM, also automatically attempts to grab the localization key for this tile and add it,
        /// assuming that the key exists; if it doesn't, then hovering over the tile will display nothing.
        /// </remarks>
        public override void PostSetDefaults() {
            if (TileColorOnMap is null) {
                return;
            }

            LocalizedText name = CreateMapEntryName();
            //AKA check if the localization for this tile exists, and only add it if it does
            //Translations will return the key if you try to get the translation value for a translation that doesn't exist.
            if (name.Value == name.Key) {
                AddMapEntry(TileColorOnMap!.Value);
            }
            else {
                AddMapEntry(TileColorOnMap!.Value, name);
            }
        }
    }
}