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
        public List<RecipeData> KnownRecipes;

        public override PlayerSaveData GetDefaultsJson<PlayerSaveData>()
        {
            var filepath = defaultsPath + $"{Id}_defaults.json";

            if (!File.Exists(filepath))
            {
                Game.Console.LogWarning($"'{Id}_defaults' not found, creating new one...");
                WriteDefaultsJson();
            }
            
            var defaultsJson = File.ReadAllText(filepath);
            return JsonConvert.DeserializeObject<PlayerSaveData>(defaultsJson);
        }

#if UNITY_EDITOR
        public override void ReadDefaultsJson()
        {
            base.ReadDefaultsJson();
            
            var filepath = defaultsPath + $"{Id}_defaults.json";
            var defaultsJson = File.ReadAllText(filepath);
            var defaults = JsonConvert.DeserializeObject<PlayerSaveData>(defaultsJson);

            /// Inventory
            /// 

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
        
        public override void WriteDefaultsJson()
        {
            var saveData = new PlayerSaveData();
            
            /// Attributes
            var ac = new AttributeCollection();
            ac.AddList(Attributes);
            saveData.Attributes = ac.WriteSaveJson();

            /// Inventory
            /// 
            
            /// Recipes
            saveData.KnownRecipes.AddRange(KnownRecipes.Select(recipe => recipe.Id));

            WriteToFile(saveData);
        }
#endif
    }
}