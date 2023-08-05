using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ObjectData;

namespace LivingWorldMod.Content.Tiles.Furniture.Critter {
    public class NimbusJarTile : BaseTile {
        public override Color? TileColorOnMap => Color.FloralWhite;

        public override void SetStaticDefaults() {
            Main.tileSolid[Type] = false;
            Main.tileNoSunLight[Type] = false;
            Main.tileNoAttach[Type] = true;
            Main.tileWaterDeath[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileFrameImportant[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, TileObjectData.newTile.Width, 0);
            TileObjectData.addTile(Type);

            AnimationFrameHeight = 36;
        }

        public override void AnimateTile(ref int frame, ref int frameCounter) {
            if (frame > 25) {
                frame = 0;
                frameCounter = 0;
            }

            if (++frameCounter > 6) {
                frameCounter = 0;
                frame++;
            }
        }
    }
}