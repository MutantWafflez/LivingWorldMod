using LivingWorldMod.Globals.Systems.DebugSystems;
using Microsoft.Xna.Framework.Input;

namespace LivingWorldMod.Custom.Classes.DebugModules;

/// <summary>
/// Class that can be extended for debug functionality with the <seealso cref="DebugToolSystem"/> ModSystem.
/// </summary>
public abstract class DebugModule {
    /// <summary>
    /// Called every tick. Put general update tasks in here, if necessary.
    /// </summary>
    public virtual void ModuleUpdate() { }

    /// <summary>
    /// Called when any amount of keys have been pressed. Called only once on first press; has no
    /// functionality for holding.
    /// </summary>
    /// <remarks>
    /// <b>DO NOT</b> have any functionality tied to <seealso cref="Keys.NumPad0"/>! This is reserved for
    /// the parent system for swapping modules, and will cause shenanigans otherwise.
    /// </remarks>
    public abstract void KeysPressed(Keys[] pressedKeys);
}