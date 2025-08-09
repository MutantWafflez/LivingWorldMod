using System;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.TownNPCRevitalization.AIStates;

public class DogFetchAIState : TownNPCAIState {
    public override int ReservedStateInteger => 21;

    public override void DoState(NPC npc) {
        Point dogStandPosition = (Main.LocalPlayer.BottomLeft + new Vector2(0, -2f)).ToTileCoordinates();

        dogStandPosition += new Point(Main.LocalPlayer.direction * 2 + (Main.LocalPlayer.direction == 1 ? (int)Math.Ceiling(Main.LocalPlayer.width / 16f) - 1 : 0), 0);

        npc.GetGlobalNPC<TownNPCPathfinderModule>().RequestPathfind(dogStandPosition);
    }
}