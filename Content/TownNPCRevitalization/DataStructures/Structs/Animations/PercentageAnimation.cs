using System;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs.Animations;

/// <summary>
///     Animation type that functions on a percentage basis, where the frames are enumerated based on an arbitrary float percentage/progress value determined by a passed-in delegate.
///     Once the animation progress is >= 1f (100%), the animation is considered finished and will mark itself as complete.
///     Exists primarily due to some vanilla animations handling themselves this way.
/// </summary>
public readonly struct PercentageAnimation : IAnimation {
    public delegate float AnimationProgressDelegate(in NPC npc);

    private readonly int[] _frames;
    private readonly float[] _frameThresholds;
    private readonly int _frameCount;
    private readonly AnimationProgressDelegate _animationProgressDelegate;

    public int Priority {
        get;
    }

    public PercentageAnimation(int[] frames, float[] frameThresholds, AnimationProgressDelegate animationProgressDelegate, int priority = 0) {
        ArgumentNullException.ThrowIfNull(frames);
        ArgumentNullException.ThrowIfNull(frameThresholds);

        if (frames.Length != frameThresholds.Length) {
            throw new ArgumentException("Frames and FrameThresholds arrays must be the same length!", nameof(frames));
        }

        if (frames.Length == 0) {
            throw new ArgumentException("Arrays cannot have length of 0!", nameof(frames));
        }

        _frames = frames;
        _frameThresholds = frameThresholds;
        _frameCount = frames.Length;
        _animationProgressDelegate = animationProgressDelegate;
        Priority = priority;
    }

    public void Start(NPC npc, int frameHeight) {
        IAnimation.StartAnimationWithFrame(npc, frameHeight, _frames[0]);
    }

    public bool Update(NPC npc, int frameHeight) {
        float currentProgress = _animationProgressDelegate(in npc);
        if (currentProgress >= 1f) {
            return true;
        }

        int currentFrameIndex = _frameCount - 1;
        for (int i = 0; i < _frameCount - 1; i++) {
            if (currentProgress >= _frameThresholds[i + 1]) {
                continue;
            }

            currentFrameIndex = i;
            break;
        }

        npc.frame.Y = _frames[currentFrameIndex] * frameHeight;

        return false;
    }
}