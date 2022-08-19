using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.Accessories.Boons {
    /// <summary>
    /// Further extension of the AccessoryItem class that focuses functionality towards LWM's
    /// concept of "Boons."
    /// </summary>
    public abstract class BoonAccessoryItem : AccessoryItem {
        /// <summary>
        /// Whether or not this boon is considered "potent" from being within the Atum Slot.
        /// </summary>
        public bool isPotent;

        /// <summary>
        /// Equivalent and mutually exclusive to <seealso cref="AccessoryItem.AccessoryUpdate"/>; only one is called
        /// depending on if this boon is placed within the Atum Accessory Slot.
        /// </summary>
        public virtual void PotentBoonUpdate(Player player, bool hideVisual) { }

        public override void ResetAccessoryEffects(Player player) => isPotent = false;

        public override bool PreReforge() => GetAccPlayer(Main.LocalPlayer).activeMiscEffects.Any(effect => effect == "CanReforgeBoon");

        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            //Manually handle our tooltips, just to make things simpler
            TooltipLine equipLine = tooltips.FirstOrDefault(tip => tip.Mod == "Terraria" && tip.Name == "Equipable");

            if (equipLine is null) {
                return;
            }
            int equipLineIndex = tooltips.IndexOf(equipLine);

            tooltips.Insert(equipLineIndex + 1, new TooltipLine(Mod, "BoonFlavorText", LocalizationUtils.GetLWMTextValue($"BoonFlavorText.{Name}")));

            string descriptionText = LocalizationUtils.GetLWMTextValue((isPotent ? "Potent" : "") + $"BoonDescription.{Name}");
            tooltips.Insert(equipLineIndex + 2, new TooltipLine(Mod, "BoonDescription", descriptionText));

            string curseText = LocalizationUtils.GetLWMTextValue((isPotent ? "Potent" : "") + $"BoonCurse.{Name}");
            tooltips.Insert(equipLineIndex + 3, new TooltipLine(Mod, "BoonCurse", curseText) {
                OverrideColor = new Color(252, 60, 60)
            });
        }
    }
}