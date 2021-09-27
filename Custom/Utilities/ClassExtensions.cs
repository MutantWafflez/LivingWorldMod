using LivingWorldMod.Content.NPCs.Villagers;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace LivingWorldMod.Custom.Utilities {

    /// <summary>
    /// Class that holds all of the class extending methods that this mod uses.
    /// </summary>
    public static class ClassExtensions {

        /// <summary>
        /// Returns whether or not this npc is any type of Villager.
        /// </summary>
        /// <param name="npc"> </param>
        /// <returns> </returns>
        public static bool IsTypeOfVillager(this NPC npc) {
            return npc.ModNPC?.GetType().IsSubclassOf(typeof(Villager)) ?? false;
        }

        /// <summary>
        /// Returns whether or not this npc is any type of Villager and has an out parameter for
        /// returning the villager ModNPC class pertaining to this NPC if applicable.
        /// </summary>
        public static bool IsTypeOfVillager(this NPC npc, out Villager villager) {
            bool isVillager = npc.ModNPC?.GetType().IsSubclassOf(typeof(Villager)) ?? false;
            villager = isVillager ? (Villager)npc.ModNPC : null;
            return isVillager;
        }

        /// <summary>
        /// Adds all of the objects and their weights within another list into this list.
        /// </summary>
        /// <param name="originalList"> List that will be added to. </param>
        /// <param name="list">
        /// List that will have its objects and theirs weights added to the other list.
        /// </param>
        public static void AddList<T>(this WeightedRandom<T> originalList, WeightedRandom<T> list) {
            foreach ((T obj, double weight) in list.elements) {
                originalList.Add(obj, weight);
            }
        }

        /// <summary>
        /// Adds to the <see cref="WeightedRandom{T}"/> only if the given condition returns true.
        /// </summary>
        /// <param name="list"> List to add to. </param>
        /// <param name="obj"> Object to add to the list. </param>
        /// <param name="condition">
        /// Condition that determines whether or not to add the given object to the list.
        /// </param>
        /// <param name="weight"> The potential weight of the object, if required. </param>
        public static void AddConditionally<T>(this WeightedRandom<T> list, T obj, bool condition, double weight = 1) {
            if (condition) {
                list.Add(obj, weight);
            }
        }

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
        /// Short-hand method to determine whether or not a modded tile entity exists at the given
        /// location in TILE coordinates.
        /// </summary>
        /// <param name="entity"> The entity type in question. </param>
        /// <param name="x"> The x coordinate to test for a tile entity. </param>
        /// <param name="y"> The y coordinate to test for a tile entity. </param>
        /// <returns> </returns>
        public static bool EntityExistsHere(this ModTileEntity entity, int x, int y) => entity.Find(x, y) >= 0;
    }
}