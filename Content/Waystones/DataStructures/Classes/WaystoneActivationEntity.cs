﻿using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria.Audio;

namespace LivingWorldMod.Content.Waystones.DataStructures.Classes;

/// <summary>
///     Psuedo-Entity that exists during the Activation Process for Waystones. This exists mainly for consistency between
///     multiplayer and singleplayer, since Tile Entities updating on the client is a bit weird.
/// </summary>
public sealed class WaystoneActivationEntity (Vector2 position, Color activationColor) {
    /// <summary>
    ///     How many ticks it takes to fully complete the activation process.
    /// </summary>
    public const int FullActivationWaitTime = 372;

    /// <summary>
    ///     Whether or not the activation process for this specific entity has finished.
    /// </summary>
    public bool isFinished;

    private int _activationVFXStage;
    private int _activationVFXTimer;
    private int _activationVFXSecondaryTimer;

    public void Update() {
        //If this is finished, don't update
        if (isFinished) {
            return;
        }

        float circleRadius = 160f;

        if (_activationVFXStage == 0) {
            // Generate circle of dust. Would use the Utils method that was made for this, but this is "special" drawing
            for (int x = 0; x <= _activationVFXTimer; x++) {
                Dust.NewDustPerfect(position - new Vector2(0, circleRadius).RotatedBy(MathHelper.ToRadians(x * 20f)), DustID.GoldCoin, newColor: activationColor);
            }

            if (++_activationVFXSecondaryTimer > 18) {
                // Every 18 frames, add a particle to the circle
                _activationVFXSecondaryTimer = 0;
                _activationVFXTimer++;

                SoundEngine.PlaySound(SoundID.Item100, position - new Vector2(0, circleRadius).RotatedBy(MathHelper.ToRadians(_activationVFXTimer * 20f)));
            }

            if (_activationVFXTimer >= 18) {
                // After 18 particles are created, move to next stage
                _activationVFXStage = 1;
                _activationVFXTimer = 0;
                _activationVFXSecondaryTimer = 0;
            }
        }
        else if (_activationVFXStage == 1) {
            // Drag circle to the center of the tile
            int circlePullThreshold = 30;
            int finaleThreshold = 5;

            LWMUtils.CreateCircle(position, circleRadius * (1f - _activationVFXTimer / (float)circlePullThreshold), DustID.GoldCoin, newColor: activationColor, angleChange: 20f);

            // Step RAPIDLY closer
            _activationVFXTimer++;

            // After center of tile is reached, complete activation
            if (_activationVFXTimer == circlePullThreshold) {
                // Play finale sound and give text confirmation
                SoundEngine.PlaySound(SoundID.Item113, position);

                Main.NewText("Event.WaystoneActivation".Localized(), LWMUtils.YellowErrorTextColor);
            }
            else if (_activationVFXTimer > circlePullThreshold + finaleThreshold) {
                // Internally end sequence
                isFinished = true;
            }
        }
    }
}