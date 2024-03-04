using LivingWorldMod.Content.Villages.HarpyVillage.Food;
using LivingWorldMod.Custom.Classes;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;

namespace LivingWorldMod.Globals.Patches;

/// <summary>
/// Patches that relate to player drawing.
/// </summary>
public class PlayerDrawPatches : LoadablePatch {
    public override void LoadPatches() {
        On_PlayerDrawLayers.DrawPlayer_RenderAllLayers += AddShaderToHeldItem;
    }

    private void AddShaderToHeldItem(On_PlayerDrawLayers.orig_DrawPlayer_RenderAllLayers orig, ref PlayerDrawSet drawinfo) {
        if (drawinfo.heldItem.type == ModContent.ItemType<EffervescentNugget>()
            && drawinfo.DrawDataCache.FindIndex(data => data.texture == TextureAssets.Item[ItemID.ChickenNugget].Value) is var index and > 0) {
            DrawData drawItemData = drawinfo.DrawDataCache[index];
            drawItemData.shader = GameShaders.Armor.GetShaderIdFromItemId(ItemID.HallowBossDye);

            drawinfo.DrawDataCache.RemoveAt(index);
            drawinfo.DrawDataCache.Insert(index, drawItemData);
        }

        orig(ref drawinfo);
    }
}