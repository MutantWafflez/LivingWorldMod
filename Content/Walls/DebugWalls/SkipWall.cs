﻿#if DEBUG

using LivingWorldMod.Content.Items.DebugItems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Walls.DebugWalls {

    /// <summary>
    /// Wall used for "skipping" over certain wall positions when used in conjunction with the
    /// structure stick.
    /// </summary>
    public class SkipWall : BaseWall {
        public override string Texture => "Terraria/Images/Wall_" + WallID.StarsWallpaper;

        public override void SetStaticDefaults() {
            Main.wallHouse[Type] = false;
            Main.wallLight[Type] = true;

            WallID.Sets.AllowsWind[Type] = false;
        }
    }
}

#endif