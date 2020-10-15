using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

namespace LivingWorldMod.NPCs.Villagers {
    public struct GiftPreferences {
        private List<int> likedGifts;

        private List<int> dislikedGifts;

        public GiftPreferences(List<int> likedGifts, List<int> dislikedGifts) {
            this.likedGifts = likedGifts;
            this.dislikedGifts = dislikedGifts;
        }

        /// <summary>
        /// Returns whether or not item of a given type is contained within the likedGifts list.
        /// </summary>
        /// <param name="itemType">The type of the item to look for.</param>
        public bool LikedGiftsContains(int itemType) {
            return likedGifts.Contains(itemType);
        }

        /// <summary>
        /// Returns whether or not item of a given type is contained within the dislikedGifts list.
        /// </summary>
        /// <param name="itemType">The type of the item to look for.</param>
        public bool DislikedGiftsContains(int itemType) {
            return dislikedGifts.Contains(itemType);
        }

        public static GiftPreferences[] InitializeGiftArray() {
            return new GiftPreferences[(int)VillagerType.VillagerTypeCount] 
            {
                //Harpy
                new GiftPreferences(new List<int> 
                {
                    ItemID.FallenStar,
                    ItemID.Worm
                },
                new List<int> {
                    ItemID.Feather
                }
                ),
                //Lihzahrd
                new GiftPreferences(new List<int> 
                {

                }, 
                new List<int> 
                {

                }
                )
            };
        }

    }
}
