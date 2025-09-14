using System;
using System.Linq;
using LivingWorldMod.Content.TownNPCRevitalization.AIStates;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs.Animations;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.BaseTypes.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Hooks;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;

/// <summary>
///     Town NPC Module that deals specifically with animations/framing for Town NPCs. This module handles no additional drawing; that is for <see cref="TownNPCSpriteModule" /> to handle.
/// </summary>
public class TownNPCAnimationModule : TownNPCModule, IOnTownNPCAttack {
    // Walk frame info
    private const int TownDogWalkStartFrame = 9;
    private const int TownBunnyWalkStartFrame = 1;
    private const int DefaultWalkStartFrame = 2;

    private const int TownDogAndBunnyWalkFrameDuration = 12;
    private const int DefaultWalkFrameDuration = 6;

    // Vertical frame info
    private const int TownDogVerticalMovementFrame = 8;
    private const int DefaultVerticalMovementFrame = 1;

    // Idle animation info
    private const int DogIdleTailWagAnimationFrameDuration = 4;
    private const int DogIdleTailWagAnimationEndFrame = 7;
    private const int AttackAnimationPriority = -2;

    private static readonly LoopingAnimation DogIdleTailWagAnimation = new(
        0,
        DogIdleTailWagAnimationEndFrame,
        DogIdleTailWagAnimationFrameDuration,
        IsDogIdleTailWagAnimationFinished,
        priority: 1
    );

    private IAnimation _currentAnimation;
    private bool _animationStarted;

    public override int UpdatePriority => -2;

    public static SingleFrameAnimation GetVerticalMovementAnimation(in NPC npc, SingleFrameAnimation.AnimationFinishedPredicate animationFinished = null) {
        int fallingFrame = npc.type switch {
            NPCID.TownDog => TownDogVerticalMovementFrame,
            _ => DefaultVerticalMovementFrame
        };

        return new SingleFrameAnimation(fallingFrame, animationFinished ?? IsVerticalMovementAnimationFinished, -1);
    }

    private static bool IsVerticalMovementAnimationFinished(in NPC npc) => npc.velocity.Y == 0f;

    private static float WalkingAnimationTickRate(in NPC npc) => Math.Abs(npc.velocity.X) * 2f + 1f;

    private static bool IsWalkingAnimationFinished(in NPC npc) => npc.velocity.X == 0f;

    private static bool IsDogIdleTailWagAnimationFinished(in NPC npc) => npc.velocity.LengthSquared() != 0f;

    private static float MeleeAttackAnimationProgress(in NPC npc) => (int)npc.ai[0] == MeleeAttackAIState.StateInteger ? 1f - npc.ai[1] / NPCID.Sets.AttackTime[npc.type] : 1f;

    private static bool FirearmAnimationFinished(in NPC npc) => (int)npc.ai[0] != FirearmAttackAIState.StateInteger;

    public override void UpdateModule() {
        if (NPC.velocity.Y != 0f) {
            RequestAnimation(GetVerticalMovementAnimation(NPC));

            return;
        }

        if (NPC.velocity.X == 0f) {
            RequestDogIdleAnimation();

            return;
        }

        // Walk animation
        RequestAnimation(
            new LoopingAnimation(
                NPC.type switch {
                    NPCID.TownDog => TownDogWalkStartFrame,
                    NPCID.TownBunny => TownBunnyWalkStartFrame,
                    _ => DefaultWalkStartFrame
                },
                Main.npcFrameCount[NPC.type] - NPCID.Sets.ExtraFramesCount[NPC.type] - 1,
                NPC.type switch {
                    NPCID.TownDog or NPCID.TownBunny => TownDogAndBunnyWalkFrameDuration,
                    _ => DefaultWalkFrameDuration
                },
                IsWalkingAnimationFinished,
                WalkingAnimationTickRate
            )
        );
    }

    public override void FindFrame(NPC npc, int frameHeight) {
        NPC.spriteDirection = npc.direction;

        if (_currentAnimation is null) {
            return;
        }

        if (!_animationStarted) {
            _currentAnimation.Start(npc, frameHeight);

            _animationStarted = true;
        }

        if (!_currentAnimation.Update(npc, frameHeight)) {
            return;
        }

        _currentAnimation = null;
        _animationStarted = false;
        npc.frame.Y = (int)(npc.frameCounter = 0);
    }

    public void RequestAnimation(IAnimation newAnimation) {
        if (_currentAnimation is not null && _currentAnimation.Priority <= newAnimation.Priority) {
            return;
        }

        _animationStarted = false;
        _currentAnimation = newAnimation;
    }

    public void OnTownNPCAttack(NPC npc) {
        int nonAttackFrameCount = Main.npcFrameCount[NPC.type] - NPCID.Sets.AttackFrameCount[NPC.type];
        int attackFrameCount = NPCID.Sets.AttackFrameCount[NPC.type];

        switch (npc.ai[0]) {
            case NurseHealAIState.StateInteger:
            case ThrowAttackAIState.StateInteger: {
                int subsequentFrameDuration = npc.type is NPCID.BestiaryGirl ? 2 : 6;

                RequestAnimation(
                    new LinearAnimation(
                        Enumerable.Range(nonAttackFrameCount, attackFrameCount).ToArray(),
                        Enumerable.Repeat(subsequentFrameDuration, attackFrameCount).ToArray(),
                        AttackAnimationPriority
                    )
                );

                break;
            }
            case FirearmAttackAIState.StateInteger: {
                RequestAnimation(new SingleFrameAnimation(nonAttackFrameCount + npc.GetShootingFrame(NPC.ai[2]), FirearmAnimationFinished, AttackAnimationPriority));

                break;
            }
            case MagicAttackAIState.StateInteger: {
                break;
            }
            case MeleeAttackAIState.StateInteger: {
                RequestAnimation(
                    new PercentageAnimation(
                        Enumerable.Range(nonAttackFrameCount, attackFrameCount).ToArray(),
                        [0f, 0.35f, 0.5f, 0.85f],
                        MeleeAttackAnimationProgress,
                        AttackAnimationPriority
                    )
                );

                break;
            }
        }
    }

    private void RequestDogIdleAnimation() {
        if (NPC.type is not NPCID.TownDog) {
            return;
        }

        RequestAnimation(DogIdleTailWagAnimation);
    }
}