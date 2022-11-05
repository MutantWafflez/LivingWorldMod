using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.ModTypes {
    public abstract class PyramidRoomCurse : ModType {
        /// <summary>
        /// The numerical type of this Curse. Auto-assigned at initialization.
        /// </summary>
        public int CurseType {
            get;
            private set;
        }

        /// <summary>
        /// The count of all PyramidRoomCurse extensions/objects that exist.
        /// </summary>
        public static int CurseTypeCount {
            get;
            private set;
        }

        /// <summary>
        /// Gets the numerical type of this Curse.
        /// </summary>
        public static int GetCurseType<T>() where T : PyramidRoomCurse => ModContent.GetInstance<T>().CurseType;

        /// <summary>
        /// Is called once when the curse is first applied. Use this to generate
        /// something in the room.
        /// </summary>
        /// <param name="roomRegion"></param>
        public virtual void DoGenerationEffect(Rectangle roomRegion) { }

        /// <summary>
        /// Called when a player is damaged.
        /// </summary>
        public virtual void PlayerHurt(Player player, bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter) { }

        /// <summary>
        /// Called every tick right before a player's buffs are updated.
        /// </summary>
        /// <param name="player"></param>
        public virtual void PlayerPreUpdateBuffs(Player player) { }

        /// <summary>
        /// Called when a player's equips are updated.
        /// </summary>
        public virtual void PlayerUpdateEquips(Player player) { }

        /// <summary>
        /// Called whenever a player shoots a projectile with an item.
        /// </summary>
        public virtual void PlayerModifyShoot(Player player, Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) { }

        protected override void Register() {
            ModTypeLookup<PyramidRoomCurse>.Register(this);
            CurseType = CurseTypeCount;
            CurseTypeCount++;
        }

        public sealed override void SetupContent() => SetStaticDefaults();
    }
}