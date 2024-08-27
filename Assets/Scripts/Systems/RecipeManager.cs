using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UZSG.Crafting;
using UZSG.Data;

namespace UZSG.Systems
{
    public class RecipeManager : MonoBehaviour, IInitializeable
    {
        bool _isInitialized;
        public bool IsInitialized => _isInitialized;
        /// <summary>
        /// Keys is Recipe Id, Value is RecipeData.
        /// </summary>
        Dictionary<string, RecipeData> _recipeDict = new();
        /// <summary>
        /// Represents the collection (all) of Recipes where this Item is used as a material.
        /// Key is ItemData Id, Value is collection of Recipe Ids.
        /// </summary>
        Dictionary<string, HashSet<string>> _materialIndex = new();
        [SerializeField] AssetLabelReference assetLabelReference;

        internal void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            var startTime = Time.time;
            Game.Console.Log("Reading data: Recipes...");

            var recipes = Resources.LoadAll<RecipeData>("Data/Recipes");

            foreach (var recipe in recipes)
            {
                _recipeDict[recipe.Id] = recipe;
            }
        }

        public RecipeData GetRecipeData(string id)
        {
            if (_recipeDict.ContainsKey(id))
            {
                return _recipeDict[id];
            }

            Game.Console?.Log("Invalid recipe id");
            return null;
        }

        public bool TryGetRecipeData(string id, out RecipeData recipeData)
        {
            if (_recipeDict.ContainsKey(id))
            {
                recipeData = _recipeDict[id];
                return true;
            }

            Game.Console?.Log("Invalid recipe id");
            recipeData = null;
            return false;
        }
    }
}
