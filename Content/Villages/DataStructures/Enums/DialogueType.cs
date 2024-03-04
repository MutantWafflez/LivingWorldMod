namespace LivingWorldMod.Content.Villages.DataStructures.Enums;

/// <summary>
/// Enum that specifies what kind of dialogue is wanted in tandem with the Dialogue request system for
/// villagers.
/// </summary>
public enum DialogueType {
    /// <summary>
    /// "Normal" dialogue, or what appears when you right click a villager and talk to them.
    /// </summary>
    Normal,

    /// <summary>
    /// Dialogue that appears in the shop dialogue box when the player FIRST opens a villager's shop.
    /// </summary>
    ShopInitial,

    /// <summary>
    /// Dialogue that appears in the shop dialogue box when the player buys an item from the villager.
    /// </summary>
    ShopBuy
}