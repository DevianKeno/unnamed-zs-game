using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace UZSG.Saves
{
    public class SaveData
    {
        public string Id { get; set; } = string.Empty;
        public string Type => GetType().Name;
        public Dictionary<string, object> Entries = new();

        public SaveData()
        {
        }

        public SaveData(string id)
        {
            this.Id = id;
        }
        
        public SaveData AddEntry(string key, object value)
        {
            Entries[key] = value;
            return this;
        }

        public SaveData AddBool(string key, bool value)
        {
            Entries[key] = value;
            return this;
        }

        public bool GetBool(string key)
        {
            if (Entries.TryGetValue(key, out var value) &&
                value is bool valueBool)
            {
                return valueBool;
            }
            return default;
        }

        public SaveData AddEntry<T>(string key, T value)
        {
            Entries[key] = value;
            return this;
        }

        public SaveData AddArray<T>(string key, T[] values)
        {
            Entries[key] = values;
            return this;
        }

        public object GetEntry(string id)
        {
            if (Entries.TryGetValue(id, out var value))
            {
                return value;
            }
            return default;
        }

        public T GetEntry<T>(string id)
        {
            if (Entries.TryGetValue(id, out var value) &&
                value is T valueT)
            {
                return valueT;
            }
            return default;
        }

        public T[] GetJArray<T>(string id)
        {
            if (Entries.TryGetValue(id, out var value) &&
                value is JArray jArray)
            {
                return jArray.Select(token => token.Value<T>()).ToArray();
            }
            return default;
        }
        
        public static bool FieldIsNull(object obj)
        {
            return obj == null;
        }
    }
}