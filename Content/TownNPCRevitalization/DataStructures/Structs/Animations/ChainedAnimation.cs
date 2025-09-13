using System;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs.Animations;

/// <summary>
///     Represents a meta-animation object that individually does not animation anything, but takes children animations that will play linear-ly until each one is considered finished.
/// </summary>
public struct ChainedAnimation : IAnimation {
    private readonly IAnimation[] _animations;

    private int _currentAnimationState;

    public int Priority {
        get;
    }

    public ChainedAnimation(int priority = 0, params IAnimation[] animations) {
        ArgumentNullException.ThrowIfNull(animations, nameof(animations));

        if (animations.Length <= 0) {
            throw new ArgumentException("Cannot initialize Chained Animation without at least one child animation!", nameof(animations));
        }

        _animations = animations;
        Priority = priority;
    }

    public void Start(NPC npc, int frameHeight) {
        _animations[0].Start(npc, frameHeight);
    }

    public bool Update(NPC npc, int frameHeight) {
        if (_currentAnimationState >= _animations.Length) {
            return true;
        }

        if (!_animations[_currentAnimationState].Update(npc, frameHeight)) {
            return false;
        }

        _currentAnimationState++;
        return false;
    }
}