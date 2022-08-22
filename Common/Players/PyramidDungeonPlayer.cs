﻿using LivingWorldMod.Custom.Classes;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Players {
    /// <summary>
    /// ModPlayer that handles player related things with the Revamped Pyramid dungeon.
    /// </summary>
    public class PyramidDungeonPlayer : ModPlayer {
        /// <summary>
        /// The pyramid room this player is currently in.
        /// </summary>
        public PyramidRoom currentRoom;
    }
}