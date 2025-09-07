namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;

/// <summary>
///     Describes an object which has a priority, a finished status, and can be updated. Used for Town NPC animations.
/// </summary>
public interface IAnimation {
    /// <summary>
    ///     Whether the animation is considered finished, and can be removed as the current animation.
    /// </summary>
    public bool IsFinished {
        get;
    }

    /// <summary>
    ///     The priority of this animation. The lower this number is, the more priority this animation will have to overwrite other running animations.
    /// </summary>
    public int Priority {
        get;
    }

    /// <summary>
    ///     Called every tick that this animation object is considered to be running.
    /// </summary>
    public void Update(NPC npc, int frameHeight);
}