using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

using Newtonsoft.Json;

using UZSG.Data;

namespace UZSG
{
    public class LocalizationManager : MonoBehaviour
    {
        const string DEFAULT_LOCALE = "en_us";
        const string MISSING_KEY = "Missing_Locale_Key";

        [SerializeField] LocalizationData currentLocale;
        public LocalizationData CurrentLocale => currentLocale;

        List<LocalizationData> availableLocales = new();
        /// <summary>
        /// List of available locales. [Read Only]
        /// </summary>
        public List<LocalizationData> AvailableLocales => new(availableLocales);

        string _previousLocaleKey;
        /// <summary>
        /// Key is key, Value is the localized string.
        /// </summary>
        Dictionary<string, string> translationKeys = new();
        ConcurrentDictionary<string, string> translationKeysConcurrent = new(); /// UNUSED

        /// <summary>
        /// Raised once upon successfully changing locales.
        /// </summary>
        public event Action OnLocaleChanged;

        internal IEnumerator<float> _Initialize()
        {
            foreach (var localization in Resources.LoadAll<LocalizationData>("Locale"))
            {
                availableLocales.Add(localization);
            }
            
            var task = SetLocalizationAsync(currentLocale.LocaleKey, force: true);
            while (false == task.IsCompleted)
            {
                yield return 0f;
            }
        }


        #region Public
        
        /// <summary>
        /// Get a localized version of a string given its key.
        /// </summary>
        public string Translatable(string key)
        {
            if (translationKeys.TryGetValue(key, out var localized))
            {
                return localized;
            }
            else
            {
                return $"{MISSING_KEY}:{currentLocale.LocaleKey}:{key}";
            }
        }
        
        /// <summary>
        /// Get a formatted localized version of a string given its key.
        /// </summary>
        public string TranslatableFormat(string format, params object[] args)
        {
            if (translationKeys.TryGetValue(format, out var localizedFormat))
            {
                return string.Format(localizedFormat, args);
            }
            else
            {
                return $"{MISSING_KEY}:{currentLocale.LocaleKey}:{format}";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localeKey">localeKey formatted as "en_us", "ja_jp", etc.</param>
        /// <returns><c>bool</c>: whether if successfully changed locales.</returns>
        public async Task<bool> SetLocalization(string localeKey)
        {
            await SetLocalizationAsync(localeKey);
            return _previousLocaleKey != currentLocale.LocaleKey;
        }

        /// <summary>
        /// Gets the index of a locale key as its stored in the list of available locales.
        /// </summary>
        /// <param name="localeKey">locale key formatted as "en_us", "ja_jp", etc.</param>
        /// <returns>the index of the locale, -1 if not present</returns>
        public int GetIndexOf(string localeKey)
        {
            return availableLocales.FindIndex((l) => l.LocaleKey.Equals(localeKey, StringComparison.OrdinalIgnoreCase));
        }

        #endregion
        

        /// <summary>
        /// Sets the current localization.
        /// The parameter localeKey is formatted as "en_us", "ja_jp", etc.
        /// </summary>
        /// <param name="localeKey"></param>
        async Task SetLocalizationAsync(string localeKey, bool force = false)
        {
            if (currentLocale.LocaleKey.Equals(localeKey, StringComparison.OrdinalIgnoreCase) && !force)
            {
                return;
            }

            _previousLocaleKey = currentLocale.LocaleKey;
            string filepath = Path.Combine(Application.streamingAssetsPath, $"Locale/{localeKey}.json");
            if (false == File.Exists(filepath))
            {
                Game.Console.LogError($"Unable to change localization.", true);
                Game.Console.LogWarn($"There exists no localization file for locale '{localeKey}'!");
                return;
            }

            try
            {
                string contents = await File.ReadAllTextAsync(filepath);
                var json = JsonConvert.DeserializeObject<LocalizationJson>(contents);
            
                InitializeLocaleJson(json);
                currentLocale = availableLocales.Find((l) => l.LocaleKey == localeKey);
                PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() => OnLocaleChanged?.Invoke());
            }
            catch (Exception ex)
            {
                Game.Console.LogError($"Unable to change localization.", true);
                Game.Console.LogError($"Encountered an error when parsing localization file at '{filepath}' for locale '{localeKey}'.", true);
                Debug.LogException(ex);
                return;
            }
        }

        void InitializeLocaleJson(LocalizationJson locale)
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

            /// Settings
            /// - Qualities
            foreach (var q in Enum.GetValues(typeof(SettingsQualityFlags)))
            {
                translationKeys[$"setting.{q.ToString().ToLower()}"] = ((Enum) q).ToReadable();
            }
            /// - Setting Entries
            foreach (var kv in locale.setting)
            {
                LocalizationJson.SettingsEntry setting = kv.Value;

                translationKeys[$"setting.{kv.Key}.name"] = setting.name;
                translationKeys[$"setting.{kv.Key}.description"] = setting.description;
            }

            /// Status Effects
            foreach (var kv in locale.status_effect)
            {
                LocalizationJson.StatusEffectEntry statusEffect = kv.Value;

                translationKeys[$"status_effect.{kv.Key}.name"] = statusEffect.name;
                translationKeys[$"status_effect.{kv.Key}.description"] = statusEffect.description;
            }

            /// [Other] Translatables
            foreach (var kv in locale.translatable)
            {
                translationKeys[$"{kv.Key}"] = kv.Value;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Create the default en_us locale json file.
        /// </summary>
        [ContextMenu("Create EN_US Locale")]
        public void CreateDefaults()
        {
            Debug.Log("Creating default locale (en_us), this may take a while...");

            var localeKey = "en_us";
            var newLocale = new LocalizationJson(); 

            string[] categories = { "Attributes", "Entities", "Items", "Objects", "Recipes", "Settings", "Translatable"};
            string[] dataTypes = { "AttributeData", "EntityData", "ItemData", "ObjectData", "RecipeData", "SettingsEntryData", "TranslatableKey"};

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
                        case "Settings":
                        {
                            var settingData = (SettingsEntryData) assetData;
                            Debug.Assert(!string.IsNullOrEmpty(settingData.description), $"{category} '{settingData.name}' has empty Description!");
                            newLocale.setting[settingData.Id] = new LocalizationJson.SettingsEntry()
                            {
                                name = settingData.displayName,
                                description = settingData.description,
                            };
                            break;
                        }
                        case "StatusEffect":
                        {
                            var settingData = (StatusEffectData) assetData;
                            Debug.Assert(!string.IsNullOrEmpty(settingData.Description), $"{category} '{settingData.name}' has empty Description!");
                            newLocale.status_effect[settingData.Id] = new LocalizationJson.StatusEffectEntry()
                            {
                                name = settingData.DisplayName,
                                description = settingData.Description,
                            };
                            break;
                        }
                        case "Translatable":
                        {
                            var translatable = (TranslatableKey) assetData;
                            Debug.Assert(!string.IsNullOrEmpty(translatable.DefaultText), $"{category} '{translatable.name}' has empty default text!");
                            newLocale.translatable[translatable.Key] = translatable.DefaultText;
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
                setting = newLocale.setting.OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value),
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
