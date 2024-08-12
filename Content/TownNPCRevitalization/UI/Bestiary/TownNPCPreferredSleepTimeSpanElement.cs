using System.Globalization;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.DataStructures.Interfaces;
using LivingWorldMod.Globals.UIElements;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace LivingWorldMod.Content.TownNPCRevitalization.UI.Bestiary;

public class TownNPCPreferredSleepTimeSpanElement (int npcType) : IBestiaryInfoElement, IBestiaryCategorizedElement {
    public UIBestiaryEntryInfoPage.BestiaryInfoCategory InfoCategory => UIBestiaryEntryInfoPage.BestiaryInfoCategory.FlavorText;

    public UIElement ProvideUIElement(BestiaryUICollectionInfo info) {
        // Adapted vanilla code
        UITooltipElement elementZone = new("UI.Bestiary.SleepIntervalTooltip".Localized()) { Width = StyleDimension.Fill, Height = StyleDimension.FromPixels(30f) };

        UIPanel backPanel = new (Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Stat_Panel"), null, customBarSize: 7) {
            IgnoresMouseInteraction = true,
            Width = StyleDimension.FromPixelsAndPercent(-11f, 1f),
            Height = StyleDimension.FromPixels(30f),
            BackgroundColor = new Color(43, 56, 101),
            BorderColor = Color.Transparent,
            Left = StyleDimension.FromPixels(-8f),
            HAlign = 1f
        };
        backPanel.SetPadding(0f);
        elementZone.Append(backPanel);

        SleepProfile sleepProfile = TownNPCSleepModule.GetSleepProfileOrDefault(npcType);
        CultureInfo currentInGameCulture = Language.ActiveCulture.CultureInfo;
        UIText sleepIntervalElement = new ($"{sleepProfile.StartTime.ToString("t", currentInGameCulture)} - {sleepProfile.EndTime.ToString("t", currentInGameCulture)}", 0.85f) {
            HAlign = 1f, VAlign = 0.5f, Left = StyleDimension.FromPixels(-5f)
        };
        backPanel.Append(sleepIntervalElement);

        Main.instance.LoadItem(ItemID.SleepingIcon);
        UIImage sleepIconElement = new (TextureAssets.Item[ItemID.SleepingIcon]) { VAlign = 0.5f, Left = StyleDimension.FromPixels(5f) };
        backPanel.Append(sleepIconElement);

        return elementZone;
    }
}