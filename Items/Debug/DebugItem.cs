using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Items.Debug
{
    public abstract class DebugItem : ModItem
    {
        public override bool Autoload(ref string name)
        {
            return LivingWorldMod.debugMode;
        }

        public override void AutoDefaults()
        {
            item.rare = ItemRarityID.Quest;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(mod, "DebugFlavor", "[Debug]"));
        }
    }
}
