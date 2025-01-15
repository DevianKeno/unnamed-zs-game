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
        /// <summary>
        /// <c>string</c> is Id
        /// </summary>
        public Dictionary<string, ItemData> ItemDataDict => _itemsDict;
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
                ValidateData(item);
                _itemsDict[item.Id] = item;
            }
        }

        void ValidateData(ItemData item)
        {
            if (item.StackSize <= 0) item.StackSize = 1;
        }

        /// <summary>
        /// Loads and caches the model.
        /// </summary>
        public bool LoadItemModel(string itemId)
        {
            if (_itemsDict.ContainsKey(itemId))
            {
                var itemData = _itemsDict[itemId];

                if (itemData.EntityModel != null)
                {
                    /// Load model
                    Addressables.LoadAssetAsync<GameObject>(itemData.EntityModel).Completed += (a) =>
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
                    Game.Console.Warn($"There is no Addressable Asset assigned to item {itemId}.");
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
        
        public bool TryGetData(string id, out ItemData itemData)
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
