using LivingWorldMod.NPCs.Villagers;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace LivingWorldMod.Tiles.Interactables
{
    public abstract class VillagerShrineTile : ModTile
    {
        public VillagerType shrineType;
        public int shrineStage;

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
            int reputation = LWMWorld.GetReputation(shrineType);
            shrineStage = (int)(reputation / 6.66f); //Increase stage for each 7 rep (100 rep / 15 frames = 6.666..)

            //Negative values won't be visually displayed, only in the UI
            if (shrineStage < 0) shrineStage = 0;
            frame = frameCounter = shrineStage; 
        }
    }
}
