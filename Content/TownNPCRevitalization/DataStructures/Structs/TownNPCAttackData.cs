namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;

public struct TownNPCAttackData(
    int damage,
    float knockBack,
    int maxValue,
    int attackCooldown,
    int itemWidth,
    int itemHeight,
    int projType,
    float speedMult,
    int attackDelay,
    float gravityCorrection,
    float randomOffset
) {
    public int damage = damage;
    public float knockBack = knockBack;
    public int maxValue = maxValue;
    public int attackCooldown = attackCooldown;
    public int itemWidth = itemWidth;
    public int itemHeight = itemHeight;
    public int projType = projType;
    public float speedMult = speedMult;
    public int attackDelay = attackDelay;
    public float gravityCorrection = gravityCorrection;
    public float randomOffset = randomOffset;
}