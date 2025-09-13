namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;

/// <summary>
///     Describes an object which has a priority, a start method, and can be updated. Used for Town NPC animations.
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
    ///     Default boilerplate of <see cref="Start" /> that sets the NPC's frame to some abitrary start frame value.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="frameHeight"></param>
    /// <param name="startFrame"></param>
    public static void StartAnimationWithFrame(NPC npc, int frameHeight, int startFrame) {
        npc.frame.Y = startFrame * frameHeight;
        npc.frameCounter = 0;
    }

    /// <summary>
    ///     Called once when the animation is first requested. Typically used to set the current frame to the starting frame of the animation.
    /// </summary>
    public void Start(NPC npc, int frameHeight);

    /// <summary>
    ///     Called every tick that this animation object is considered to be running after <see cref="Start" /> is called. Returns true when the animation is considered complete/finished.
    /// </summary>
    public bool Update(NPC npc, int frameHeight);
}