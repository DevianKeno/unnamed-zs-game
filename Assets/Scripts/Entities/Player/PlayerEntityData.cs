using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using Newtonsoft.Json;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Saves;
using UZSG.Attributes;
using System.Linq;

namespace UZSG.Entities
{
    /// <summary>
    /// EntityData class made specifically for Player entities.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Player Entity Data", menuName = "UZSG/Entity/Player Entity Data")]
    public class PlayerEntityData : EntityData
    {
        string defaultsPath = Application.dataPath + "/Resources/Defaults/Entities/";

        [Header("Attributes")]
        public List<Attributes.Attribute> Attributes;
        public List<RecipeData> KnownRecipes;

        public PlayerSaveData GetDefaultsJson()
        {
            var filepath = defaultsPath + $"{Id}_defaults.json";

            if (!File.Exists(filepath))
            {
                Game.Console.LogWarning($"'player_defaults' not found, creating new one...");
                WriteDefaultsJson();
            }
            
            var defaultsJson = File.ReadAllText(filepath);
            return JsonConvert.DeserializeObject<PlayerSaveData>(defaultsJson);
        }

#if UNITY_EDITOR
        public void ReadDefaultsJson()
        {
            var filepath = defaultsPath + $"{Id}_defaults.json";
            var defaultsJson = File.ReadAllText(filepath);
            var defaults = JsonConvert.DeserializeObject<PlayerSaveData>(defaultsJson);

            /// Inventory
            
            /// Attributes
            Attributes.Clear();
            foreach (var attrSave in defaults.Attributes)
            {
                var attrData = Resources.Load<AttributeData>($"Data/Attributes/{attrSave.Id}");
                if (attrData != null)
                {
                    var newAttr = new Attributes.Attribute(attrData);
                    newAttr.ReadSaveJson(attrSave);
                    Attributes.Add(newAttr);
                }
                else
                {
                    Debug.LogWarning($"Invalid Attribute Id '{attrSave.Id}'. It does not exist.");
                }
            }

            /// Recipes
            KnownRecipes.Clear();
            foreach (var id in defaults.KnownRecipes)
            {
                var recipeData = Resources.Load<RecipeData>($"Data/Recipes/{id}");
                if (recipeData != null)
                {
                    KnownRecipes.Add(recipeData);
                }
                else
                {
                    Debug.LogWarning($"Invalid Recipe Id '{id}'. It does not exist.");
                }
            }
        }
#endif

        public void WriteDefaultsJson()
        {
            var saveData = new PlayerSaveData();
            /// Inventory
            
            /// Attributes
            var ac = new AttributeCollection();
            ac.AddList(Attributes);
            saveData.Attributes = ac.WriteSaveJson();

            /// Recipes
            saveData.KnownRecipes.AddRange(KnownRecipes.Select(recipe => recipe.Id));

            var filepath = defaultsPath + $"{Id}_defaults.json";
            if (!Directory.Exists(defaultsPath)) Directory.CreateDirectory(defaultsPath);///
            var defaultsJson = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            File.WriteAllText(filepath, defaultsJson);
        }
    } 
}