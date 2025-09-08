using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs.Animations;

/// <summary>
///     Representation of a single frame "animation." The NPC will stay with this single <see cref="frame" /> until the animation terminates, which is determined via the passed in predicate
///     <see cref="AnimationFinishedPredicate" />.
/// </summary>
public readonly struct SingleFrameAnimation(int frame, SingleFrameAnimation.AnimationFinishedPredicate animationFinished, int priority = 0) : IAnimation {
    public delegate bool AnimationFinishedPredicate(in NPC npc);

    public int Priority {
        get;
    } = priority;

    public void Start(NPC npc, int frameHeight) {
        npc.frame.Y = frame * frameHeight;
        npc.frameCounter = 0;
    }

    public bool Update(NPC npc, int frameHeight) => animationFinished(in npc);
}