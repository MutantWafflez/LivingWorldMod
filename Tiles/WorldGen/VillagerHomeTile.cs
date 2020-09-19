using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Tiles.WorldGen
{
    public class VillagerHomeTile : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileMergeDirt[Type] = false;
            Main.tileNoAttach[Type] = true;
            Main.tileBlockLight[Type] = false;
            Main.tileNoSunLight[Type] = false;
            minPick = 500;
            mineResist = 10f;
        }

        public override bool KillSound(int i, int j) => false;
        public override bool CreateDust(int i, int j, ref int type) => false;
        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) => false;
        public override bool CanPlace(int i, int j) => false;
        public override bool CanExplode(int i, int j) => false;
        public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;
    }
}
