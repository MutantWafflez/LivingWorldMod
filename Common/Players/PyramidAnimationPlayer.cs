using LivingWorldMod.Common.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Players {
    /// <summary>
    /// ModPlayer that exclusively handles doing custom drawing with player layers
    /// with the Pyramid Door animation.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class PyramidAnimationPlayer : ModPlayer {
        private PyramidDoorSystem DoorSystem => ModContent.GetInstance<PyramidDoorSystem>();

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
            if (DoorSystem.DoorAnimationPhase == PyramidDoorSystem.PlayerWalkingIntoDoorPhase) {
                //These three lines are helpful vanilla code
                int yFrame = (int)(Main.GameUpdateCount / 0.07f) % 14 + 6;
                Player.bodyFrame.Y = Player.legFrame.Y = Player.headFrame.Y = yFrame * 56;

                float currentStep = DoorSystem.DoorAnimationTimer / 240f;
                //This is really disgusting, and I don't like it, but I don't think there's really any other choice, sadly. Vanilla :-(
                //(If someone comes across this and tells me how to do this much cleaner, please do tell)
                LerpToTransparentBlack(ref drawInfo.colorArmorHead, currentStep);
                LerpToTransparentBlack(ref drawInfo.colorArmorBody, currentStep);
                LerpToTransparentBlack(ref drawInfo.colorArmorLegs, currentStep);
                LerpToTransparentBlack(ref drawInfo.colorBodySkin, currentStep);
                LerpToTransparentBlack(ref drawInfo.colorHead, currentStep);
                LerpToTransparentBlack(ref drawInfo.colorHair, currentStep);
                LerpToTransparentBlack(ref drawInfo.colorEyes, currentStep);
                LerpToTransparentBlack(ref drawInfo.colorEyeWhites, currentStep);
                LerpToTransparentBlack(ref drawInfo.colorLegs, currentStep);
                LerpToTransparentBlack(ref drawInfo.colorPants, currentStep);
                LerpToTransparentBlack(ref drawInfo.colorShirt, currentStep);
                LerpToTransparentBlack(ref drawInfo.colorUnderShirt, currentStep);
                LerpToTransparentBlack(ref drawInfo.colorShoes, currentStep);
                LerpToTransparentBlack(ref drawInfo.bodyGlowColor, currentStep);
                LerpToTransparentBlack(ref drawInfo.armGlowColor, currentStep);
                LerpToTransparentBlack(ref drawInfo.headGlowColor, currentStep);
                LerpToTransparentBlack(ref drawInfo.legsGlowColor, currentStep);
            }
        }

        public override void PreUpdateMovement() {
            //Prevent movement during animation

            if (DoorSystem.DoorBeingOpenedPosition != Point16.NegativeOne) {
                Player.velocity = Vector2.Zero;

                Player.mount.Dismount(Player);
                Player.gravDir = 1f;

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

                Player.direction = 1;
            }
        }

        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) {
            //Player is invincible during the door opening animation
            if (DoorSystem.DoorBeingOpenedPosition != Point16.NegativeOne) {
                return false;
            }

            return base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource);
        }

        private void LerpToTransparentBlack(ref Color color, float step) {
            Color transparentBlack = Color.Black;
            transparentBlack.A = 255;

            color = Color.Lerp(color, transparentBlack, step);
        }
    }
}