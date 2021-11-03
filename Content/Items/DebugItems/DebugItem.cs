using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.DebugItems {

    /// <summary>
    /// Item that is only loaded when in Debug mode.
    /// </summary>
    public abstract class DebugItem : ModItem {

        public override bool IsLoadingEnabled(Mod mod) => LivingWorldMod.IsDebug;

        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            //Name will be a very weird color to signify debug-ness (in case anyone couldn't figure it out)
            TooltipLine nameLine = tooltips.FirstOrDefault(tooltip => tooltip.Name == "ItemName" && tooltip.mod == "Terraria");

            if (nameLine != null) {
                int flashColor = (int)(255 * Main.masterColor);

                nameLine.text += " [DEBUG]";
                nameLine.overrideColor = new Color(flashColor, flashColor, flashColor);
            }
        }
    }
}