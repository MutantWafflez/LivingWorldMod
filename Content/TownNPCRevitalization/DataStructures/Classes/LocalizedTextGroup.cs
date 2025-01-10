using Terraria.Localization;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes;

/// <summary>
///     An arbitrary grouping of <see cref="LocalizedText" /> instances, provided verbatim or implicitly by a <see cref="LanguageSearchFilter" /> object. Supports both lazy-loading and instant loading.
/// </summary>
public class LocalizedTextGroup {
    private readonly LanguageSearchFilter _filter;
    private bool _isLoaded;

    private LocalizedText[] _texts;

    public LocalizedText[] Texts {
        get {
            EnsureLoadedTexts();

            return _texts;
        }
        private init => _texts = value;
    }

    public LocalizedText RandomText => Main.rand.Next(Texts);

    public LocalizedTextGroup(LanguageSearchFilter filter, bool immediateLoad = false) {
        _filter = filter;
        if (!immediateLoad) {
            return;
        }

        EnsureLoadedTexts();
    }

    public LocalizedTextGroup(LocalizedText[] texts) {
        Texts = texts;
        _isLoaded = true;
    }

    private void EnsureLoadedTexts() {
        if (_isLoaded) {
            return;
        }

        _texts = Language.FindAll(_filter);
        _isLoaded = true;
    }
}