﻿using System.IO;
using LivingWorldMod.Common.ModTypes;
using LivingWorldMod.Common.Players;
using LivingWorldMod.Content.Subworlds;
using LivingWorldMod.Content.Tiles.Interactables;
using Microsoft.Xna.Framework;
using SubworldLibrary;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Cutscenes {
    /// <summary>
    /// Cutscene that plays when the player right clicks on the Revamped Pyramid
    /// door, where the door dramatically opens and the player walks in.
    /// </summary>
    public class EnterPyramidCutscene : Cutscene {
        /// <summary>
        /// The position in tiles of the door currently being opened to enter.
        /// </summary>
        public Point16 DoorBeingOpenedPosition {
            get;
            private set;
        }

        /// <summary>
        /// The "Phase" of the opening animation the specified door is currently in. Starts
        /// at 1.
        /// </summary>
        public int DoorAnimationPhase {
            get;
            private set;
        } = 1;

        /// <summary>
        /// The timer for the door opening process.
        /// </summary>
        public int DoorAnimationTimer {
            get;
            private set;
        }

        /// <summary>
        /// The very last phase of the door animation before the door is completely open.
        /// </summary>
        public const int LastDoorAnimationPhase = 5;

        /// <summary>
        /// The phase that is simply a brief "pause" where the player stands still
        /// after the door is fully, visually open, before they actually enter the
        /// door.
        /// </summary>
        public const int PauseBeforePlayerWalkingPhase = 6;

        /// <summary>
        /// The phase of the door opening animation where the player is meant
        /// to visual "walk into" the open door.
        /// </summary>
        public const int PlayerWalkingIntoDoorPhase = 7;

        /// <summary>
        /// How many ticks it takes for the door to visually update from phase to phase.
        /// </summary>
        public const int DoorOpeningRate = 33;

        /// <summary>
        /// How many ticks it takes before the player begins visually entering the door after the door
        /// has completely opened.
        /// </summary>
        public const int WaitTimeBeforeWalkIn = 60;

        /// <summary>
        /// How long it takes for the player to fully "walk into" the open door.
        /// </summary>
        public const int PlayerWalkingTime = 240;

        public static readonly SoundStyle OpeningDoorSound = new SoundStyle($"{LivingWorldMod.LWMSoundPath}Tiles/PyramidDoorOpen");

        public EnterPyramidCutscene(Point16 doorPos) {
            DoorBeingOpenedPosition = doorPos;
        }

        // Here for autoloading; doesn't register otherwise
        private EnterPyramidCutscene() { }

        protected override void WritePacketData(ModPacket packet) {
            packet.WriteVector2(DoorBeingOpenedPosition.ToVector2());
        }

        public override void HandleCutscenePacket(BinaryReader reader, int fromWhomst) {
            if (Main.netMode == NetmodeID.Server) {
                Player player = Main.player[fromWhomst];
                DoorBeingOpenedPosition = reader.ReadVector2().ToPoint16();

                if (Framing.GetTileSafely(DoorBeingOpenedPosition).TileType != ModContent.TileType<PyramidDoorTile>() || !player.active) {
                    return;
                }

                player.GetModPlayer<CutscenePlayer>().StartCutscene(this);
            }
            else {
                Player player = Main.player[reader.ReadInt32()];
                DoorBeingOpenedPosition = reader.ReadVector2().ToPoint16();

                if (!player.active) {
                    return;
                }

                player.GetModPlayer<CutscenePlayer>().StartCutscene(this);
            }
        }

        public override void OnStart(Player player) {
            if (player.whoAmI == Main.myPlayer) {
                Vector2 telePos = DoorBeingOpenedPosition.ToWorldCoordinates(22, 22);
                player.Teleport(telePos, -1);
                NetMessage.SendData(MessageID.Teleport, number: 0, number2: player.whoAmI, number3: telePos.X, number4: telePos.Y, number5: -1);
            }
            ModContent.GetInstance<PyramidDoorTile>().playerInCutscene = player;

            SoundEngine.PlaySound(OpeningDoorSound with { Pitch = -1f }, DoorBeingOpenedPosition.ToWorldCoordinates(32));
        }

        public override void Update(Player player) {
            if (DoorAnimationPhase < PlayerWalkingIntoDoorPhase) {
                if (++DoorAnimationTimer >= (DoorAnimationPhase != PauseBeforePlayerWalkingPhase ? DoorOpeningRate : WaitTimeBeforeWalkIn)) {
                    DoorAnimationPhase++;
                    DoorAnimationTimer = 0;
                }
            }
            else if (DoorAnimationPhase == PlayerWalkingIntoDoorPhase) {
                if (++DoorAnimationTimer >= PlayerWalkingTime) {
                    DoorBeingOpenedPosition = Point16.NegativeOne;
                    DoorAnimationPhase = 0;
                    DoorAnimationTimer = 0;

                    IsFinished = true;
                }
            }
        }

        public override void OnFinish(Player player) {
            if (Main.netMode == NetmodeID.Server) {
                return;
            }
            ModContent.GetInstance<PyramidDoorTile>().playerInCutscene = null;
            if (player.whoAmI != Main.myPlayer) {
                return;
            }

            ModContent.GetInstance<PyramidSubworld>().GenerateNewRandom(WorldGen.genRand.Next(int.MaxValue));
            SubworldSystem.Enter<PyramidSubworld>();
        }

        public override void ModifyPlayerDrawInfo(Player player, ref PlayerDrawSet drawInfo) {
            if (DoorAnimationPhase == PlayerWalkingIntoDoorPhase) {
                //These lines are helpful vanilla code
                int yFrame = (int)(Main.GameUpdateCount / 0.07f) % 14 + 6;
                player.bodyFrame.Y = player.legFrame.Y = player.headFrame.Y = yFrame * 56;
                player.WingFrame(false);

                float currentStep = DoorAnimationTimer / (float)PlayerWalkingTime;
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

        private void LerpToTransparentBlack(ref Color color, float step) {
            Color transparentBlack = Color.Black;
            transparentBlack.A = 255;

            color = Color.Lerp(color, transparentBlack, step);
        }
    }
}