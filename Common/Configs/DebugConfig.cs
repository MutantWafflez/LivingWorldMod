using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using LivingWorldMod.Content.Subworlds.Pyramid;
using Terraria.ModLoader.Config;

namespace LivingWorldMod.Common.Configs {
    /// <summary>
    /// Config that handles debug related matters in the mod.
    /// </summary>
    public class DebugConfig : ModConfig {
        [Label("Force Debug Mode")]
        [Tooltip("Forces the mod to enter Debug mode, regardless of if the mod is being built from Visual Studio.\nONLY enable this if you know what you're doing.")]
        [DefaultValue(false)]
        [ReloadRequired]
        public bool forceDebugMode;

        [Header("Revamped Pyramid Debug Options")]
        [Label("Pyramid Generation Debug")]
        [Tooltip("Requires Debug Mode to be enabled to function; upon entering any world, instantly enter the Revamped Pyramid Subworld.")]
        [DefaultValue(false)]
        public bool pyramidDebug;

        [Label("Super Curse Mode")]
        [Tooltip("Requires Debug Mode to be enabled to function; All rooms in the pyramid dungeon become cursed.")]
        [DefaultValue(false)]
        public bool allPyramidRoomsAreCursed;

        [Label("Forced Room Curse Type")]
        [Tooltip("Requires Debug Mode to be enabled to function; All cursed rooms in the pyramid become cursed with the added curses.")]
        public List<PyramidRoomCurseType> forcedCurseTypes = new List<PyramidRoomCurseType>();

        public override ConfigScope Mode => ConfigScope.ServerSide;
    }
}