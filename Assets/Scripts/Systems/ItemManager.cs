using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using UZSG.Data;
using UZSG.Items;

namespace UZSG.Systems
{
    public class ItemManager : MonoBehaviour, IInitializeable
    {
        bool _isInitialized;
        public bool IsInitialized => _isInitialized;
        Dictionary<string, ItemData> _itemsDict = new();
        Dictionary<string, GameObject> _cachedItemModels = new();

        public event EventHandler<string> OnDoneLoadModel;

        internal void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            
            var startTime = Time.time;
            Game.Console.Log("Reading data: Items...");
            var items = Resources.LoadAll<ItemData>("Data/Items");
            foreach (var item in items)
            {
                _itemsDict[item.Id] = item;
            }
        }

        /// <summary>
        /// Loads and caches the model.
        /// </summary>
        public bool LoadItemModel(string itemId)
        {
            if (_itemsDict.ContainsKey(itemId))
            {
                var itemData = _itemsDict[itemId];

                if (itemData.Model != null)
                {
                    /// Load model
                    Addressables.LoadAssetAsync<GameObject>(itemData.Model).Completed += (a) =>
                    {
                        if (a.Status == AsyncOperationStatus.Succeeded)
                        {
                            _cachedItemModels[itemId] = a.Result;
                            OnDoneLoadModel?.Invoke(this, itemData.Id);
                        }
                    };
                    
                    return true;
                }
                else
                {
                    Game.Console.LogWarning($"There is no Addressable Asset assigned to item {itemId}.");
                    return false;
                }
            }
            else
            {
                Game.Console.Log($"Failed to load item id {itemId} as it does not exist.");
                return false;
            }            
        }

        public ItemData GetData(string id)
        {
            if (_itemsDict.ContainsKey(id))
            {
                return _itemsDict[id];
            }

            Game.Console.Log("Invalid item id");
            return null;
        }
        
        public bool TryGetItemData(string id, out ItemData itemData)
        {
            if (_itemsDict.ContainsKey(id))
            {
                itemData = _itemsDict[id];
                return true;
            }
            
            Game.Console.Log("Invalid item id");
            itemData = null;
            return false;
        }
    }
}
