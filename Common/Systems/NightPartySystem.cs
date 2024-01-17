using System.Linq;
using LivingWorldMod.Common.Systems.BaseSystems;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Achievements;
using Terraria.GameContent.Events;
using Terraria.Localization;
using Party = Terraria.GameContent.Events.BirthdayParty;

namespace LivingWorldMod.Common.Systems;

/// <summary>
/// ModSystem that handles special "night" parties that happen in tandem with
/// lantern nights.
/// </summary>
[Autoload(false)]
public sealed class NightPartySystem : BaseModSystem<NightPartySystem> {
    private bool shouldNightParty;

    public override void PreUpdateTime() {
        shouldNightParty = LanternNight.NextNightIsLanternNight;
    }

    public override void PostUpdateTime() {
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            return;
        }

        if (shouldNightParty && !Main.dayTime && Main.time == 0) {
            StartNightParty();
        }
        else if (Main.dayTime && Main.time == 0) {
            // Method doesn't do any time checks itself, it will just stop a party if one is happening
            Party.CheckNight();
        }
    }

    private void StartNightParty() {
        Party.GenuineParty = true;
        Party.PartyDaysOnCooldown = Main.rand.Next(1, 3);
        Party.CelebratingNPCs = LWMUtils.GetAllNPCs(Party.CanNPCParty).Select(npc => npc.whoAmI).ToList();
        NPC.freeCake = true;

        Color partyColor = new(255, 0, 160);
        WorldGen.BroadcastText(NetworkText.FromKey("Mods.LivingWorldMod.Event.NightPartyStarted"), partyColor);

        NetMessage.SendData(MessageID.WorldData);
        AchievementsHelper.NotifyProgressionEvent(25);
    }
}