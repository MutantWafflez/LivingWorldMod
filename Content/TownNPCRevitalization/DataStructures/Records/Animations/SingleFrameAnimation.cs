using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records.Animations;

/// <summary>
///     Representation of a single frame "animation." The NPC will stay with this single <see cref="Frame" /> until the animation terminates, which is determined via the passed in predicate
///     <see cref="AnimationFinishedPredicate" />.
/// </summary>
public readonly record struct SingleFrameAnimation(int Frame, SingleFrameAnimation.AnimationFinishedPredicate AnimationFinished, int Priority = 0) : IAnimation {
    public delegate bool AnimationFinishedPredicate(in NPC npc);

    public int Priority {
        get;
    } = Priority;

    public void Start(NPC npc, int frameHeight) {
        npc.frame.Y = Frame * frameHeight;
        npc.frameCounter = 0;
    }

    public bool Update(NPC npc, int frameHeight) => AnimationFinished(in npc);
}