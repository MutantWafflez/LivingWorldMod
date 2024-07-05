using Terraria.ModLoader.IO;

namespace LivingWorldMod.Globals.Players;

/// <summary>
///     ModPlayer that handles all saving and applying of buffs that are permanent
///     once they are acquired.
/// </summary>
public class PermanentBuffPlayer : ModPlayer {
    /// <summary>
    ///     Permanently increases <see cref="Player.moveSpeed" /> by 5%.
    /// </summary>
    public bool effervescentNuggetBuff;

    public override void Initialize() {
        effervescentNuggetBuff = false;
    }

    public override void SaveData(TagCompound tag) {
        tag["effervescentNuggetBuff"] = effervescentNuggetBuff;
    }

    public override void LoadData(TagCompound tag) {
        effervescentNuggetBuff = tag.GetBool("effervescentNuggetBuff");
    }

    public override void PostUpdateRunSpeeds() {
        if (effervescentNuggetBuff) {
            Player.moveSpeed += 0.05f;
        }
    }
}