using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs.Animations;

/// <summary>
///     Simple animation object type that represents an equally-spaced, conditionally looping animation. To be more specific, the animation will start at <see cref="startFrame" />, incrementing
///     linearly through each frame after <see cref="frameDuration" /> ticks have passed, up to <see cref="endFrame" />, where the animation then loops from <see cref="startFrame" />.
///     This animation will keep looping until <see cref="animationFinished" /> returns true. Also supports a variable tick rate, via a delegate <see cref="GetVariableTickRateDelegate" /> or flat
///     tick rate via <see cref="tickRate" />.
/// </summary>
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

        if (npc.frame.Y >= endFrame * frameHeight) {
            npc.frame.Y = startFrame * frameHeight;
        }

        return false;
    }

    private float GetTickRate(in NPC npc) => _tickRateDelegate is { } @delegate ? @delegate(npc) : tickRate;
}