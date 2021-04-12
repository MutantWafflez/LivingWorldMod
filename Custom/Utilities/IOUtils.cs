using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LivingWorldMod.Custom.Interfaces;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Custom.Utilities {

    /// <summary>
    /// Utility class that handles methods that pertain to saving/loading or anything generally
    /// related to I/O.
    /// </summary>
    public static class IOUtils {

        public static void SaveDictionary<TKey, TValue>(TagCompound parent, string key, Dictionary<TKey, TValue> dict, Func<TKey, object> keyMapper = null, Func<TValue, object> valueMapper = null) {
            parent.Add(key, dict.Select(pair => new TagCompound
            {
                {"key", keyMapper == null ? pair.Key : keyMapper.Invoke(pair.Key)},
                {"value", valueMapper == null ? pair.Value : valueMapper.Invoke(pair.Value)}
            })
                .ToList()
            );
        }

        public static Dictionary<TKey, TValue> LoadDictionary<TKey, TValue>(TagCompound parent,
            string key) {
            return LoadDictionary<TKey, TValue, TKey>(parent, key, val => val);
        }

        public static Dictionary<TKey, TValue> LoadDictionary<TKey, TValue, TStartKey>(TagCompound parent,
            string key, Func<TStartKey, TKey> keyMapper) {
            return LoadDictionary<TKey, TValue, TStartKey, TValue>(parent, key, keyMapper, val => val);
        }

        public static Dictionary<TKey, TValue> LoadDictionary<TKey, TValue, TStartKey, TStartValue>(TagCompound parent, string key, Func<TStartKey, TKey> keyMapper, Func<TStartValue, TValue> valueMapper) {
            // fetch entry list
            IList<TagCompound> entries = parent.GetList<TagCompound>(key);
            // create and populate dictionary

            T Fetch<T, TStart>(TagCompound entry, Func<TStart, T> converter, string entryKey) {
                if (converter == null)
                    return entry.Get<T>(entryKey);
                return converter.Invoke(entry.Get<TStart>(entryKey));
            }

            return entries.ToDictionary(
                entry => Fetch(entry, keyMapper, "key"),
                entry => Fetch(entry, valueMapper, "value")
            );
        }

        public static void WriteList<T>(BinaryWriter writer, IList<T> list) where T : IBinarySerializable {
            WriteList(writer, list, list.Count);
        }

        public static void WriteList<T>(BinaryWriter writer, IEnumerable<T> list, int length) where T : IBinarySerializable {
            WriteList(writer, list, length, (w, obj) => obj.Write(w));
        }

        public static void WriteList<T>(BinaryWriter writer, IList<T> list, Action<BinaryWriter, T> writeFunc) {
            WriteList(writer, list, list.Count, writeFunc);
        }

        public static void WriteList<T>(BinaryWriter writer, IEnumerable<T> list, int length, Action<BinaryWriter, T> writeFunc) {
            writer.Write(length);
            foreach (T obj in list)
                writeFunc(writer, obj);
        }

        public static List<T> ReadList<T>(BinaryReader reader) where T : IBinarySerializable, new() {
            return ReadList(reader, FetchParser<T>());
        }

        public static List<T> ReadList<T>(BinaryReader reader, Func<BinaryReader, T> parser) {
            // fetch length
            int length = reader.ReadInt32();
            // create and populate list
            List<T> list = new List<T>(length);
            for (int i = 0; i < length; i++)
                list.Add(parser.Invoke(reader));
            return list;
        }

        public static void WriteDictionary<TKey, TValue>(BinaryWriter writer, Dictionary<TKey, TValue> dict)
            where TKey : IBinarySerializable
            where TValue : IBinarySerializable {
            WriteDictionary(writer, dict, (w, key) => key.Write(w), (w, value) => value.Write(w));
        }

        public static void WriteDictionary<TKey, TValue>(BinaryWriter writer, Dictionary<TKey, TValue> dict, Action<BinaryWriter, TKey> keyWriter)
            where TValue : IBinarySerializable {
            WriteDictionary(writer, dict, keyWriter, (w, value) => value.Write(w));
        }

        public static void WriteDictionary<TKey, TValue>(BinaryWriter writer, Dictionary<TKey, TValue> dict, Action<BinaryWriter, TValue> valueWriter)
            where TKey : IBinarySerializable {
            WriteDictionary(writer, dict, (w, key) => key.Write(w), valueWriter);
        }

        public static void WriteDictionary<TKey, TValue>(BinaryWriter writer, Dictionary<TKey, TValue> dict, Action<BinaryWriter, TKey> keyWriter, Action<BinaryWriter, TValue> valueWriter) {
            WriteList(writer, dict.Select(pair => pair).ToList(), (w, pair) => {
                keyWriter.Invoke(w, pair.Key);
                valueWriter.Invoke(w, pair.Value);
            });
        }

        public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(BinaryReader reader)
            where TKey : IBinarySerializable, new()
            where TValue : IBinarySerializable, new() {
            return ReadDictionary(reader, FetchParser<TKey>(), FetchParser<TValue>());
        }

        public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(BinaryReader reader, Func<BinaryReader, TKey> keyReader)
            where TValue : IBinarySerializable, new() {
            return ReadDictionary(reader, keyReader, FetchParser<TValue>());
        }

        public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(BinaryReader reader, Func<BinaryReader, TValue> valueReader)
            where TKey : IBinarySerializable, new() {
            return ReadDictionary(reader, FetchParser<TKey>(), valueReader);
        }

        public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(BinaryReader reader, Func<BinaryReader, TKey> keyReader, Func<BinaryReader, TValue> valueReader) {
            int length = reader.ReadInt32();
            Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>(length);
            for (int i = 0; i < length; i++)
                dict.Add(keyReader.Invoke(reader), valueReader.Invoke(reader));
            return dict;
        }

        private static Func<BinaryReader, T> FetchParser<T>() where T : IBinarySerializable, new() {
            // Type type = typeof(T); FieldInfo field = type.GetField("READER"); return
            // (Func<BinaryReader, T>) field.GetValue(null);
            return reader => {
                T obj = new T();
                obj.Read(reader);
                return obj;
            };
        }
    }
}