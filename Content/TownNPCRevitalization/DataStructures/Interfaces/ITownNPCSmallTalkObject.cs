using LivingWorldMod.DataStructures.Records;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;

/// <summary>
///     Represents an arbitrary object that can be spoken to (or at) for the purpose of Town NPC small talk.
/// </summary>
public interface ITownNPCSmallTalkObject {
    /// <summary>
    ///     The time, in ticks, that must pass before this object can receive small talk again.
    /// </summary>
    public BoundedNumber<int> SmallTalkReceptionCooldown {
        get;
        set;
    }

    /// <summary>
    ///     The localization category under "TownNPCSmallTalk" in the localization files, used to determine which flavor-text should be used for the interaction. For example, if the small talk object is
    ///     another Town NPC, this value would be their type name, i.e "Guide" or "BestiaryGirl".
    /// </summary>
    public string SmallTalkLocalizationCategory {
        get;
    }

    /// <summary>
    ///     The actual value that will be formatted/substituted into the small talk sentence when it is generated. For example, if the small talk object is another Town NPC, this value should be their
    ///     display name.
    /// </summary>
    public string SmallTalkFlavorTextSubstitution {
        get;
    }
}