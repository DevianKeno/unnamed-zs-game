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
        Dictionary<string, EntityData> _entitiesDict = new();
        [SerializeField] AssetLabelReference assetLabelReference;
        
        public Vector3 SpawnCoordinates = new(0f, 0f, 0f);
        
        internal void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            var startTime = Time.time;
            Game.Console.Log("Initializing Entity database...");
            var ettys = Resources.LoadAll<EntityData>("Data/entities");
            foreach (var etty in ettys)
            {
                _entitiesDict[etty.Id] = etty;
            }
        }

        /// <summary>
        /// Spawn an entity in the game world.
        /// </summary>
        public void Spawn(string entityId)
        {
            if (_entitiesDict.ContainsKey(entityId))
            {
                Addressables.LoadAssetAsync<GameObject>(_entitiesDict[entityId].AssetReference).Completed += (a) =>
                {
                    if (a.Status == AsyncOperationStatus.Succeeded)
                    {
                        Vector3 position = new(0f, 1f, 0f);
                        var go = Instantiate(a.Result, position, Quaternion.identity);
                        
                        if (go.TryGetComponent(out Entity entity))
                        {
                            go.name = $"{entity.EntityData.Name} (Entity)";
                            entity.OnSpawn();
                        }
                        Game.Console.Log($"Spawned entity {entityId} at ({position.x}, {position.y}, {position.z})");
                        return;
                    }

                    Game.Console.LogDebug($"Failed to spawn entity {entityId}");
                };
            }            
        }
        
        public struct EntitySpawnedInfo
        {
            public Entity Entity { get; set; }
        }

        public delegate void OnEntitySpawnComplete(EntitySpawnedInfo info);

        /// <summary>
        /// Spawn an entity in the game world.
        /// </summary>
        public void Spawn(string entityId, OnEntitySpawnComplete callback = null)
        {
            if (_entitiesDict.ContainsKey(entityId))
            {
                Addressables.LoadAssetAsync<GameObject>(_entitiesDict[entityId].AssetReference).Completed += (a) =>
                {
                    if (a.Status == AsyncOperationStatus.Succeeded)
                    {
                        Vector3 position = new(0f, 1f, 0f);
                        var go = Instantiate(a.Result, position, Quaternion.identity);
                        
                        if (go.TryGetComponent(out Entity entity))
                        {
                            go.name = entity.EntityData.Name;
                            entity.OnSpawn();
                        }

                        callback?.Invoke(new()
                        {
                            Entity = entity
                        });
                        Game.Console.Log($"Spawned entity {entityId} at ({position.x}, {position.y}, {position.z})");
                        return;
                    }

                    Game.Console.LogDebug($"Failed to spawn entity {entityId}");
                };
            }
        }

        public void SpawnItem(string itemId)
        {            
            if (_entitiesDict.ContainsKey("item"))
            {
                // Load Item (Entity) model
                Addressables.LoadAssetAsync<GameObject>(_entitiesDict["item"].AssetReference).Completed += (a) =>
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
                        Game.Console.LogDebug($"Spawned item {itemId} at ({position.x}, {position.y}, {position.z})");
                        return;
                        
                    } else
                    {
                        Game.Console.Log($"Failed to spawn item {itemId}");
                    }
                };
            } else
            {
                /// Force load item asset
                Game.Console.Log($"Missing asset for Item (Entity)");
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

            //             Game.Console.LogDebug($"Spawned item at ({position.x}, {position.y}, {position.z})");
            //             return;
            //         } else
            //         {
            //             Game.Console.Log($"Failed to spawn item {itemId}");
            //         }
            //     };
            // } else
            // {
            //     Game.Console.Log($"Failed to spawn item {itemId} as it does not exists");
            // }
        }

        public void Kill(Entity entity)
        {
            Destroy(entity.gameObject);
        }
    }
}