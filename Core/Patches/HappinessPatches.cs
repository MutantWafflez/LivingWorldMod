using System.Linq;
using LivingWorldMod.Common.GlobalNPCs;
using LivingWorldMod.Custom.Classes;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace LivingWorldMod.Core.Patches {
    /// <summary>
    /// Patches that deal with Town NPC happiness.
    /// </summary>
    public sealed class HappinessPatches : LoadablePatch {
        private const float MinCostModifier = 0.67f;
        private const float MaxCostModifier = 1.5f;

        public override void LoadPatches() {
            On_ShopHelper.ProcessMood += MoodOverhaulChanges;
            On_ShopHelper.AddHappinessReportText += GetMoodModifier;
        }

        private void MoodOverhaulChanges(On_ShopHelper.orig_ProcessMood orig, ShopHelper self, Player player, NPC npc) {
            orig(self, player, npc);
            if (!npc.TryGetGlobalNPC(out TownAIGlobalNPC globalNPC)) {
                return;
            }

            TownNPCMoodModule moodModule = globalNPC.MoodModule;
            if (npc.life < npc.lifeMax) {
                moodModule.AddModifier("Injured", LocalizationUtils.GetLWMTextValue("TownNPCMoodFlavorText.Injured"), 0);
            }

            if (BirthdayParty.PartyIsUp && BirthdayParty.GenuineParty && BirthdayParty.CelebratingNPCs.Contains(npc.whoAmI)) {
                moodModule.AddModifier("Party", LocalizationUtils.GetLWMTextValue("TownNPCMoodFlavorText.Party"), 0);
            }

            float currentMood = moodModule.CurrentMood;
            self._currentPriceAdjustment = MathHelper.Lerp(MinCostModifier, MaxCostModifier, 1f - currentMood / TownNPCMoodModule.MaxMoodValue);
            // TODO: Localize properly (or replace with full UI for Part 2)
            self._currentHappiness =
                $"Current Mood: {(int)currentMood}/{(int)TownNPCMoodModule.MaxMoodValue}\n"
                + string.Join('\n', moodModule.GetFlavorTextAndModifiers().Select(flavorTextAndModifer => {
                        (string flavorText, float moodModifier) = flavorTextAndModifer;
                        return $"\"{flavorText}\" ({(moodModifier >= 0 ? "+" : "")}{moodModifier})";
                    })
                );
        }

        private void GetMoodModifier(On_ShopHelper.orig_AddHappinessReportText orig, ShopHelper self, string textKeyInCategory, object substitutes, int otherNPCType) {
            //Adapted vanilla code
            string text = "TownNPCMood_" + NPCID.Search.GetName(self._currentNPCBeingTalkedTo.netID);

            if (textKeyInCategory == "Princess_LovesNPC") {
                text = ModContent.GetModNPC(otherNPCType).GetLocalizationKey("TownNPCMood");
            }
            else if (self._currentNPCBeingTalkedTo.ModNPC is ModNPC modNPC) {
                text = modNPC.GetLocalizationKey("TownNPCMood");
            }

            if (self._currentNPCBeingTalkedTo.type == NPCID.BestiaryGirl && self._currentNPCBeingTalkedTo.altTexture == 2) {
                text += "Transformed";
            }

            string flavorText = Language.GetTextValueWith(text + "." + textKeyInCategory, substitutes);
            // To prevent the "content" modifier from showing up when other modifiers are present
            self._currentHappiness = " ";
            if (self._currentNPCBeingTalkedTo.TryGetGlobalNPC(out TownAIGlobalNPC globalNPC)) {
                globalNPC.MoodModule.AddModifier(textKeyInCategory.Split('_')[0], flavorText, 0);
            }
        }
    }
}