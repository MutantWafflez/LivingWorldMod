using LivingWorldMod.Common.ModTypes;
using Terraria;
using Terraria.ID;

namespace LivingWorldMod.Content.Subworlds.Pyramid.RoomCurses {
    /// <summary>
    /// Curse that applies the confusion buff on players.
    /// </summary>
    public sealed class ConfusionCurse : PyramidRoomCurse {
        public override void PlayerPreUpdateBuffs(Player player) {
            player.AddBuff(BuffID.Confused, 15);
        }
    }
}