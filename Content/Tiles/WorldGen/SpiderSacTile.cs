using LivingWorldMod.Content.Projectiles.Hostile;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace LivingWorldMod.Content.Tiles.WorldGen {

    public class SpiderSacTile : ModTile {

        public override void SetDefaults() {
            Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileWaterDeath[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileCut[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top);
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.SolidBottom, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.WaterDeath = true;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.addTile(Type);

            disableSmartCursor = true;

            ModTranslation mapName = CreateMapEntryName();
            mapName.SetDefault("Spider Sac");
            AddMapEntry(new Color(220, 205, 205), mapName);
        }

        public override bool Dangersense(int i, int j, Player player) => true;

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
            if (Main.rand.NextFloat() < 0.075f) {
                Dust dust;
                Vector2 position = new Vector2(i * 16, j * 16);
                dust = Main.dust[Dust.NewDust(position, 16, 16, 102, 0f, 0f, 0, new Color(255, 255, 255), 1f)];
                dust.noGravity = true;
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) {
            Projectile.NewProjectile(new Vector2(i * 16 + 16, j * 16 + frameY + 16), Vector2.Zero, ModContent.ProjectileType<SpiderSacProj>(), 0, 0);
        }
    }
}