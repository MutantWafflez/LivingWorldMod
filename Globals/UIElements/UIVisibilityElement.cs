using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Terraria.UI;

namespace LivingWorldMod.Globals.UIElements;

/// <summary>
///     Super simple element that can have its visibility toggled. When this element is hidden, the <see cref="UIElement.Elements" /> list will be swapped out for an empty one. When visible, any stored
///     hidden elements will be added back to the <see cref="UIElement.Elements" /> list.
/// </summary>
public class UIVisibilityElement : UIElement {
    private readonly bool _shouldUpdateWhileHidden;

    private List<UIElement> _hiddenElements = [];

    /// <summary>
    ///     Whether or not this element is currently visible.
    /// </summary>
    public bool IsVisible {
        get;
        private set;
    }

    public UIVisibilityElement(bool shouldUpdateWhileHidden = false) {
        _shouldUpdateWhileHidden = shouldUpdateWhileHidden;
    }

    public override void Update(GameTime gameTime) {
        if (!IsVisible && _shouldUpdateWhileHidden) {
            foreach (UIElement element in _hiddenElements) {
                element.Update(gameTime);
            }

            return;
        }

        base.Update(gameTime);
    }

    public void SetVisibility(bool newValue) {
        if (IsVisible == newValue) {
            return;
        }

        if (!IsVisible) {
            Unsafe.AsRef(in Elements) = _hiddenElements;
            _hiddenElements = [];
        }
        else {
            _hiddenElements = Elements;
            Unsafe.AsRef(in Elements) = [];
        }

        IsVisible = newValue;
    }

    public new void Append(UIElement element) {
        element.Remove();
        element.Parent = this;

        if (!IsVisible) {
            _hiddenElements.Add(element);
        }
        else {
            Elements.Add(element);
        }

        element.Recalculate();
    }
}