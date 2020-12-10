using LivingWorldMod.Items.Materials;
using LivingWorldMod.Items.Placeable.Herbs;
using LivingWorldMod.Tiles.Furniture.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace LivingWorldMod.Tiles.WorldGen {

	enum PlantStage : byte {
		Planted,
		Growing,
		Grown
	}

	public class SkyBudHerb : ModTile {
		private const short FrameWidth = 14;

		public override void SetDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoFail[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileSpelunker[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);
			TileObjectData.newTile.AnchorValidTiles = new int[] {
					TileID.Cloud,
					TileID.RainCloud,
					TileID.SnowCloud
			};
			TileObjectData.newTile.AnchorAlternateTiles = new int[] {
					ModContent.TileType<SkyBudPlanterBox>()
			};
			TileObjectData.addTile(Type);
		}

		public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
			if (i % 2 == 1) {
				spriteEffects = SpriteEffects.FlipHorizontally;
			}
		}

		public override bool Drop(int i, int j) {
			PlantStage stage = GetStage(i, j);

			if (stage == PlantStage.Grown) {
				Item.NewItem(new Vector2(i, j).ToWorldCoordinates(), ModContent.ItemType<SkyBudItem>());
				Item.NewItem(new Vector2(i, j).ToWorldCoordinates(), ModContent.ItemType<SkyBudSeeds>(), Main.rand.Next(2, 4));
			}
			else if (stage == PlantStage.Growing) {
				Item.NewItem(new Vector2(i, j).ToWorldCoordinates(), ModContent.ItemType<SkyBudItem>());
			}

			return false;
		}

        public override bool KillSound(int i, int j) {
			Main.PlaySound(SoundID.Grass, i * 16, j * 16);
			return false;
        }

        public override bool CreateDust(int i, int j, ref int type) {
			type = DustID.Grass;
            return base.CreateDust(i, j, ref type);
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			PlantStage stage = GetStage(i, j);

			if (stage == PlantStage.Grown) {
				r = g = b = 0.25f;
            }
			else {
				r = g = b = 0f;
            }
        }

        public override void RandomUpdate(int i, int j) {
			Tile tile = Framing.GetTileSafely(i, j);
			PlantStage stage = GetStage(i, j);

			if (stage != PlantStage.Grown) {
				tile.frameX += FrameWidth;

				if (Main.netMode != NetmodeID.SinglePlayer) {
					NetMessage.SendTileSquare(-1, i, j, 1);
				}
			}
		}

		private PlantStage GetStage(int i, int j) {
			Tile tile = Framing.GetTileSafely(i, j);
			return (PlantStage)(tile.frameX / FrameWidth);
		}
	}
}