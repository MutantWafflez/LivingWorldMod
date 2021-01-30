using LivingWorldMod.ID;
using LivingWorldMod.NPCs.Villagers;
using LivingWorldMod.NPCs.Villagers.Quest;
using LivingWorldMod.Tiles.Interactables;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using static Terraria.ModLoader.ModContent;

namespace LivingWorldMod.Utilities
{
    internal static class LWMUtils
    {
        public static void ConditionalStringAdd<T>(this WeightedRandom<T> weightedRandom, T curType, bool boolean, double weight = 1)
        {
            if (boolean)
            {
                weightedRandom.Add(curType, weight);
            }
        }
        
        #region IO Utils

        public static void SaveDictionary<TKey, TValue>(TagCompound parent, string key, Dictionary<TKey, TValue> dict, Func<TKey, object> keyMapper = null, Func<TValue, object> valueMapper = null)
        {
            parent.Add(key, dict.Select(pair => new TagCompound
            {
                {"key", keyMapper == null ? pair.Key : keyMapper.Invoke(pair.Key)},
                {"value", valueMapper == null ? pair.Value : valueMapper.Invoke(pair.Value)}
            })
                .ToList()
            );
        }

        public static Dictionary<TKey, TValue> LoadDictionary<TKey, TValue>(TagCompound parent,
            string key)
        {
            return LoadDictionary<TKey, TValue, TKey>(parent, key, val => val);
        }
        public static Dictionary<TKey, TValue> LoadDictionary<TKey, TValue, TStartKey>(TagCompound parent,
            string key, Func<TStartKey, TKey> keyMapper)
        {
            return LoadDictionary<TKey, TValue, TStartKey, TValue>(parent, key, keyMapper, val => val);
        }
        public static Dictionary<TKey, TValue> LoadDictionary<TKey, TValue, TStartKey, TStartValue>(TagCompound parent, string key, Func<TStartKey, TKey> keyMapper, Func<TStartValue, TValue> valueMapper)
        {
            // fetch entry list
            IList<TagCompound> entries = parent.GetList<TagCompound>(key);
            // create and populate dictionary

            T Fetch<T, TStart>(TagCompound entry, Func<TStart, T> converter, string entryKey)
            {
                if (converter == null)
                    return entry.Get<T>(entryKey);
                return converter.Invoke(entry.Get<TStart>(entryKey));
            } 
            
            return entries.ToDictionary(
                entry => Fetch(entry, keyMapper, "key"),
                entry => Fetch(entry, valueMapper, "value")
            );
        }

        private static Func<BinaryReader, T> FetchParser<T>() where T : BinarySerializable, new()
        {
            // Type type = typeof(T);
            // FieldInfo field = type.GetField("READER");
            // return (Func<BinaryReader, T>) field.GetValue(null);
            return reader =>
            {
                T obj = new T();
                obj.Read(reader);
                return obj;
            };
        }

        public static void WriteList<T>(BinaryWriter writer, IList<T> list) where T : BinarySerializable
        {
            WriteList(writer, list, list.Count);
        }
        
        public static void WriteList<T>(BinaryWriter writer, IEnumerable<T> list, int length) where T : BinarySerializable
        {
            WriteList(writer, list, length, (w, obj) => obj.Write(w));
        }
        
        public static void WriteList<T>(BinaryWriter writer, IList<T> list, Action<BinaryWriter, T> writeFunc)
        {
            WriteList(writer, list, list.Count, writeFunc);
        }
        
        public static void WriteList<T>(BinaryWriter writer, IEnumerable<T> list, int length, Action<BinaryWriter, T> writeFunc)
        {
            writer.Write(length);
            foreach (T obj in list)
                writeFunc(writer, obj);
        }
        
        public static List<T> ReadList<T>(BinaryReader reader) where T : BinarySerializable, new()
        {
            return ReadList(reader, FetchParser<T>());
        }

        public static List<T> ReadList<T>(BinaryReader reader, Func<BinaryReader, T> parser)
        {
            // fetch length
            int length = reader.ReadInt32();
            // create and populate list
            List<T> list = new List<T>(length);
            for (int i = 0; i < length; i++)
                list.Add(parser.Invoke(reader));
            return list;
        }

        public static void WriteDictionary<TKey, TValue>(BinaryWriter writer, Dictionary<TKey, TValue> dict)
            where TKey : BinarySerializable
            where TValue : BinarySerializable
        {
            WriteDictionary(writer, dict, (w, key) => key.Write(w), (w, value) => value.Write(w));
        }
        
