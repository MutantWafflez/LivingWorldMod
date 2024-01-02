namespace LivingWorldMod.Custom.Structs {
    /// <summary>
    /// Struct holding data for Town NPC attacks where a melee weapon is used.
    /// </summary>
    public struct TownNPCMeleeAttackData {
        public int attackCooldown;
        public int maxValue;
        public int damage;
        public float knockBack;
        public int itemWidth;
        public int itemHeight;

        public TownNPCMeleeAttackData(int attackCooldown, int maxValue, int damage, float knockBack, int itemWidth, int itemHeight) {
            this.attackCooldown = attackCooldown;
            this.maxValue = maxValue;
            this.damage = damage;
            this.knockBack = knockBack;
            this.itemWidth = itemWidth;
            this.itemHeight = itemHeight;
        }
    }
}