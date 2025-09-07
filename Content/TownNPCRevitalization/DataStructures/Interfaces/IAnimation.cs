namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;

/// <summary>
///     Describes an object which has a priority, and can be updated. Used for Town NPC animations.
/// </summary>
public interface IAnimation {
    /// <summary>
    ///     The priority of this animation. The lower this number is, the more priority this animation will have to overwrite other running animations.
    ///     If the priority of two animations is equal, then the new one will be ignored.
    /// </summary>
    public int Priority {
        get;
    }

    /// <summary>
    ///     Called every tick that this animation object is considered to be running. Returns true when the animation is considered complete/finished.
    /// </summary>
    public bool Update(NPC npc, int frameHeight);
}