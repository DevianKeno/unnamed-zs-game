using System.Collections.Generic;

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

        public SaveData AddEntry<T>(string key, T value)
        {
            Entries[key] = value;
            return this;
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

        public SaveData[] GetArray(string id)
        {
            if (Entries.TryGetValue(id, out var value) &&
                value is SaveData[] array)
            {
                return array;
            }
            return default;
        }
        
        public static bool FieldIsNull(object obj)
        {
            return obj == null;
        }
    }
}