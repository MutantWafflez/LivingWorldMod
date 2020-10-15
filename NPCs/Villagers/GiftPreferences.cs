using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

namespace LivingWorldMod.NPCs.Villagers {
    public struct GiftPreferences {
        private List<Tuple<short, int>> likedGifts;

        private List<Tuple<short, int>> dislikedGifts;

        public GiftPreferences(List<Tuple<short, int>> likedGifts, List<Tuple<short, int>> dislikedGifts) {
            this.likedGifts = likedGifts;
            this.dislikedGifts = dislikedGifts;
        }

        /// <summary>
        /// Searches both the likedGifts and dislikedGifts list for itemType, and returns the number it modifies Reputation by.
        /// </summary>
        /// <param name="itemType">The type of the item being searched.</param>
        public int GetReputationModifier(short itemType) {
            int reputationModification = 0;

            var likedResult = likedGifts.FirstOrDefault(gift => gift.Item1 == itemType);
            reputationModification = likedResult != null ? likedResult.Item2 : reputationModification;

            var dislikedResult = dislikedGifts.FirstOrDefault(gift => gift.Item1 == itemType);
            reputationModification = dislikedResult != null ? dislikedResult.Item2 : reputationModification;

            return reputationModification;
        }

        public static GiftPreferences[] InitializeGiftArray() {
            return new GiftPreferences[(int)VillagerType.VillagerTypeCount] 
            {
                //Harpy
                new GiftPreferences(new List<Tuple<short, int>> 
                {
                    Tuple.Create(ItemID.FallenStar, 5),
                    Tuple.Create(ItemID.Worm, 3)
                },
                new List<Tuple<short, int>> {
                    Tuple.Create(ItemID.Feather, -3),
                    Tuple.Create(ItemID.GiantHarpyFeather, -5)
                }
                ),
                //Lihzahrd
                new GiftPreferences(new List<Tuple<short, int>>
                {

                }, 
                new List<Tuple<short, int>>
                {

                }
                )
            };
        }

    }
}
