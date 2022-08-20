using System;
using LivingWorldMod.Custom.Utilities;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Core.Patches {
    /// <summary>
    /// Miscellaneous loadable class that applies patches to the Player class with no
    /// real similarities otherwise.
    /// </summary>
    public class MiscPlayerPatches : ILoadable {
        public void Load(Mod mod) {
            IL.Terraria.Player.KillMe += SkipPreKill;
        }

        public void Unload() { }

        private void SkipPreKill(ILContext il) {
            //Simple edit, adding some branching that allows us to skip over tML's Prekill call when we Force Kill.

            ILCursor c = new ILCursor(il);

            ILLabel exitLabel = c.DefineLabel();

            //Navigate to instruction we want our check to jump to in case of true return, which conveniently already exists
            c.ErrorOnFailedGotoNext(MoveType.After, i => i.MatchCall(typeof(PlayerLoader), nameof(PlayerLoader.PreKill)));
            if (c.Next.OpCode != OpCodes.Brtrue_S) {
                throw new PatchingUtils.InstructionNotFoundException();
            }
            exitLabel = (ILLabel)c.Next.Operand;

            //Jump back to beginning, and navigate to where we want to add our branch instruction, then add it
            c.Index = 0;
            c.ErrorOnFailedGotoNext(i => i.MatchCall<Player>(nameof(Player.StopVanityActions)));
            c.Index += 5;
            c.EmitDelegate<Func<bool>>(() => PlayerUtils.SkipPreKill);
            c.Emit(OpCodes.Brtrue_S, exitLabel);
        }
    }
}