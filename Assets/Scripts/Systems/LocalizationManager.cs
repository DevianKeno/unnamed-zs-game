using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

using UZSG.Data;

namespace UZSG
{
    public enum LocaleKeys {
        EN_US, JA_JP, 
    }

    public class LocalizationManager : MonoBehaviour
    {
        const string MISSING_KEY = "Missing_Locale_Key";

        Dictionary<string, string> translationKeys = new();

        internal void Initialize()
        {
            /// once we replace all magic strings with these, there's no going back :P
        }
        
        public void SetLocalization(LocaleKeys localeKey)
        {
            LoadLocalizationFile(localeKey.ToString().Trim().ToLower());
        }

        public string GetKey(string key)
        {
            return MISSING_KEY;
        }

        void LoadLocalizationFile(string localeString)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, $"Localization/{localeString}.json");
            if (File.Exists(filePath))
            {
                string contents = File.ReadAllText(filePath);
                var locale = JsonConvert.DeserializeObject<LocalizationJson>(contents);
                InitializeLocale(locale);
            }
            else
            {
                Debug.LogError($"Localization file not found: {filePath}");
            }
        }

        void InitializeLocale(LocalizationJson locale)
        {
            translationKeys.Clear();

            /// Attributes
            foreach (var kv in locale.attribute)
            {
                LocalizationJson.AttributeEntry attr = kv.Value;
                
                translationKeys[$"attribute.{kv.Key}.name"] = attr.name;
                translationKeys[$"attribute.{kv.Key}.description"] = attr.description;
            }

            /// Entities
            foreach (var kv in locale.entity)
            {
                LocalizationJson.EntityEntry attr = kv.Value;
                
                translationKeys[$"entity.{kv.Key}.name"] = attr.name;
            }

            /// Items
            foreach (var kv in locale.item)
            {
                LocalizationJson.ItemEntry item = kv.Value;

                translationKeys[$"item.{kv.Key}.name"] = item.name;
                translationKeys[$"item.{kv.Key}.description"] = item.description;
                translationKeys[$"item.{kv.Key}.source"] = item.source;
            }

            /// Objects
            foreach (var kv in locale.@object)
            {
                LocalizationJson.ObjectEntry obj = kv.Value;

                translationKeys[$"object.{kv.Key}.name"] = obj.name;
                translationKeys[$"object.{kv.Key}.description"] = obj.description;
            }

            /// Recipes
            foreach (var kv in locale.recipe)
            {
                LocalizationJson.RecipeEntry recipe = kv.Value;

                translationKeys[$"recipe.{kv.Key}.name"] = recipe.name;
                translationKeys[$"recipe.{kv.Key}.description"] = recipe.description;
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Create EN_US Locale")]
        public void CreateDefaults()
        {
            Debug.Log("Creating default locale (en_us), this may take a while...");

            var localeKey = LocaleKeys.EN_US.ToString().ToLower();
            var newLocale = new LocalizationJson(); 

            string[] categories = { "Attributes", "Entities", "Items", "Objects", "Recipes" };
            string[] dataTypes = { "AttributeData", "EntityData", "ItemData", "ObjectData", "RecipeData" };

            for (int i = 0; i < categories.Length; i++)
            {
                string category = categories[i];
                string dataType = dataTypes[i];
            
                Debug.Log($"Started writing {category} localization...");
                string[] guids = AssetDatabase.FindAssets($"t:{dataType}", new[] { $"Assets/Resources/Data/{category}" });

                foreach (var guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var assetData = AssetDatabase.LoadAssetAtPath<BaseData>(assetPath);

                    if (assetData == null)
                    {
                        Debug.LogError($"Failed to load {category} asset at '{assetPath}'");
                        continue;
                    }

                    Debug.Assert(!string.IsNullOrEmpty(assetData.Id), $"{category} '{assetData.name}' has empty Id!");
                    
                    switch (category)
                    {
                        case "Attributes":
                        {
                            var attrData = (AttributeData) assetData;
                            Debug.Assert(!string.IsNullOrEmpty(attrData.Description), $"{category} '{attrData.name}' has empty Description!");
                            newLocale.attribute[assetData.Id] = new LocalizationJson.AttributeEntry()
                            {
                                name = attrData.DisplayName,
                                description = attrData.Description,
                            };
                            break;
                        }
                        case "Entities":
                        {
                            var ettyData = (EntityData) assetData;
                            newLocale.entity[ettyData.Id] = new LocalizationJson.EntityEntry()
                            {
                                name = ettyData.DisplayName,
                            };
                            break;
                        }
                        case "Items":
                        {
                            var itemData = (ItemData) assetData;
                            Debug.Assert(!string.IsNullOrEmpty(itemData.Description), $"{category} '{itemData.name}' has empty Description!");
                            Debug.Assert(!string.IsNullOrEmpty(itemData.SourceDescription), $"{category} '{itemData.name}' has empty SourceDescription!");
                            newLocale.item[assetData.Id] = new LocalizationJson.ItemEntry()
                            {
                                name = itemData.DisplayName,
                                description = itemData.Description,
                                source = itemData.SourceDescription,
                            };
                            break;
                        }
                        case "Objects":
                        {
                            var objData = (ObjectData) assetData;
                            Debug.Assert(!string.IsNullOrEmpty(objData.Description), $"{category} '{objData.name}' has empty Description!");
                            newLocale.@object[assetData.Id] = new LocalizationJson.ObjectEntry()
                            {
                                name = objData.DisplayName,
                                description = objData.Description,
                            };
                            break;
                        }
                        case "Recipes":
                        {
                            var recipeData = (RecipeData) assetData;
                            Debug.Assert(!string.IsNullOrEmpty(recipeData.Description), $"{category} '{recipeData.name}' has empty Description!");
                            newLocale.recipe[recipeData.Id] = new LocalizationJson.RecipeEntry()
                            {
                                name = recipeData.DisplayName,
                                description = recipeData.Description,
                            };
                            break;
                        }
                    }
                }
                Debug.Log($"Finished writing {category} localization.");
            }

            /// Sort alphabetically
            var sortedLocale = new LocalizationJson
            {
                attribute = newLocale.attribute.OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value),
                entity = newLocale.entity.OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value),
                item = newLocale.item.OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value),
                @object = newLocale.@object.OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value),
                recipe = newLocale.recipe.OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value),
            };

            /// Save json
            string contents = JsonConvert.SerializeObject(newLocale, Formatting.Indented);
            string localePath = Path.Combine(Application.streamingAssetsPath, $"Locale/{localeKey}.json");

            if (File.Exists(localePath))
            {
                Debug.LogWarning($"Locale file already exists: {localePath}. Overwriting...");
            }
            
            File.WriteAllText(localePath, contents);
            Debug.Log($"Finished creation of default locale (en_us). Saved at '{localePath}'");
        }
#endif
    }
}
