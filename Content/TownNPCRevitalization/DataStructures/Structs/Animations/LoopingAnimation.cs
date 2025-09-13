using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs.Animations;

/// <summary>
///     Simple animation object type that represents an equally-spaced, linear, and conditionally looping animation.
/// </summary>
/// <param name="startFrame"> The frame the animation will start on. </param>
/// <param name="endFrame"> The last frame the animation will be on BEFORE it loops.</param>
/// <param name="frameDuration"> How long, in ticks, each frame will last before moving to the next frame. </param>
/// <param name="animationFinished"> The predicate that determines whether this animation is considered finished. </param>
/// <param name="tickRate"> The flat rate at which this animation "ticks". By default, 1 animation for each in-game tick.</param>
/// <param name="priority"> The priority of this animation when requested in <see cref="TownNPCAnimationModule" />. </param>
public readonly struct LoopingAnimation(
    int startFrame,
    int endFrame,
    int frameDuration,
    LoopingAnimation.AnimationFinishedPredicate animationFinished,
    float tickRate = 1f,
    int priority = 0
) : IAnimation {
    public delegate bool AnimationFinishedPredicate(in NPC npc);

    public delegate float GetVariableTickRateDelegate(in NPC npc);

    private readonly GetVariableTickRateDelegate _tickRateDelegate = null;

    public int Priority {
        get;
    } = priority;

    public LoopingAnimation(
        int startFrame,
        int endFrame,
        int frameDuration,
        AnimationFinishedPredicate animationFinishedPredicate,
        GetVariableTickRateDelegate tickRateDelegate,
        int priority = 0
    ) : this(startFrame, endFrame, frameDuration, animationFinishedPredicate, priority: priority) {
        _tickRateDelegate = tickRateDelegate;
    }

    public void Start(NPC npc, int frameHeight) {
        npc.frame.Y = startFrame * frameHeight;
        npc.frameCounter = 0;
    }

    public bool Update(NPC npc, int frameHeight) {
        if (animationFinished(in npc)) {
            return true;
        }

        npc.frameCounter += GetTickRate(in npc);

        if (npc.frameCounter > frameDuration) {
            npc.frame.Y += frameHeight;
            npc.frameCounter = 0;
        }

        if (npc.frame.Y > endFrame * frameHeight) {
            npc.frame.Y = startFrame * frameHeight;
        }

        return false;
    }

    private float GetTickRate(in NPC npc) => _tickRateDelegate is { } @delegate ? @delegate(npc) : tickRate;
}