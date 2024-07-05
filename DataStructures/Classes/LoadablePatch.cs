using System;
using MonoMod.Cil;

namespace LivingWorldMod.DataStructures.Classes;

/// <summary>
///     Extendable class that should be extended for any IL/On patches that provides extra debug information and
///     QoL for editing.
/// </summary>
public abstract class LoadablePatch : ILoadable {
    /// <summary>
    ///     The current ILContext that's being modified. Used for error handling.
    /// </summary>
    /// <remarks>
    ///     For <b>EVERY</b> IL patch, set this value to the ILContext for that edit.
    /// </remarks>
    public static ILContext currentContext;

    /// <summary>
    ///     Load all patches in this method.
    /// </summary>
    public abstract void LoadPatches();

    public void Load(Mod mod) {
        #pragma warning disable CS0162 // Unreachable code detected
        if (!LWM.EnableILPatches) {
            return;
        }
        #pragma warning restore CS0162 // Unreachable code detected

        try {
            LoadPatches();
            currentContext = null;
        }
        catch (ILPatchFailureException) {
            throw;
        }
        catch (Exception ex) {
            throw new ILPatchFailureException(ModContent.GetInstance<LWM>(), currentContext, ex);
        }
    }

    public void Unload() { }
}