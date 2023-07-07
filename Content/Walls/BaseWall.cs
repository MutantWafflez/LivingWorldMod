using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Walls {
    /// <summary>
    /// Base class for all LWM Walls that has some functionality for the streamlining of the wall
    /// creation process.
    /// </summary>
    public abstract class BaseWall : ModWall {
        /// <summary>
        /// What Color is used to display this Wall on the Map.
        /// Return null if you wish for this wall to not be displayed
        /// on the map at all. Returns null by default.
        /// </summary>
        /// <remarks>
        /// Make sure to call base.SetStaticDefaults() if you override SetStaticDefaults, so the
        /// automatic map handling is run.
        /// </remarks>
        public virtual Color? WallColorOnMap => null;

        public override string Texture => GetType()
                                          .Namespace?
                                          .Replace($"{nameof(LivingWorldMod)}.Content.", LivingWorldMod.LWMSpritePath)
                                          .Replace('.', '/')
                                          + $"/{Name}";


        public override void SetStaticDefaults() {
            if (WallColorOnMap is null) {
                return;
            }

            LocalizedText name = CreateMapEntryName();
            //AKA check if the localization for this wall exists, and only add it if it does
            //Translations will return the key if you try to get the translation value for a translation that doesn't exist.
            if (name.Value == name.Key) {
                AddMapEntry(WallColorOnMap!.Value);
            }
            else {
                AddMapEntry(WallColorOnMap!.Value, name);
            }
        }
    }
}