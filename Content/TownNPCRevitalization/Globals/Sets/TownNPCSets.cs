namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Sets;

/// <summary>
///     Sets that correspond to Town NPCs, specifically for the Town NPC Revitalization
///     Content set.
/// </summary>
public static class TownNPCSets {
    /// <summary>
    ///     Whether a given Town NPC gives irritable dialogue when spoken to by the player during the
    ///     Blood Moon event.
    /// </summary>
    public static readonly bool[] IrritatedByBloodMoon = Factory.CreateBoolSet(false, NPCID.Nurse, NPCID.BestiaryGirl, NPCID.Stylist, NPCID.Dryad, NPCID.Mechanic, NPCID.Steampunker);

    private static SetFactory Factory => NPCID.Sets.Factory;
}