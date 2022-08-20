using LivingWorldMod.Common.Players;
using LivingWorldMod.Content.Items.Accessories;
using Terraria;
using Terraria.DataStructures;

namespace LivingWorldMod.Custom.Utilities {
    /// <summary>
    /// Utilities class that holds methods that pertains to players.
    /// </summary>
    public static class PlayerUtils {
        /// <summary>
        /// Calculates and returns the entirety of the savings of the player in all applicable inventories.
        /// </summary>
        /// <param name="player"> </param>
        /// <returns> </returns>
        public static long CalculateTotalSavings(this Player player) {
            bool _;

            long playerInvCashCount = Utils.CoinsCount(out _, player.inventory);
            long piggyCashCount = Utils.CoinsCount(out _, player.bank.item);
            long safeCashCount = Utils.CoinsCount(out _, player.bank2.item);
            long defForgeCashCount = Utils.CoinsCount(out _, player.bank3.item);
            long voidVaultCashCount = Utils.CoinsCount(out _, player.bank4.item);

            return Utils.CoinsCombineStacks(out _, playerInvCashCount, piggyCashCount, safeCashCount, defForgeCashCount, voidVaultCashCount);
        }

        /// <summary>
        /// A wrapper method for the Player.KillMe method that respects LivingWorldMod's
        /// <seealso cref="AccessoryItem.PrePlayerForceKill"/> method.
        /// </summary>
        public static void AttemptForceKill(this Player player, PlayerDeathReason deathReason) {
            bool shouldKill = true;
            foreach (AccessoryItem accessory in player.GetModPlayer<AccessoryPlayer>().ActiveAccessoryItems) {
                if (!accessory.PrePlayerForceKill(player, deathReason)) {
                    shouldKill = false;
                }
            }

            if (shouldKill) {
                player.KillMe(deathReason, player.statLife, 0);
            }
        }
    }
}