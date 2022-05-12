using System.IO;
using LivingWorldMod.Common.ModTypes;
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
                return false;
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
        public static string LWMStructurePath => "Content/Structures";

        /// <summary>
        /// Directory of the Music files for LivingWorldMod.
        /// </summary>
        public static string LWMMusicPath => "Assets/Audio/Music/";

        public override void HandlePacket(BinaryReader reader, int whoAmI) {
            byte handlerType = reader.ReadByte();

            if (PacketHandler.GetHandler(handlerType) is { } handler) {
                handler.HandlePacket(reader, whoAmI);
            }
        }
    }
}