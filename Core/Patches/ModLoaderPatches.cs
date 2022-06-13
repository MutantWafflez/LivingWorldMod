using LivingWorldMod.Custom.Utilities;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Core.Patches {
    /// <summary>
    /// Class that has patches for tML methods. These are all internal, so special things must be done.
    /// </summary>
    public class ModLoaderPatches : ILoadable {
        private static MethodInfo originalSaveNPCMethod;
        private static MethodInfo originalLoadNPCMethod;

        private static event ILContext.Manipulator ModifySaveNPC {
            add => HookEndpointManager.Modify(originalSaveNPCMethod, value);
            remove => HookEndpointManager.Unmodify(originalSaveNPCMethod, value);
        }

        private static event ILContext.Manipulator ModifyLoadNPC {
            add => HookEndpointManager.Modify(originalLoadNPCMethod, value);
            remove => HookEndpointManager.Unmodify(originalLoadNPCMethod, value);
        }

        public void Load(Mod mod) {
            Type worldIOType = typeof(ModLoader).Assembly.GetType("Terraria.ModLoader.IO.WorldIO");
            originalSaveNPCMethod = worldIOType.GetMethod("SaveNPCs", BindingFlags.Static | BindingFlags.NonPublic);
            originalLoadNPCMethod = worldIOType.GetMethod("LoadNPCs", BindingFlags.Static | BindingFlags.NonPublic);
            ModifySaveNPC += FixedNPCIO;
            ModifyLoadNPC += FixedNPCIO;
        }

        public void Unload() {
            ModifySaveNPC -= FixedNPCIO;
            ModifyLoadNPC -= FixedNPCIO;
        }

        //Both methods can actually be edited the same way; no reason to use different ones
        private void FixedNPCIO(ILContext il) {
            //Simple edit. Find the first instance of checking if "npc.townNPC", and replacing it with "npc.isLikeATownNPC".
            ILCursor c = new ILCursor(il);

            c.ErrorOnFailedGotoNext(i => i.MatchLdfld<NPC>(nameof(NPC.townNPC)));
            c.Remove();
            c.Emit(OpCodes.Callvirt, typeof(NPC).GetProperty(nameof(NPC.isLikeATownNPC), BindingFlags.Instance | BindingFlags.Public).GetMethod);
        }
    }
}