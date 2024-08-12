using Terraria.GameContent.UI.Elements;

namespace LivingWorldMod.DataStructures.Interfaces;

/// <summary>
///     Abstraction over vanilla's hardcoded categorization method, using <see cref="UIBestiaryEntryInfoPage.BestiaryInfoCategory" /> with this interface.
/// </summary>
public interface IBestiaryCategorizedElement {
    public UIBestiaryEntryInfoPage.BestiaryInfoCategory InfoCategory {
        get;
    }
}