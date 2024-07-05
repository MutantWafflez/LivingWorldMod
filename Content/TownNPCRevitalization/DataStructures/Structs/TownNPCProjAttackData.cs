namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;

/// <summary>
///     Struct holding data for Town NPC attacks where a projectile is created.
/// </summary>
public struct TownNPCProjAttackData {
    public int projType;
    public int projDamage;
    public float knockBack;
    public float speedMult;
    public int attackDelay;
    public int attackCooldown;
    public int maxValue;
    public float gravityCorrection;
    public float dangerDetectRange;
    public float randomOffset;

    public TownNPCProjAttackData(
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
        this.projType = projType;
        this.projDamage = projDamage;
        this.knockBack = knockBack;
        this.speedMult = speedMult;
        this.attackDelay = attackDelay;
        this.attackCooldown = attackCooldown;
        this.maxValue = maxValue;
        this.gravityCorrection = gravityCorrection;
        this.dangerDetectRange = dangerDetectRange;
        this.randomOffset = randomOffset;
    }
}