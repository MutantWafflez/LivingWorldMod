using Terraria.ModLoader;

namespace LivingWorldMod {

    public class LivingWorldMod : Mod {

        /// <summary>
        /// Directory of the Sprites for LivingWorldMod.
        /// </summary>
        public static string LWMSpritePath => nameof(LivingWorldMod) + "/Assets/Sprites";

        /// <summary>
        /// Directory of the Structure files for LivingWorldMod.
        /// </summary>
        public static string LWMStructurePath => "Content/Structures";

        /// <summary>
        /// Directory of the Music files for LivingWorldMod.
        /// </summary>
        public static string LWMMusicPath => "Assets/Audio/Music/";
    }
}