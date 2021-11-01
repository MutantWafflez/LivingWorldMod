using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace LivingWorldMod.Custom.Utilities {

    /// <summary>
    /// Utilities class that holds methods that deals with patching, whether it be IL or detouring
    /// </summary>
    public static class PatchingUtils {

        /// <summary>
        /// Calls <seealso cref="ILCursor.TryGotoNext"/> normally, but will throw an exception if
        /// the goto does not work or does not find the proper instruction.
        /// </summary>
        public static void ErrorOnFailedGotoNext(this ILCursor cursor, params Func<Instruction, bool>[] predicates) {
            if (cursor.TryGotoNext(predicates)) {
                return;
            }

            throw new InstructionNotFoundException();
        }

        /// <summary>
        /// Calls <seealso cref="ILCursor.TryGotoNext"/> normally, but will throw an exception if
        /// the goto does not work or does not find the proper instruction.
        /// </summary>
        public static void ErrorOnFailedGotoNext(this ILCursor cursor, MoveType moveType, params Func<Instruction, bool>[] predicates) {
            if (cursor.TryGotoNext(moveType, predicates)) {
                return;
            }

            throw new InstructionNotFoundException();
        }

        public class InstructionNotFoundException : Exception { }
    }
}