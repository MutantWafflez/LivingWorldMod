using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LivingWorldMod.Content.AccessorySlots;
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
        public bool IsPotent => ModContent.GetInstance<AtumAccessorySlot>().FunctionalItem.type == Type;

        public static readonly Regex InlineWordSearch = new Regex(@"(?<PotentSwap>\|P:(?<Normal>.*)/(?<Potent>.*)\|)", RegexOptions.Compiled | RegexOptions.Multiline);
        public static readonly Regex AdditionalLineSearch = new Regex(@"(?<Start>\|NPE:)(?<Text>[\s\S]*)(?<End>\|)", RegexOptions.Compiled | RegexOptions.Multiline);
        public static readonly Color PotentTextColor = new Color(20, 126, 168);

        /// <summary>
        /// Equivalent and mutually exclusive to <seealso cref="AccessoryItem.AccessoryUpdate"/>; only one is called
        /// depending on if this boon is placed within the Atum Accessory Slot.
        /// </summary>
        public virtual void PotentBoonUpdate(Player player) { }

        public override bool PreReforge() => false; //TODO: Change when Boon of Ptah is added

        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            //Manually handle our tooltips, just to make things simpler
            TooltipLine equipLine = tooltips.FirstOrDefault(tip => tip.Mod == "Terraria" && tip.Name == "Equipable");

            if (equipLine is null) {
                return;
            }
            int equipLineIndex = tooltips.IndexOf(equipLine);

            string descriptionText = TooltipRegexCheck(LocalizationUtils.GetLWMTextValue($"BoonDescription.{Name}"));
            tooltips.Insert(equipLineIndex + 1, new TooltipLine(Mod, "BoonDescription", descriptionText));

            string curseText = TooltipRegexCheck(LocalizationUtils.GetLWMTextValue($"BoonCurse.{Name}"));
            tooltips.Insert(equipLineIndex + 2, new TooltipLine(Mod, "BoonCurse", curseText) {
                OverrideColor = new Color(252, 60, 60)
            });
        }

        /// <summary>
        /// Applies the <seealso cref="InlineWordSearch"/> and <seealso cref="AdditionalLineSearch"/> regex to the passed in
        /// tooltip lines, and replaces/adds accordingly, and returns the finalized string.
        /// </summary>
        /// <param name="originalText"> The line(s) to search. </param>
        private string TooltipRegexCheck(string originalText) {
            //Checks for |P:left/right| swaps
            foreach (Match match in InlineWordSearch.Matches(originalText)) {
                string fullTag = match.Groups["PotentSwap"].Value;
                originalText = originalText.Replace(fullTag, IsPotent ? $"[c/{PotentTextColor.R:X2}{PotentTextColor.G:X2}{PotentTextColor.B:X2}:{match.Groups["Potent"].Value}]" : match.Groups["Normal"].Value);
            }

            //Checks for |NPE: ... | line(s) to add
            foreach (Match match in AdditionalLineSearch.Matches(originalText)) {
                string fullMatch = match.Value;
                string innerText = match.Groups["Text"].Value;
                //If not Potent, or if the text doesn't have line breaks, do a simple replace
                if (!innerText.Contains('\n') || !IsPotent) {
                    originalText = originalText.Replace(fullMatch, IsPotent ? $"[c/{PotentTextColor.R:X2}{PotentTextColor.G:X2}{PotentTextColor.B:X2}:{innerText}]" : "");

                    //Remove lingering new line if it exists
                    originalText = originalText.EndsWith("\n") ? originalText.TrimEnd('\n') : originalText;
                    continue;
                }

                //If potent AND there are multi-lines, since chat tags can't span multiple lines, we must apply the color tag for each line
                originalText = innerText.Split('\n').Aggregate(originalText, (current, line) => current.Replace(line, $"[c/{PotentTextColor.R:X2}{PotentTextColor.G:X2}{PotentTextColor.B:X2}:{line}]"));
                originalText = originalText.Replace(match.Groups["Start"].Value, "");
                originalText = originalText.Replace(match.Groups["End"].Value, "");
            }

            return originalText;
        }
    }
}