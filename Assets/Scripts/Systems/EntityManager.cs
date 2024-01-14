using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UZSG.Entities;

namespace UZSG.Systems
{
    /// <summary>
    /// The entities should be only initialized upon entering a world.
    /// </summary>
    public class EntityManager : MonoBehaviour, IInitializable
    {
        bool _isInitialized;
        public bool IsInitialized => _isInitialized;
        /// <summary>
        /// Contains the list of all spawnable entities in the game.
        /// </summary>
        Dictionary<string, EntityData> _entityList = new();
        [SerializeField] AssetLabelReference assetLabelReference;
        public Vector3 SpawnCoordinates = new(0f, 0f, 0f);
        
        internal void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            var startTime = Time.time;
            Game.Console?.Log("Initializing Entity database...");

            Addressables.LoadAssetsAsync<EntityData>(assetLabelReference, (a) =>
            {
                Game.Console?.LogDebug($"Loading data for Entity {a.Id}");
                _entityList[a.Id] = a;
            });
        }

        /// <summary>
        /// Spawn an entity in the game world.
        /// </summary>
        public void Spawn(string entityId)
        {
            if (_entityList.ContainsKey(entityId))
            {
                Addressables.LoadAssetAsync<GameObject>(_entityList[entityId].AssetReference).Completed += (a) =>
                {
                    if (a.Status == AsyncOperationStatus.Succeeded)
                    {
                        Vector3 position = new(0f, 1f, 0f);
                        var go = Instantiate(a.Result, position, Quaternion.identity);
                        
                        if (go.TryGetComponent(out Entity entity))
                        {
                            go.name = entity.EntityData.Name;
                            // entity.OnSpawn();
                        }

                        Game.Console?.Log($"Spawned entity {entityId} at ({position.x}, {position.y}, {position.z})");
                    }

                    Game.Console?.LogDebug($"Failed to spawn entity {entityId}");
                };
            }            
        }

        /// <summary>
        /// Spawn an entity in the game world.
        /// </summary>
        public void Spawn(string entityId, out GameObject obj)
        {
            GameObject loadedObj = null;

            if (_entityList.ContainsKey(entityId))
            {
                Addressables.LoadAssetAsync<GameObject>(_entityList[entityId].AssetReference).Completed += (a) =>
                {
                    if (a.Status == AsyncOperationStatus.Succeeded)
                    {
                        Vector3 position = new(0f, 1f, 0f);
                        var go = Instantiate(a.Result, position, Quaternion.identity);
                        loadedObj = go;
                        
                        if (go.TryGetComponent(out Entity entity))
                        {
                            go.name = entity.EntityData.Name;
                            entity.OnSpawn();
                        }                        

                        Game.Console?.Log($"Spawned entity {entityId} at ({position.x}, {position.y}, {position.z})");
                        return;
                    }

                    Game.Console?.LogDebug($"Failed to spawn entity {entityId}");
                };
            }

            obj = loadedObj;
        }

        public void SpawnItem(string itemId)
        {            
            if (_entityList.ContainsKey("item"))
            {
                // Load Item (Entity) model
                Addressables.LoadAssetAsync<GameObject>(_entityList["item"].AssetReference).Completed += (a) =>
                {
                    if (a.Status == AsyncOperationStatus.Succeeded)
                    {
                        Vector3 position = new(0f, 1f, 0f);
                        var go = Instantiate(a.Result, position, Quaternion.identity);

                        // Load item data
                        if (go.TryGetComponent(out ItemEntity itemEntity)) // this has a zero chance to fail >:(
                        {
                            itemEntity.SetItemData(itemId);
                            go.name = itemEntity.ItemData.Name;
                            itemEntity.OnSpawn();
                        }

                        Game.Console?.LogDebug($"Spawned item {itemId} at ({position.x}, {position.y}, {position.z})");
                        return;
                        
                    } else
                    {
                        Game.Console?.Log($"Failed to spawn item {itemId}");
                    }
                };
            } else
            {
                // Force load item asset
                Game.Console?.Log($"Missing asset for Item (Entity)");
            }
            
            // obj = loadedObj;

            // if (Game.Items.TryGetItemData(itemId, out ItemData itemData))
            // {
            //     Addressables.LoadAssetAsync<GameObject>(itemData.AssetReference).Completed += (a) =>
            //     {
            //         if (a.Status == AsyncOperationStatus.Succeeded)
            //         {
            //             Vector3 position = new(0f, 1f, 0f);
            //             var go = Instantiate(a.Result, position, Quaternion.identity);

            //             if (go.TryGetComponent(out ItemEntity itemEntity)) // this has a zero chance to fail >:(
            //             {
            //                 go.name = itemEntity.Data.Name;
            //                 itemEntity.SetItemData(itemId);
            //                 itemEntity.OnSpawn();
            //             }

            //             Game.Console?.LogDebug($"Spawned item at ({position.x}, {position.y}, {position.z})");
            //             return;
            //         } else
            //         {
            //             Game.Console?.Log($"Failed to spawn item {itemId}");
            //         }
            //     };
            // } else
            // {
            //     Game.Console?.Log($"Failed to spawn item {itemId} as it does not exists");
            // }
        }

        public void Kill(Entity entity)
        {
            Destroy(entity.gameObject);
        }
    }
}