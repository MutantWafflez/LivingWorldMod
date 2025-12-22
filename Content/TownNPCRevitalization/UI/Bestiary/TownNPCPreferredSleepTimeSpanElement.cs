using System.Globalization;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;
using LivingWorldMod.Globals.UIElements;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace LivingWorldMod.Content.TownNPCRevitalization.UI.Bestiary;

public class TownNPCPreferredSleepTimeSpanElement (int npcType) : IBestiaryInfoElement, ICategorizedBestiaryInfoElement {
    public UIBestiaryEntryInfoPage.BestiaryInfoCategory ElementCategory => UIBestiaryEntryInfoPage.BestiaryInfoCategory.FlavorText;

    public UIElement ProvideUIElement(BestiaryUICollectionInfo info) {
        if (info.UnlockState <= BestiaryEntryUnlockState.NotKnownAtAll_0) {
            return null;
        }

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

        SleepSchedule sleepSchedule = TownNPCDataSystem.ProfileDatabase[npcType].SleepSchedule;
        CultureInfo currentInGameCulture = Language.ActiveCulture.CultureInfo;
        UIText sleepIntervalElement = new ($"{sleepSchedule.StartTime.ToString("t", currentInGameCulture)} - {sleepSchedule.EndTime.ToString("t", currentInGameCulture)}", 0.85f) {
            HAlign = 1f, VAlign = 0.5f, Left = StyleDimension.FromPixels(-5f)
        };
        backPanel.Append(sleepIntervalElement);

        Main.instance.LoadItem(ItemID.SleepingIcon);
        UIImage sleepIconElement = new (TextureAssets.Item[ItemID.SleepingIcon]) { VAlign = 0.5f, Left = StyleDimension.FromPixels(5f) };
        backPanel.Append(sleepIconElement);

        return elementZone;
    }
}