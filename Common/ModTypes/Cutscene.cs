using System.Collections.Generic;
using System.IO;
using System.Linq;
using LivingWorldMod.Core.PacketHandlers;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.ModTypes {
    /// <summary>
    /// Abstract class for creating "cutscenes" with the player.
    /// </summary>
    public abstract class Cutscene : ModType {
        /// <summary>
        /// Whether or not the player is able to take damage during the cutscene.
        /// </summary>
        public virtual bool InvincibleDuringCutscene => true;

        /// <summary>
        /// Whether or not the player is frozen in place and unable to input anything.
        /// </summary>
        public virtual bool LockPlayerControl => true;

        /// <summary>
        /// Whether or not this cutscene is finished. Setting this to true will end the cutscene and free the player.
        /// </summary>
        public bool IsFinished {
            get;
            protected set;
        }

        public byte Type {
            get;
            private set;
        }

        public static byte TypeCount {
            get;
            private set;
        }

        /// <summary>
        /// Gets and returns the cutscene that pertains to the specified type. Returns null if none are found.
        /// </summary>
        public static Cutscene GetCutsceneFromType(byte type) => ModContent.GetContent<Cutscene>().FirstOrDefault(cutscene => cutscene.Type == type);

        /// <summary>
        /// Called when a packet needs to be written for this specific cutscene type. Write all important data
        /// that needs to be synced for the cutscene to work properly across all clients. The sent data will
        /// be subsequently read in <seealso cref="HandleCutscenePacket"/>.
        /// </summary>
        /// <remarks>
        /// You only need to write to the passed in packet, not send it.
        /// </remarks>
        protected abstract void WritePacketData(ModPacket packet);

        /// <summary>
        /// Read the data that you sent from <seealso cref="WritePacketData"/> and change the necessary
        /// things within this instance.
        /// </summary>
        /// <param name="reader"> The stream holding the data you need to read from. </param>
        /// <param name="fromWhomst"> What client sent the packet. Only matters if receiving on the server. </param>
        public abstract void HandleCutscenePacket(BinaryReader reader, int fromWhomst);

        /// <summary>
        /// Allows you to do things when the cutscene first starts. Called once on every MP
        /// client and on the server. If you only want to do things on client, make sure
        /// to double check:
        /// <code>player.whoAmI = Main.myPlayer</code>
        /// </summary>
        public virtual void OnStart(Player player) { }

        /// <summary>
        /// Runs once a tick on both clients and the server. This is where
        /// the main code for updating the cutscene should be placed. The player parameter
        /// passed in is the player that is currently within this cutscene.
        /// </summary>
        public virtual void Update(Player player) { }

        /// <summary>
        /// Allows you to do things when the cutscene ends. Called once, right when the cutscene
        /// is finished on both the server and clients.
        /// </summary>
        public virtual void OnFinish(Player player) { }

        /// <summary>
        /// Identical to <seealso cref="ModPlayer.ModifyDrawInfo"/>.
        /// </summary>
        public virtual void ModifyPlayerDrawInfo(Player player, ref PlayerDrawSet drawInfo) { }


        public sealed override void SetupContent() => SetStaticDefaults();

        protected sealed override void Register() {
            ModTypeLookup<Cutscene>.Register(this);
            Type = TypeCount;
            TypeCount++;
        }

        public void SendCutscenePacket(int fromWhomst, int ignoreClient = -1) {
            //Doesn't need to function in SP, saves the boilerplate from having to do netmode checks elsewhere
            if (Main.netMode == NetmodeID.SinglePlayer) {
                return;
            }

            ModPacket packet = ModContent.GetInstance<CutscenePacketHandler>().GetPacket(CutscenePacketHandler.SyncCutsceneToAllClients);
            packet.Write(Type);
            if (Main.netMode == NetmodeID.Server) {
                packet.Write(fromWhomst);
            }
            WritePacketData(packet);

            packet.Send(ignoreClient: ignoreClient);
        }
    }
}