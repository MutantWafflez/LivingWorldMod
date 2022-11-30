using LivingWorldMod.Common.Players;
using LivingWorldMod.Content.Subworlds.Pyramid;
using Microsoft.Xna.Framework;
using On.Terraria.Audio;
using ReLogic.Utilities;
using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SoundStyle = Terraria.Audio.SoundStyle;

namespace LivingWorldMod.Core.Patches {
    /// <summary>
    /// ILoadable class for patches relating to the Sound Engine.
    /// </summary>
    public class SoundEnginePatches : ILoadable {
        public void Load(Mod mod) {
            SoundEngine.PlaySound_refSoundStyle_Nullable1 += PreventSound;
        }

        public void Unload() { }

        private SlotId PreventSound(SoundEngine.orig_PlaySound_refSoundStyle_Nullable1 orig, ref SoundStyle style, Vector2? position) {
            //Gives functionality to the Curse of Silence, preventing sound for the player while in a room with said curse.

            if (Main.netMode != NetmodeID.Server && SubworldSystem.IsActive<PyramidSubworld>() && Main.LocalPlayer.GetModPlayer<PyramidDungeonPlayer>().CurrentCurses.Contains(PyramidRoomCurseType.Silence)) {
                return SlotId.Invalid;
            }

            return orig(ref style, position);
        }
    }
}