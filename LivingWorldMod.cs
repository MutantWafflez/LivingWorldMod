using LivingWorldMod.Common.ModTypes;
using System.IO;
#if !DEBUG
using LivingWorldMod.Common.Configs;
#endif
using Terraria.ModLoader;

namespace LivingWorldMod {
    public class LivingWorldMod : Mod {
        /// <summary>
        /// Whether or not the mod is in Debug, which is determined by if you are building from some
        /// IDE as Debug.
        /// </summary>
        public static bool IsDebug {
            get {
                #if DEBUG
                return true;
                #else
                return ModContent.GetInstance<DebugConfig>().forceDebugMode;
                #endif
            }
        }

        /// <summary>
        /// Directory of the Sprites for LivingWorldMod.
        /// </summary>
        public static string LWMSpritePath => nameof(LivingWorldMod) + "/Assets/Sprites/";

        /// <summary>
        /// Directory of the Structure files for LivingWorldMod.
        /// </summary>
        public static string LWMStructurePath => "Content/Structures/";

        /// <summary>
        /// Directory of the Music files for LivingWorldMod.
        /// </summary>
        public static string LWMMusicPath => "Assets/Audio/Music/";

        /// <summary>
        /// Directory of sound files for LivingWorldMod.
        /// </summary>
        public static string LWMSoundPath => nameof(LivingWorldMod) + "/Assets/Audio/Sounds/";

        public override void HandlePacket(BinaryReader reader, int whoAmI) {
            byte handlerType = reader.ReadByte();

            if (PacketHandler.GetHandler(handlerType) is { } handler) {
                handler.HandlePacket(reader, whoAmI);
            }
        }
    }
}