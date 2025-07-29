using LivingWorldMod.Globals.BaseTypes.Items;

namespace LivingWorldMod.Content.DevSets.MutantWafflez;

[AutoloadEquip(EquipType.Head)]
public class MutantDevHead : BaseItem {
    public override void SetStaticDefaults() {
        ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
    }

    public override void SetDefaults() {
        Item.vanity = true;
        Item.value = Item.sellPrice(gold: 1);
        Item.wornArmor = true;
        Item.width = 20;
        Item.height = 10;
        Item.rare = ItemRarityID.Cyan;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<MutantDevBody>() && legs.type == ModContent.ItemType<MutantDevLegs>();
}

[AutoloadEquip(EquipType.Body)]
public class MutantDevBody : BaseItem {
    public override void SetStaticDefaults() {
        ArmorIDs.Body.Sets.HidesHands[Item.bodySlot] = false;
    }

    public override void SetDefaults() {
        Item.vanity = true;
        Item.value = Item.sellPrice(gold: 1);
        Item.wornArmor = true;
        Item.width = 30;
        Item.height = 22;
        Item.rare = ItemRarityID.Cyan;
    }
}

[AutoloadEquip(EquipType.Legs)]
public class MutantDevLegs : BaseItem {
    public override void SetDefaults() {
        Item.vanity = true;
        Item.value = Item.sellPrice(gold: 1);
        Item.wornArmor = true;
        Item.width = 22;
        Item.height = 18;
        Item.rare = ItemRarityID.Cyan;
    }
}