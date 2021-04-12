using LivingWorldMod.Custom.Enums;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace LivingWorldMod.Content.Tiles.Interactables
{
    public abstract class VillagerShrineTile : ModTile
    {
        public VillagerID shrineType;

        public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileTable[Type] = false;
            Main.tileLighted[Type] = true;
            Main.tileWaterDeath[Type] = false;
            Main.tileLavaDeath[Type] = false;

            TileObjectData.newTile.WaterDeath = false;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;

            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.Origin = new Point16(1, 4);
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16, 16 };
            TileObjectData.newTile.Height = 5;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.Width = 4;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, TileObjectData.newTile.Width, 0);
            TileObjectData.addTile(Type);

            animationFrameHeight = 90;
        }

        public override bool HasSmartInteract() => true;

        public override bool CanKillTile(int i, int j, ref bool blockDamaged) => LivingWorldMod.debugMode;

        public override bool CanExplode(int i, int j) => LivingWorldMod.debugMode;

        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            int giftProgress = LWMWorld.GetGiftProgress(shrineType);

            frame = frameCounter = (int)(giftProgress / 6.66f);
        }
    }
}