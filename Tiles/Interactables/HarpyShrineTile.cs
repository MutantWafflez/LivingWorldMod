using LivingWorldMod.NPCs.Villagers;
using LivingWorldMod.UI;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace LivingWorldMod.Tiles.Interactables
{
    public class HarpyShrineTile : VillagerShrineTile
    {
        public HarpyShrineTile()
        {
            shrineType = VillagerType.Harpy;
        }

        public override void PostSetDefaults()
        {
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Harpy Shrine");
            AddMapEntry(new Color(210, 210, 210), name);
        }

        public override bool NewRightClick(int i, int j)
        {
            //ItemSlot UI Placement
            ShrineUIPanel.TileRightClicked(i, j, shrineType);
            return true;
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;

            player.showItemIcon2 = ItemType<Items.Debug.HarpyShrine>();
            player.showItemIcon = true;
            player.noThrow = 2;
        }
    }
}