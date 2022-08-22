using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace LivingWorldMod.Custom.Classes {
    /// <summary>
    /// Abstract class for creating "cutscenes" with the player.
    /// </summary>
    public abstract class Cutscene {
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
    }
}