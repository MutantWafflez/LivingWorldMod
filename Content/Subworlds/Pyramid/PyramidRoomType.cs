namespace LivingWorldMod.Content.Subworlds.Pyramid {
    /// <summary>
    /// The different types of room a Pyramid Dungeon room can be.
    /// </summary>
    public enum PyramidRoomType {

        /// <summary>
        /// Normal room. Normal chance for everything.
        /// </summary>
        Normal,
        /// <summary>
        /// As it is on the tin. Cursed in some way; chooses a random
        /// curse to inflict while within the room.
        /// </summary>
        Cursed,
        /// <summary>
        /// No enemies. Bad things only happen if the puzzle is failed.
        /// </summary>
        Puzzle,
        /// <summary>
        /// No enemies, room is immediately cleared upon entry, and has loot
        /// of some kind.
        /// </summary>
        Treasure
    }
}
