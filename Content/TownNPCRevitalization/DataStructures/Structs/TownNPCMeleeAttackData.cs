namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;

/// <summary>
///     Struct holding data for Town NPC attacks where a melee weapon is used.
/// </summary>
public struct TownNPCMeleeAttackData (int attackCooldown, int maxValue, int damage, float knockBack, int itemWidth, int itemHeight) {
    public int attackCooldown = attackCooldown;
    public int maxValue = maxValue;
    public int damage = damage;
    public float knockBack = knockBack;
    public int itemWidth = itemWidth;
    public int itemHeight = itemHeight;
}