using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UZSG.Items;

namespace UZSG.Systems
{
    public class ItemManager : MonoBehaviour, IInitializable
    {
        bool _isInitialized;
        public bool IsInitialized => _isInitialized;
        Dictionary<string, ItemData> _itemList = new();
        [SerializeField] AssetLabelReference assetLabelReference;

        internal void Initialize()
        {            
            if (_isInitialized) return;
            _isInitialized = true;
            
            var startTime = Time.time;
            Game.Console.LogDebug("Initializing item database...");

            Addressables.LoadAssetsAsync<ItemData>(assetLabelReference, (a) =>
            {                
                _itemList[a.Name] = a;
            });

            Game.Console?.LogDebug($"Done initializing items took {Time.time - startTime} ms");
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
    }
}
