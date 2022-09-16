using LivingWorldMod.Common.ModTypes;
using LivingWorldMod.Core.PacketHandlers;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Players {
    /// <summary>
    /// ModPlayer that works in tandem with the <seealso cref="Cutscene"/> class.
    /// </summary>
    public class CutscenePlayer : ModPlayer {
        /// <summary>
        /// The current cutscene this player is in. When this is null,
        /// the player is not within any cutscene.
        /// </summary>
        public Cutscene CurrentCutscene {
            get;
            private set;
        }

        /// <summary>
        /// Shorthand for whether or not the player is in a cutscene.
        /// </summary>
        public bool InCutscene => CurrentCutscene is not null;

        public override void PreUpdateMovement() {
            if (CurrentCutscene is not { LockPlayerControl: true }) {
                return;
            }

            Player.velocity = Vector2.Zero;

            Player.controlMount = false;
            Player.controlJump = false;
            Player.controlDown = false;
            Player.controlLeft = false;
            Player.controlRight = false;
            Player.controlUp = false;
            Player.controlUseItem = false;
            Player.controlUseTile = false;
            Player.controlThrow = false;
            Player.controlTorch = false;
            Player.controlInv = false;
        }

        public override void PostUpdate() {
            if (!InCutscene) {
                return;
            }

            CurrentCutscene.Update(Player);

            if (CurrentCutscene.IsFinished) {
                EndCutscene();
            }
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
            if (!InCutscene) {
                return;
            }

            CurrentCutscene.ModifyPlayerDrawInfo(Player, ref drawInfo);
        }

        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter) {
            if (CurrentCutscene is { InvincibleDuringCutscene: true }) {
                return false;
            }

            return base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource, ref cooldownCounter);
        }

        /// <summary>
        /// Granted that no other cutscene is currently running, starts the passed in
        /// cutscene on this player.
        /// </summary>
        public void StartCutscene(Cutscene cutscene) {
            if (InCutscene) {
                return;
            }

            CurrentCutscene = cutscene;
            CurrentCutscene.OnStart(Player);
        }

        /// <summary>
        /// Forcefully ends the current cutscene. Nothing occurs if there is no cutscene
        /// playing.
        /// </summary>
        public void EndCutscene() {
            if (!InCutscene) {
                return;
            }

            CurrentCutscene.OnFinish(Player);
            if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI == Main.myPlayer) {
                ModPacket packet = ModContent.GetInstance<CutscenePacketHandler>().GetPacket(CutscenePacketHandler.SyncCutsceneFinishToAllClients);
                packet.Send();
            }
            CurrentCutscene = null;
        }
    }
}