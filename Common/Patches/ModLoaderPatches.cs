using System;
using System.Reflection;
using LivingWorldMod.Custom.Utilities;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Patches {
    /// <summary>
    /// Class that has patches for tML methods. These are all internal, so special things must be done.
    /// </summary>
    public class ModLoaderPatches : ILoadable {
        private MethodInfo _originalSaveNPCMethod;
        private MethodInfo _originalLoadNPCMethod;


        public void Load(Mod mod) {
            Type worldIOType = typeof(ModLoader).Assembly.GetType("Terraria.ModLoader.IO.WorldIO");
            _originalSaveNPCMethod = worldIOType.GetMethod("SaveNPCs", BindingFlags.Static | BindingFlags.NonPublic);
            _originalLoadNPCMethod = worldIOType.GetMethod("LoadNPCs", BindingFlags.Static | BindingFlags.NonPublic);
            HookEndpointManager.Modify(_originalSaveNPCMethod, FixedNPCIO);
            HookEndpointManager.Modify(_originalLoadNPCMethod, FixedNPCIO);
        }

        public void Unload() {
            HookEndpointManager.Unmodify(_originalSaveNPCMethod, FixedNPCIO);
            HookEndpointManager.Unmodify(_originalLoadNPCMethod, FixedNPCIO);
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