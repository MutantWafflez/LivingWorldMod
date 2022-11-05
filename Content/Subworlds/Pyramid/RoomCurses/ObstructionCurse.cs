using LivingWorldMod.Common.ModTypes;
using Terraria;
using Terraria.ID;

namespace LivingWorldMod.Content.Subworlds.Pyramid.RoomCurses {
    /// <summary>
    /// Gives the player the obstruction debuff.
    /// </summary>
    public sealed class ObstructionCurse : PyramidRoomCurse {
        public override void PlayerPreUpdateBuffs(Player player) {
            player.AddBuff(BuffID.Obstructed, 15);
        }
    }
}