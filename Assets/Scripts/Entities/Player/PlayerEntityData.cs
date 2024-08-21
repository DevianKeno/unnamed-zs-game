using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using Newtonsoft.Json;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Saves;
using UZSG.Attributes;

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
            ac.AddList(BaseAttributes);
            saveData.Attributes = ac.WriteSaveData();

            /// Inventory
            /// 
            
            /// Recipes
            saveData.KnownRecipes.AddRange(KnownRecipes.Select(recipe => recipe.Id));

            WriteToFile(saveData);
        }
#endif
    }
}