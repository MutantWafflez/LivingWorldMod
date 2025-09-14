using System;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs.Animations;

/// <summary>
///     Represents a simple, non-looping animation following a linear route throughout the provided frames.
/// </summary>
public struct LinearAnimation : IAnimation {
    private readonly int[] _frames;
    private readonly int[] _frameDurations;
    private readonly int _frameCount;

    private int _currentAnimationStage;

    public int Priority {
        get;
    }

    public LinearAnimation(int[] frames, int[] frameDurations, int priority = 0) {
        ArgumentNullException.ThrowIfNull(frames);
        ArgumentNullException.ThrowIfNull(frameDurations);

        if (frames.Length != frameDurations.Length) {
            throw new ArgumentException("Frames and FrameDuration arrays must be the same length!", nameof(frames));
        }

        if (frames.Length == 0) {
            throw new ArgumentException("Arrays cannot have length of 0!", nameof(frames));
        }

        _frames = frames;
        _frameDurations = frameDurations;
        _frameCount = frames.Length;
        _currentAnimationStage = 0;

        Priority = priority;
    }

    public void Start(NPC npc, int frameHeight) {
        npc.frame.Y = _frames[0] * frameHeight;
        _currentAnimationStage = (int)(npc.frameCounter = 0);
    }

    public bool Update(NPC npc, int frameHeight) {
        npc.frameCounter++;

        if (npc.frameCounter < _frameDurations[_currentAnimationStage]) {
            return false;
        }

        if (++_currentAnimationStage >= _frameCount) {
            return true;
        }

        npc.frame.Y = _frames[_currentAnimationStage] * frameHeight;
        npc.frameCounter = 0;

        return false;
    }
}