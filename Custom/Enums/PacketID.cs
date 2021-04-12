namespace LivingWorldMod.Custom.Enums
{
    /// <summary>
    /// Packet types. Each one is more or less expected to have an associated subclass of LWMPacket.
    /// IMPORTANT: Remember to add new packet types to RegisterAllHandlers()!
    /// </summary>
    public enum PacketID : byte
    {
        PlayerData, // client -> server, syncs GUID
        VillagerData,
        LimitedPurchase, // client -> server, notification of purchases
        PacketCount
    }
}