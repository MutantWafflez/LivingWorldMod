using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;
using LivingWorldMod.DataStructures.Records;
using Terraria.GameContent.UI;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Players;

/// <summary>
///     Player that stores/tracks the "small talk" conversations that Town NPCs can have with the player, such that they don't talk full priority over inter-Town NPC chit-chat.
/// </summary>
public class TownNPCSmallTalkPlayer : ModPlayer, ITownNPCSmallTalkObject {
    public const string PlayerSmallTalkLocalizationCategory = "Player";

    /// <summary>
    ///     How many ticks that are remaining before a Town NPC can attempt to do small talk with this player again.
    /// </summary>
    public BoundedNumber<int> SmallTalkReceptionCooldown {
        get;
        set;
    } = new(0, 0, int.MaxValue);

    public string SmallTalkLocalizationCategory => PlayerSmallTalkLocalizationCategory;

    public string SmallTalkFlavorTextSubstitution => Player.name;

    public WorldUIAnchor ObjectAnchor => new(Player);

    public override void PostUpdate() {
        SmallTalkReceptionCooldown -= 1;
    }
}