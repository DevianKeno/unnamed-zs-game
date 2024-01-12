using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UZSG.Items;

namespace UZSG.Systems
{
    public class ItemManager : MonoBehaviour, IInitializable
    {
        bool _isInitialized;
        public bool IsInitialized => _isInitialized;
        Dictionary<string, ItemData> _itemList = new();
        Dictionary<string, GameObject> _cachedModels = new();
        [SerializeField] AssetLabelReference assetLabelReference;

        public event EventHandler<string> OnDoneLoadModel;

        internal void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            
            var startTime = Time.time;
            Game.Console.LogDebug("Initializing item database...");

            Addressables.LoadAssetsAsync<ItemData>(assetLabelReference, (a) =>
            {
                Game.Console?.LogDebug($"Loading data for item {a.Id}");
                _itemList[a.Id] = a;
            });
        }

        /// <summary>
        /// Loads and caches the model.
        /// </summary>
        public bool LoadModel(string id)
        {
            if (_itemList.ContainsKey(id))
            {
                var itemData = _itemList[id];

                if (itemData.AssetReference != null)
                {
                    // Load model
                    Addressables.LoadAssetAsync<GameObject>(itemData.AssetReference).Completed += (a) =>
                    {
                        if (a.Status == AsyncOperationStatus.Succeeded)
                        {
                            _cachedModels[id] = a.Result;
                            OnDoneLoadModel?.Invoke(this, itemData.Id);
                        }
                    };
                    
                    return true;
                } else
                {
                    Game.Console.LogWarning($"There is no asset assigned to item {id}.");
                    return false;
                }
            } else
            {
                Game.Console.Log($"Failed to load item id {id} as it does not exist.");
                return false;
            }            
        }

        /// <summary>
        /// Creates an Item object.
        /// </summary>
        public Item CreateItem(string id, int amount = 1)
        {
            if (_itemList.ContainsKey(id))
            {
                return new Item(_itemList[id], amount);
            }
            
            Game.Console?.Log("Invalid item id");
            return Item.None;
        }

        public ItemData GetItemData(string id)
        {
            if (_itemList.ContainsKey(id))
            {
                return _itemList[id];
            }
            
            Game.Console?.Log("Invalid item id");
            return null;
        }
        
        public bool TryGetItemData(string id, out ItemData itemData)
        {
            if (_itemList.ContainsKey(id))
            {
                itemData = _itemList[id];
                return true;
            }
            
            Game.Console?.Log("Invalid item id");
            itemData = null;
            return false;
        }
    }
}
