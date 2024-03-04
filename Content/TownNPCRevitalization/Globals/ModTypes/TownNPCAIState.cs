using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;

/// <summary>
/// ModType that denotes a certain state of being for Town NPCs.
/// </summary>
public abstract class TownNPCAIState : ModType {
    /// <summary>
    /// The next integer value that will be given to <see cref="ReservedStateInteger"/>
    /// when <see cref="ReserveStateInteger"/> is called.
    /// </summary>
    private static int NextReservedStateInteger = -1;

    /// <summary>
    /// This is the number that will be used to denote that the Town NPC
    /// is in this AI state. Different AI State classes cannot have a same value
    /// for this property.
    /// </summary>
    /// <remarks>
    /// <b>NOTE:</b> If you override this property, make sure to give it a POSITIVE
    /// integer value, to ensure there is no clash with the automatic reservation.
    /// </remarks>
    public virtual int ReservedStateInteger {
        get;
    } = ReserveStateInteger();

    /// <summary>
    /// The internal numeric type of this state. Auto-assigned at initialization.
    /// </summary>
    public byte Type {
        get;
        private set;
    }

    /// <summary>
    /// The count of all TownNPCAIState extensions/objects that exist.
    /// </summary>
    public static byte StateCount {
        get;
        private set;
    }

    /// <summary>
    /// Called every draw frame that the given Town NPC is doing the given activity
    /// (see <see cref="DoActivity"/>). Use this to frame the NPC to a specific animation
    /// frame for this activity.
    /// </summary>
    /// <remarks>
    /// The <see cref="TownGlobalNPC"/> parameter is added for your convenience, so you don't have to manually call
    /// <see cref="NPC.GetGlobalNPC"/>.
    /// </remarks>
    public virtual void FrameNPC(TownGlobalNPC globalNPC, NPC npc, int frameHeight) { }

    /// <summary>
    /// Called every draw frame that the given Town NPC is doing the given activity
    /// (see <see cref="DoActivity"/>). Use this to draw things BEHIND the NPC.
    /// </summary>
    /// <remarks>
    /// The <see cref="TownGlobalNPC"/> parameter is added for your convenience, so you don't have to manually call
    /// <see cref="NPC.GetGlobalNPC"/>.
    /// </remarks>
    public virtual void PreDrawNPC(TownGlobalNPC globalNPC, NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }

    /// <summary>
    /// Called every draw frame that the given Town NPC is doing the given activity
    /// (see <see cref="DoActivity"/>). Use this to draw things IN FRONT OF the NPC.
    /// </summary>
    /// <remarks>
    /// The <see cref="TownGlobalNPC"/> parameter is added for your convenience, so you don't have to manually call
    /// <see cref="NPC.GetGlobalNPC"/>.
    /// </remarks>
    public virtual void PostDrawNPC(TownGlobalNPC globalNPC, NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }

    public sealed override void SetupContent() => SetStaticDefaults();

    /// <summary>
    /// Called every tick that the given Town NPC is in this state.
    /// </summary>
    public abstract void DoState(TownGlobalNPC globalNPC, NPC npc);

    /// <summary>
    /// Retrieves the state integer associated with the type <typeparamref name="T"/>.
    /// </summary>
    public static int GetStateInteger<T>()
        where T : TownNPCAIState => ModContent.GetInstance<T>().ReservedStateInteger;

    protected sealed override void Register() {
        ModTypeLookup<TownNPCAIState>.Register(this);
        Type = StateCount++;
    }

    protected static int ReserveStateInteger() => NextReservedStateInteger--;
}