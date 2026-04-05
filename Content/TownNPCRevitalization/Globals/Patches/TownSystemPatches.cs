using System.Linq;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using LivingWorldMod.DataStructures.Classes;
using Microsoft.Xna.Framework;
using Terraria.GameContent;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;

/// <summary>
///     Patch class that handles the necessary patches/detours for stitching together Vanilla's <see cref="TownRoomManager" /> and LWM's <see cref="TownNPCTownSystem" />
/// </summary>
public class TownSystemPatches : LoadablePatch {
    private static Point GetRoomBeforeRemoval(TownRoomManager roomManager, int npcType) {
        Point roomPos;
        lock (TownRoomManager.EntityCreationLock) {
            roomPos = roomManager._roomLocationPairs.FirstOrDefault(pair => pair.Item1 == npcType)?.Item2 ?? Point.Zero;
        }

        return roomPos;
    }

    public override void LoadPatches() {
        On_TownRoomManager.SetRoom_int_Point += OnSetRoomRefreshLWMTowns;
        On_TownRoomManager.KickOut_int += OnKickOutRefreshLWMTowns;
    }

    private void OnSetRoomRefreshLWMTowns(On_TownRoomManager.orig_SetRoom_int_Point orig, TownRoomManager self, int npcID, Point pt) {
        Point previousRoomPos = GetRoomBeforeRemoval(self, npcID);

        orig(self, npcID, pt);

        if (previousRoomPos != Point.Zero) {
            TownNPCTownSystem.Instance.RemoveRoomFromTown(previousRoomPos, false);
        }

        TownNPCTownSystem.Instance.AddRoomToTown(pt);
    }

    private void OnKickOutRefreshLWMTowns(On_TownRoomManager.orig_KickOut_int orig, TownRoomManager self, int npcType) {
        Point roomPos = GetRoomBeforeRemoval(self, npcType);
        orig(self, npcType);

        if (roomPos == Point.Zero) {
            return;
        }

        TownNPCTownSystem.Instance.RemoveRoomFromTown(roomPos);
    }
}