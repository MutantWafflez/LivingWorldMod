using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.DataStructures.Structs;
using LivingWorldMod.Utilities;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.ShopPersonalityTraits;

public class NumericBiomePreferenceTrait(int moodOffset, IShoppingBiome biome) : IShopPersonalityTrait {
    public override string ToString() => $"Biome: {biome.NameKey}, Offset: {moodOffset}";

    public void ModifyShopPrice(HelperInfo info, ShopHelper shopHelperInstance) {
        if (!biome.IsInBiome(info.player))  {
            return;
        }

        info.npc.GetGlobalNPC<TownGlobalNPC>()
            .MoodModule.AddModifier(
                new SubstitutableLocalizedText("TownNPCMoodDescription.InBiome".Localized(), new { Biome = ShopHelper.BiomeNameByKey(biome.NameKey) }),
                $"TownNPCMood.{LWMUtils.GetNPCTypeNameOrIDName(info.npc.type)}.Biome_{biome.NameKey}".Localized(),
                moodOffset,
                0
            );
    }
}