        public static void WriteDictionary<TKey, TValue>(BinaryWriter writer, Dictionary<TKey, TValue> dict, Action<BinaryWriter, TKey> keyWriter)
            where TValue : BinarySerializable
        {
            WriteDictionary(writer, dict, keyWriter, (w, value) => value.Write(w));
        }
        
        public static void WriteDictionary<TKey, TValue>(BinaryWriter writer, Dictionary<TKey, TValue> dict, Action<BinaryWriter, TValue> valueWriter)
            where TKey : BinarySerializable
        {
            WriteDictionary(writer, dict, (w, key) => key.Write(w), valueWriter);
        }
        
        public static void WriteDictionary<TKey, TValue>(BinaryWriter writer, Dictionary<TKey, TValue> dict, Action<BinaryWriter, TKey> keyWriter, Action<BinaryWriter, TValue> valueWriter)
        {
            WriteList(writer, dict.Select(pair => pair).ToList(), (w, pair) =>
            {
                keyWriter.Invoke(w, pair.Key);
                valueWriter.Invoke(w, pair.Value);
            });
        }

        public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(BinaryReader reader)
            where TKey : BinarySerializable, new()
            where TValue : BinarySerializable, new()
        {
            return ReadDictionary(reader, FetchParser<TKey>(), FetchParser<TValue>());
        }
        
        public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(BinaryReader reader, Func<BinaryReader, TKey> keyReader)
            where TValue : BinarySerializable, new()
        {
            return ReadDictionary(reader, keyReader, FetchParser<TValue>());
        }
        
        public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(BinaryReader reader, Func<BinaryReader, TValue> valueReader)
            where TKey : BinarySerializable, new()
        {
            return ReadDictionary(reader, FetchParser<TKey>(), valueReader);
        }
        
        public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(BinaryReader reader, Func<BinaryReader, TKey> keyReader, Func<BinaryReader, TValue> valueReader)
        {
            int length = reader.ReadInt32();
            Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>(length);
            for (int i = 0; i < length; i++)
                dict.Add(keyReader.Invoke(reader), valueReader.Invoke(reader));
            return dict;
        }
        
        #endregion IO Utils

        /// <summary>
        /// Returns whether or not a given NPC is a type of Villager.
        /// </summary>
        /// <param name="npc">The npc to check.</param>
        public static bool IsTypeOfVillager(this NPC npc)
        {
            if (npc.modNPC?.GetType().BaseType == typeof(Villager))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns whether or not a given NPC is a type of Quest Villager.
        /// </summary>
        /// <param name="npc">The npc to check.</param>
        public static bool IsTypeOfQuestVillager(this NPC npc)
        {
            if (npc.modNPC?.GetType().BaseType == typeof(QuestVillager))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Finds the closest NPC to any other NPC.
        /// Optionally can find the nearest NPC of a given type if needed.
        /// </summary>
        public static NPC FindNearestNPC(this NPC npc, int npcType = -1)
        {
            float distance = float.MaxValue;
            NPC selectedNPC = null;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i] != npc && Main.npc[i].Distance(npc.Center) < distance && (Main.npc[i].type == npcType || npcType == -1))
                {
                    selectedNPC = Main.npc[i];
                    distance = Main.npc[i].Distance(npc.Center);
                }
            }
            return selectedNPC;
        }

        /// <summary>
        /// Finds the top left tile coordinate of a multi tile.
        /// </summary>
        /// <param name="i">Horizontal tile coordinates</param>
        /// <param name="j">Vertical tile coordinates</param>
        /// <param name="tileType">Multi tile's tile type.</param>
        /// <returns></returns>
        public static Vector2 FindMultiTileTopLeft(int i, int j, int tileType)
        {
            Vector2 topLeftPos = new Vector2(i, j);
            //Finding top left corner
            for (int k = 0; k < 4; k++)
            {
                for (int l = 0; l < 5; l++)
                {
                    if (Framing.GetTileSafely(i - k, j - l).type == TileType<HarpyShrineTile>())
                        topLeftPos = new Vector2(i - k, j - l);
                    else continue;
                }
            }

            return topLeftPos;
        }

        //----------Extension Methods----------
        public static Tile ToTile(this TileNode tn) => Framing.GetTileSafely(tn.position);

        public static Point16 Add(this Point16 point, int p1x, int p1y) => new Point16(point.X + p1x, point.Y + p1y);

        public static IEnumerable<KeyValuePair<TKey, TValue>> Entries<TKey, TValue>(this Dictionary<TKey, TValue> dict)
        {
            return dict.Select(pair => pair);
        }
    }
}