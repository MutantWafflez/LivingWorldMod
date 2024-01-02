using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.ModLoader;

namespace LivingWorldMod.Custom.Utilities;

/// <summary>
/// Utilities class that holds methods that deals with patching, whether it be IL or detouring
/// </summary>
public static class PatchingUtils {
    /// <summary>
    /// Exception that designates the given IL search parameters did not yield any found instruction.
    /// </summary>
    public class InstructionNotFoundException : Exception { }

    /// <summary>
    /// Calls <seealso cref="ILCursor.TryGotoNext"/> normally, but will throw an exception if
    /// the goto does not work or does not find the proper instruction.
    /// </summary>
    public static void ErrorOnFailedGotoNext(this ILCursor cursor, params Func<Instruction, bool>[] predicates) {
        if (cursor.TryGotoNext(predicates)) {
            return;
        }

        throw new ILPatchFailureException(ModContent.GetInstance<LivingWorldMod>(), cursor.Context, new InstructionNotFoundException());
    }

    /// <summary>
    /// Calls <seealso cref="ILCursor.TryGotoNext"/> normally, but will throw an exception if
    /// the goto does not work or does not find the proper instruction.
    /// </summary>
    public static void ErrorOnFailedGotoNext(this ILCursor cursor, MoveType moveType, params Func<Instruction, bool>[] predicates) {
        if (cursor.TryGotoNext(moveType, predicates)) {
            return;
        }

        throw new ILPatchFailureException(ModContent.GetInstance<LivingWorldMod>(), cursor.Context, new InstructionNotFoundException());
    }

    /// <summary>
    /// Calls <seealso cref="ILCursor.TryGotoPrev"/> normally, but will throw an exception if
    /// the goto does not work or does not find the proper instruction.
    /// </summary>
    public static void ErrorOnFailedGotoPrev(this ILCursor cursor, params Func<Instruction, bool>[] predicates) {
        if (cursor.TryGotoPrev(predicates)) {
            return;
        }

        throw new ILPatchFailureException(ModContent.GetInstance<LivingWorldMod>(), cursor.Context, new InstructionNotFoundException());
    }

    /// <summary>
    /// Calls <seealso cref="ILCursor.TryGotoPrev"/> normally, but will throw an exception if
    /// the goto does not work or does not find the proper instruction.
    /// </summary>
    public static void ErrorOnFailedGotoPrev(this ILCursor cursor, MoveType moveType, params Func<Instruction, bool>[] predicates) {
        if (cursor.TryGotoPrev(moveType, predicates)) {
            return;
        }

        throw new ILPatchFailureException(ModContent.GetInstance<LivingWorldMod>(), cursor.Context, new InstructionNotFoundException());
    }

    /// <summary>
    /// Moves and points this cursor to the very last instruction in the given IL content.
    /// </summary>
    public static void GotoLastInstruction(this ILCursor cursor) {
        cursor.Index = cursor.Instrs.Count - 1;
    }
}