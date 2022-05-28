﻿using LivingWorldMod.Common.Systems;
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
        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
            PyramidDoorSystem doorSystem = ModContent.GetInstance<PyramidDoorSystem>();

            if (doorSystem.DoorOpeningPhase == 5) {
                //These three lines are helpful vanilla code
                int yFrame = (int)(Main.GlobalTimeWrappedHourly / 0.07f) % 14 + 6;
                Player.bodyFrame.Y = Player.legFrame.Y = Player.headFrame.Y = yFrame * 56;
                Player.WingFrame(false);

                float currentStep = doorSystem.DoorOpeningTimer / 240f;
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

        public override void PostUpdateRunSpeeds() {
            //Prevent movement during animation
            PyramidDoorSystem doorSystem = ModContent.GetInstance<PyramidDoorSystem>();

            if (doorSystem.DoorOpeningPhase == 5) {
                Player.velocity = Vector2.Zero;
                Player.wingTime = 0;
            }
        }

        private void LerpToTransparentBlack(ref Color color, float step) {
            Color transparentBlack = Color.Black;
            transparentBlack.A = 255;

            color = Color.Lerp(color, transparentBlack, step);
        }
    }
}