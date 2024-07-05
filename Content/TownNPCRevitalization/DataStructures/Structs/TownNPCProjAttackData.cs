namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;

/// <summary>
///     Struct holding data for Town NPC attacks where a projectile is created.
/// </summary>
public struct TownNPCProjAttackData (
    int projType,
    int projDamage,
    float knockBack,
    float speedMult,
    int attackDelay,
    int attackCooldown,
    int maxValue,
    float gravityCorrection,
    float dangerDetectRange,
    float randomOffset
) {
    public int projType = projType;
    public int projDamage = projDamage;
    public float knockBack = knockBack;
    public float speedMult = speedMult;
    public int attackDelay = attackDelay;
    public int attackCooldown = attackCooldown;
    public int maxValue = maxValue;
    public float gravityCorrection = gravityCorrection;
    public float dangerDetectRange = dangerDetectRange;
    public float randomOffset = randomOffset;
}