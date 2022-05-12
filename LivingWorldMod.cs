using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LivingWorldMod.Content.TileEntities.Interactables;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
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

        //TODO: Make this not a switch case
        public override void HandlePacket(BinaryReader reader, int whoAmI) {
            PacketType packetType = (PacketType)reader.ReadInt32();
            switch (packetType) {
                case PacketType.SyncWaystones:
                    switch (Main.netMode) {
                        case NetmodeID.Server: {
                            List<WaystoneEntity> waystones = TileEntity.ByID.Values.OfType<WaystoneEntity>().ToList();

                            foreach (WaystoneEntity entity in waystones) {
                                NetMessage.SendData(MessageID.TileSection, whoAmI, number: entity.Position.X - 1, number2: entity.Position.Y - 1, number3: 4, number4: 4);
                            }

                            break;
                        }
                    }
                    break;
                default:
                    Logger.Warn($"Invalid type of packet recieved: {packetType}");
                    break;
            }
        }
    }
}