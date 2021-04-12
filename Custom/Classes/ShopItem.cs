using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LivingWorldMod.Common.Players;
using LivingWorldMod.Custom.Interfaces;
using Terraria;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Custom.Classes
{
    public class ShopItem : TagSerializable, IBinarySerializable
    {
        public static readonly Func<TagCompound, ShopItem> DESERIALIZER = Load;

        public int stackSize;
        private int customPrice;
        private float minRep;
        private ShopConstraint[] constraints;
        public int ItemID { get; private set; }

        public ShopItem() { }

        public ShopItem(int itemId, int stackSize, int customPrice = -1, float minRep = -1, params ShopConstraint[] constraints)
        {
            this.ItemID = itemId;
            this.stackSize = stackSize;
            this.customPrice = customPrice;
            this.minRep = minRep;
            this.constraints = constraints;
        }

        public static ShopItem Load(TagCompound tag)
        {
            int customPrice = -1;
            if (tag.ContainsKey("price"))
                customPrice = tag.GetInt("price");

            float minRep = -1;
            if (tag.ContainsKey("rep"))
                minRep = tag.GetFloat("rep");

            ShopConstraint[] constraints;
            if (tag.ContainsKey("constraints"))
            {
                IList<int> ids = tag.GetList<int>("constraints");
                constraints = ids.Select(ShopConstraint.Get).ToArray();
            }
            else
                constraints = new ShopConstraint[0];

            return new ShopItem(tag.GetInt("itemid"), tag.GetInt("stack"), customPrice, minRep, constraints);
        }

        public ShopItem Clone()
        {
            return new ShopItem(ItemID, stackSize, customPrice, minRep, constraints);
        }

        public bool CanPurchase(LWMPlayer player, float rep)
        {
            if (minRep >= 0 && rep < minRep)
                return false;

            return constraints.All(constraint => constraint.CanPurchase(player));
        }

        public void Apply(Item item)
        {
            item.SetDefaults(ItemID);
            item.stack = stackSize;
            if (customPrice >= 0)
                item.shopCustomPrice = customPrice;
        }

        public TagCompound SerializeData()
        {
            TagCompound tag = new TagCompound
            {
                {"itemid", ItemID},
                {"stack", stackSize}
            };

            if (customPrice >= 0)
                tag.Add("price", customPrice);
            if (minRep >= 0)
                tag.Add("rep", minRep);
            if (constraints.Length > 0)
                tag.Add("constraints", constraints.Select(c => c.id).ToList());

            return tag;
        }

        public void Write(BinaryWriter writer, byte syncMode = default)
        {
            writer.Write(ItemID);
            writer.Write(stackSize);
            writer.Write(customPrice);
            writer.Write(minRep);
            writer.Write((byte)constraints.Length);
            foreach (ShopConstraint c in constraints)
                writer.Write((byte)c.id);
        }

        public void Read(BinaryReader reader, byte syncMode = default)
        {
            ItemID = reader.ReadInt32();
            stackSize = reader.ReadInt32();
            customPrice = reader.ReadInt32();
            minRep = reader.ReadSingle();
            constraints = new ShopConstraint[reader.ReadByte()];
            for (int i = 0; i < constraints.Length; i++)
                constraints[i] = ShopConstraint.Get(reader.ReadByte());
        }
    }

    public class ShopConstraint
    {
        public readonly int id;
        private static readonly ShopConstraint[] _constraintArray = new ShopConstraint[10];
        private static int counter = 0;

        private readonly Func<LWMPlayer, bool> func;

        private ShopConstraint(Func<LWMPlayer, bool> func)
        {
            this.func = func;
            id = counter++;
            _constraintArray[id] = this;
        }

        public static ShopConstraint Get(int id)
        {
            return _constraintArray[id];
        }

        #region Constants

        // add constraints here

        public static readonly ShopConstraint RAINING = new ShopConstraint(player => Main.raining);

        #endregion Constants

        public bool CanPurchase(LWMPlayer player)
        {
            return func(player);
        }
    }
}