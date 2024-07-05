using LivingWorldMod.Globals.BaseTypes.Items;
using LivingWorldMod.Globals.BaseTypes.Tiles;
using LivingWorldMod.Globals.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ObjectData;

namespace LivingWorldMod.Content.Villages.HarpyVillage.Tiles.Torches;

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
        TileObjectData.newAlternate.AnchorAlternateTiles = [124];
        TileObjectData.addAlternate(1);
        TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
        TileObjectData.newAlternate.AnchorRight = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
        TileObjectData.newAlternate.AnchorAlternateTiles = [124];
        TileObjectData.addAlternate(2);
        TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
        TileObjectData.newAlternate.AnchorWall = true;
        TileObjectData.addAlternate(0);
        TileObjectData.addTile(Type);

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
        AdjTiles = [TileID.Torches];

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

public class StarTorchItem : BaseItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 30;

        ItemID.Sets.Torches[Type] = true;
        ItemID.Sets.WaterTorches[Type] = true;
        ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.ShimmerTorch;
    }

    public override void SetDefaults() {
        Item.CloneDefaults(ItemID.IchorTorch);
        Item.rare = ItemRarityID.Blue;
        Item.placeStyle = 0;
        Item.value = Item.buyPrice(silver: 5);
        Item.createTile = ModContent.TileType<StarTorchTile>();
        Item.flame = false;
    }

    public override void HoldItem(Player player) {
        if (Main.rand.NextBool(player.itemAnimation > 0 ? 40 : 80)) {
            Dust.NewDust(new Vector2(player.itemLocation.X + 16f * player.direction, player.itemLocation.Y - 14f * player.gravDir), 4, 4, DustID.YellowStarDust);
        }

        Vector2 position = player.RotatedRelativePoint(new Vector2(player.itemLocation.X + 12f * player.direction + player.velocity.X, player.itemLocation.Y - 14f + player.velocity.Y), true);
        Lighting.AddLight(position, BlockLightSystem.Instance.starTorchColor.ToVector3());
    }

    public override void PostUpdate() {
        Lighting.AddLight(new Vector2((Item.position.X + Item.width / 2f) / 16f, (Item.position.Y + Item.height / 2f) / 16f), BlockLightSystem.Instance.starTorchColor.ToVector3());
    }

    public override void AddRecipes() {
        CreateRecipe(3)
            .AddIngredient(ItemID.FallenStar)
            .AddIngredient(ItemID.Torch, 3)
            .Register();
    }
}