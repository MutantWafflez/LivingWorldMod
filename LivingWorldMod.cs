global using Terraria;
global using Terraria.ModLoader;
global using Terraria.ID;
global using LWM = LivingWorldMod.LivingWorldMod;
using System.IO;
using LivingWorldMod.Globals.Configs;
using LivingWorldMod.Globals.ModTypes;

namespace LivingWorldMod;

public class LivingWorldMod : Mod {
    /// <summary>
    ///     Whether or not to enable IL edit patches on load.
    ///     Do NOT change unless you know what you're doing!
    /// </summary>
    public const bool EnableILPatches = true;

    /// <summary>
    ///     Whether or not the mod is in Debug, which is determined by if you are building from some
    ///     IDE as Debug.
    /// </summary>
    public static bool IsDebug {
        get {
            bool isDebug = ModContent.GetInstance<DebugConfig>().forceDebugMode;

            #if DEBUG
            isDebug = true;
            #endif

            return isDebug;
        }
    }

    /// <summary>
    ///     Directory of the Sprites for LivingWorldMod.
    /// </summary>
    public static string SpritePath => nameof(LivingWorldMod) + "/Assets/Sprites/";

    /// <summary>
    ///     Directory of the Music files for LivingWorldMod.
    /// </summary>
    public static string MusicPath => "Assets/Audio/Music/";

    public override void HandlePacket(BinaryReader reader, int whoAmI) {
        byte handlerType = reader.ReadByte();

        if (PacketHandler.GetHandler(handlerType) is { } handler) {
            handler.HandlePacket(reader, whoAmI);
        }
    }
}