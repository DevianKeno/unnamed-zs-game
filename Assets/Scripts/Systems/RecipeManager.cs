using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UZSG.Crafting;

namespace UZSG.Systems
{
    public class RecipeManager : MonoBehaviour, IInitializable
    {
        bool _isInitialized;
        public bool IsInitialized => _isInitialized;
        Dictionary<string, RecipeData> _recipeList = new();
        [SerializeField] AssetLabelReference assetLabelReference;
        
        internal void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            
            var startTime = Time.time;
            Game.Console.LogDebug("Initializing recipes...");

            Addressables.LoadAssetsAsync<RecipeData>(assetLabelReference, (a) =>
            {                
                Game.Console?.LogDebug($"Loading data for recipe {a.Id}");
                _recipeList[a.Id] = a;
            });
        }
    }
}
