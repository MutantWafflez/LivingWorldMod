using System;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;

namespace LivingWorldMod.Utilities;

// Utilities class that holds methods that deals with patching, whether it be IL or detouring
public static partial class LWMUtils {
    /// <summary>
    ///     Exception that designates the given IL search parameters did not yield any found instruction.
    /// </summary>
    public class InstructionNotFoundException : Exception { }

    /// <summary>
    ///     Calls <seealso cref="ILCursor.TryGotoNext" /> normally, but will throw an exception if
    ///     the goto does not work or does not find the proper instruction.
    /// </summary>
    public static void ErrorOnFailedGotoNext(this ILCursor cursor, params Func<Instruction, bool>[] predicates) {
        if (cursor.TryGotoNext(predicates)) {
            return;
        }

        throw new ILPatchFailureException(LWM.Instance, cursor.Context, new InstructionNotFoundException());
    }

    /// <summary>
    ///     Calls <seealso cref="ILCursor.TryGotoNext" /> normally, but will throw an exception if
    ///     the goto does not work or does not find the proper instruction.
    /// </summary>
    public static void ErrorOnFailedGotoNext(this ILCursor cursor, MoveType moveType, params Func<Instruction, bool>[] predicates) {
        if (cursor.TryGotoNext(moveType, predicates)) {
            return;
        }

        throw new ILPatchFailureException(LWM.Instance, cursor.Context, new InstructionNotFoundException());
    }

    /// <summary>
    ///     Calls <seealso cref="ILCursor.TryGotoPrev" /> normally, but will throw an exception if
    ///     the goto does not work or does not find the proper instruction.
    /// </summary>
    public static void ErrorOnFailedGotoPrev(this ILCursor cursor, params Func<Instruction, bool>[] predicates) {
        if (cursor.TryGotoPrev(predicates)) {
            return;
        }

        throw new ILPatchFailureException(LWM.Instance, cursor.Context, new InstructionNotFoundException());
    }

    /// <summary>
    ///     Calls <seealso cref="ILCursor.TryGotoPrev" /> normally, but will throw an exception if
    ///     the goto does not work or does not find the proper instruction.
    /// </summary>
    public static void ErrorOnFailedGotoPrev(this ILCursor cursor, MoveType moveType, params Func<Instruction, bool>[] predicates) {
        if (cursor.TryGotoPrev(moveType, predicates)) {
            return;
        }

        throw new ILPatchFailureException(LWM.Instance, cursor.Context, new InstructionNotFoundException());
    }

    /// <summary>
    ///     Moves and points this cursor to the very last instruction in the given IL content.
    /// </summary>
    public static void GotoLastInstruction(this ILCursor cursor) {
        cursor.Index = cursor.Instrs.Count - 1;
    }

    /// <summary>
    ///     Clones the given method body to the cursor, wholesale copying all
    ///     relevant contents (instructions, parameters, variables, etc.).
    /// </summary>
    /// <param name="body">The body to copy from.</param>
    /// <param name="c">The cursor to copy to.</param>
    /// <remarks>
    ///     This method is provided courtesy of the lovely developers behind Nitrate:
    ///     https://github.com/terraria-catalyst/nitrate-mod
    ///     <para></para>
    ///     Slightly tweaked for LWM's formatting, but no underlying logic changed.
    /// </remarks>
    public static void CloneMethodBodyToCursor(MethodBody body, ILCursor c) {
        c.Index = 0;

        c.Body.MaxStackSize = body.MaxStackSize;
        c.Body.InitLocals = body.InitLocals;
        c.Body.LocalVarToken = body.LocalVarToken;

        foreach (Instruction instr in body.Instructions) {
            c.Emit(instr.OpCode, instr.Operand);
        }

        for (int i = 0; i < body.Instructions.Count; i++) {
            c.Instrs[i].Offset = body.Instructions[i].Offset;
        }

        foreach (Instruction instr in c.Body.Instructions) {
            instr.Operand = instr.Operand switch {
                Instruction target => c.Body.Instructions[body.Instructions.IndexOf(target)],
                Instruction[] targets => targets.Select(x => c.Body.Instructions[body.Instructions.IndexOf(x)]).ToArray(),
                _ => instr.Operand
            };
        }

        c.Body.ExceptionHandlers.AddRange(
            body.ExceptionHandlers.Select(
                x => new ExceptionHandler(x.HandlerType) {
                    TryStart = x.TryStart is null ? null : c.Body.Instructions[body.Instructions.IndexOf(x.TryStart)],
                    TryEnd = x.TryEnd is null ? null : c.Body.Instructions[body.Instructions.IndexOf(x.TryEnd)],
                    FilterStart = x.FilterStart is null ? null : c.Body.Instructions[body.Instructions.IndexOf(x.FilterStart)],
                    HandlerStart = x.HandlerStart is null ? null : c.Body.Instructions[body.Instructions.IndexOf(x.HandlerStart)],
                    HandlerEnd = x.HandlerEnd is null ? null : c.Body.Instructions[body.Instructions.IndexOf(x.HandlerEnd)],
                    CatchType = x.CatchType is null ? null : c.Body.Method.Module.ImportReference(x.CatchType)
                }
            )
        );

        c.Body.Variables.AddRange(body.Variables.Select(x => new VariableDefinition(x.VariableType)));

        c.Method.CustomDebugInformations.AddRange(
            body.Method.CustomDebugInformations.Select(
                x => {
                    switch (x) {
                        case AsyncMethodBodyDebugInformation asyncInfo: {
                            AsyncMethodBodyDebugInformation info = new();

                            if (asyncInfo.CatchHandler.Offset >= 0) {
                                info.CatchHandler = asyncInfo.CatchHandler.IsEndOfMethod ? new InstructionOffset() : new InstructionOffset(ResolveInstrOff(info.CatchHandler.Offset));
                            }

                            info.Yields.AddRange(asyncInfo.Yields.Select(y => y.IsEndOfMethod ? new InstructionOffset() : new InstructionOffset(ResolveInstrOff(y.Offset))));
                            info.Resumes.AddRange(asyncInfo.Resumes.Select(y => y.IsEndOfMethod ? new InstructionOffset() : new InstructionOffset(ResolveInstrOff(y.Offset))));

                            return info;
                        }

                        case StateMachineScopeDebugInformation stateInfo: {
                            StateMachineScopeDebugInformation info = new();
                            info.Scopes.AddRange(stateInfo.Scopes.Select(y => new StateMachineScope(ResolveInstrOff(y.Start.Offset), y.End.IsEndOfMethod ? null : ResolveInstrOff(y.End.Offset))));

                            return info;
                        }

                        default:
                            return x;
                    }
                }
            )
        );

        c.Method.DebugInformation.SequencePoints.AddRange(
            body.Method.DebugInformation.SequencePoints.Select(
                x => new SequencePoint(ResolveInstrOff(x.Offset), x.Document) { StartLine = x.StartLine, StartColumn = x.StartColumn, EndLine = x.EndLine, EndColumn = x.EndColumn }
            )
        );

        c.Index = 0;

        return;

        Instruction ResolveInstrOff(int off) {
            for (int i = 0; i < body.Instructions.Count; i++) {
                if (body.Instructions[i].Offset == off) {
                    return c.Body.Instructions[i];
                }
            }

            throw new Exception("Could not resolve instruction offset");
        }
    }
}