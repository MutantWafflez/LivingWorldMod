using LivingWorldMod.Common.Systems;
using LivingWorldMod.Content.Items.Placeables.Torches;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace LivingWorldMod.Content.Tiles.Torches {
    public class StarTorchTile : BaseTile {
        public override Color? TileColorOnMap => Color.Yellow;

        public override void SetStaticDefaults() {
            Main.tileSolid[Type] = false;
            Main.tileNoSunLight[Type] = false;
            Main.tileLighted[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileWaterDeath[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileFrameImportant[Type] = true;
            TileID.Sets.FramesOnKillWall[Type] = true;
            TileID.Sets.Torch[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.StyleTorch);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
            TileObjectData.newAlternate.AnchorLeft = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
            TileObjectData.newAlternate.AnchorAlternateTiles = new[] { 124 };
            TileObjectData.addAlternate(1);
            TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
            TileObjectData.newAlternate.AnchorRight = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
            TileObjectData.newAlternate.AnchorAlternateTiles = new[] { 124 };
            TileObjectData.addAlternate(2);
            TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
            TileObjectData.newAlternate.AnchorWall = true;
            TileObjectData.addAlternate(0);
            TileObjectData.addTile(Type);

            ItemDrop = ModContent.ItemType<StarTorchItem>();

            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
            AdjTiles = new int[] { TileID.Torches };

            DustType = DustID.YellowStarDust;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
            if (Framing.GetTileSafely(i, j).TileFrameX < 66) {
                r = BlockLightSystem.Instance.starTorchColor.R / 255f;
                g = BlockLightSystem.Instance.starTorchColor.G / 255f;
                b = BlockLightSystem.Instance.starTorchColor.B / 255f;
            }
        }

        public override void NumDust(int i, int j, bool fail, ref int num) {
            num = Main.rand.Next(1, 3);
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
            if (Main.rand.NextBool(15)) {
                for (int x = 0; x < Main.rand.Next(3); x++) {
                    Dust.NewDust(new Vector2(i * 16, j * 16), 16, 16, DustType);
                }
            }
        }

        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
            offsetY = 0;
            if (WorldGen.SolidTile(i, j - 1)) {
                offsetY = -2;
                if (WorldGen.SolidTile(i - 1, j + 1) || WorldGen.SolidTile(i + 1, j + 1)) {
                    offsetY = -4;
                }
            }
        }
    }
}