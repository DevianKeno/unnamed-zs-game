using System;
using System.Collections.Generic;

namespace UZSG
{
    [Serializable]
    public class LocalizationJson
    {
        /// These are json entries, hence their casing
        public Dictionary<string, AttributeEntry> attribute = new();
        public Dictionary<string, EntityEntry> entity = new();
        public Dictionary<string, ItemEntry> item = new();
        public Dictionary<string, ObjectEntry> @object = new();
        public Dictionary<string, RecipeEntry> recipe = new();
        public Dictionary<string, SettingsEntry> setting = new();
        public Dictionary<string, StatusEffectEntry> status_effect = new();
        public Dictionary<string, string> translatable = new();

        public class AttributeEntry
        {
            public string name = string.Empty;
            public string description = string.Empty;
        }

        public class EntityEntry
        {
            public string name = string.Empty;

            public string ToKey()
            {
                return $"{name}";
            }
        }

        public class ItemEntry
        {
            public string name = string.Empty;
            public string description = string.Empty;
            public string source = string.Empty;
        }

        public class ObjectEntry
        {
            public string name = string.Empty;
            public string description = string.Empty;
        }

        public class RecipeEntry
        {
            public string name = string.Empty;
            public string description = string.Empty;
        }
        
        public class SettingsEntry
        {
            public string name = string.Empty;
            public string description = string.Empty;
        }

        public class StatusEffectEntry
        {
            public string name = string.Empty;
            public string description = string.Empty;
        }
    }
}