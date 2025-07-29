using LivingWorldMod.Content.DevSets.MutantWafflez;
using Terraria.GameContent.ItemDropRules;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs.TownNPCModules;

/// <summary>
///     Specific module for the <see cref="NPCID.WitchDoctor" /> Town NPC that handles his special loot/item drops.
/// </summary>
public sealed class WitchDoctorLootModule : TownNPCModule {
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => lateInstantiation && entity.type == NPCID.WitchDoctor;

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot) {
        int[] mutantDevSetDrops = [ModContent.ItemType<MutantDevHead>(), ModContent.ItemType<MutantDevBody>(), ModContent.ItemType<MutantDevLegs>()];

        foreach (int itemType in mutantDevSetDrops) {
            npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NamedNPC("Tairree"), itemType));
        }
    }
}