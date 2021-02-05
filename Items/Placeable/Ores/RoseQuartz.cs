using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Items.Placeable.Ores
{
    internal class RoseQuartz : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SortingPriorityMaterials[item.type] = 58;
        }

        public override void SetDefaults()
        {
            item.maxStack = 999;
            item.width = 18;
            item.height = 16;
            item.value = 3000;
        }
    }
}