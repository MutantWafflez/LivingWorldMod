namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;

/// <summary>
///     ModType that denotes a certain state of being for Town NPCs.
/// </summary>
public abstract class TownNPCAIState : ModType {
    /// <summary>
    ///     The next integer value that will be given to <see cref="ReservedStateInteger" />
    ///     when <see cref="ReserveStateInteger" /> is called.
    /// </summary>
    private static int NextReservedStateInteger = -1;

    /// <summary>
    ///     The count of all TownNPCAIState extensions/objects that exist.
    /// </summary>
    public static byte StateCount {
        get;
        private set;
    }

    /// <summary>
    ///     This is the number that will be used to denote that the Town NPC
    ///     is in this AI state. Different AI State classes cannot have a same value
    ///     for this property.
    /// </summary>
    /// <remarks>
    ///     <b>NOTE:</b> If you override this property, make sure to give it a POSITIVE
    ///     integer value, to ensure there is no clash with the automatic reservation.
    /// </remarks>
    public virtual int ReservedStateInteger {
        get;
    } = ReserveStateInteger();

    /// <summary>
    ///     The internal numeric type of this state. Auto-assigned at initialization.
    /// </summary>
    public byte Type {
        get;
        private set;
    }

    /// <summary>
    ///     Retrieves the state integer associated with the type <typeparamref name="T" />.
    /// </summary>
    public static int GetStateInteger<T>()
        where T : TownNPCAIState => ModContent.GetInstance<T>().ReservedStateInteger;

    protected static int ReserveStateInteger() => NextReservedStateInteger--;

    public sealed override void SetupContent() => SetStaticDefaults();

    protected sealed override void Register() {
        ModTypeLookup<TownNPCAIState>.Register(this);
        Type = StateCount++;
    }

    /// <summary>
    ///     Called every tick that the given Town NPC is in this state.
    /// </summary>
    public abstract void DoState( NPC npc);
}