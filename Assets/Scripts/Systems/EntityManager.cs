using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UZSG.Entities;
using UZSG.Items;

namespace UZSG.Systems
{
    /// <summary>
    /// The entities should be only initialized upon entering a world.
    /// </summary>
    public class EntityManager : MonoBehaviour, IInitializeable
    {
        public struct EntitySpawnedContext
        {
            public Entity Entity { get; set; }
        }

        bool _isInitialized;
        public bool IsInitialized => _isInitialized;
        /// <summary>
        /// Contains the list of all spawnable entities in the game.
        /// </summary>
        Dictionary<string, EntityData> _entitiesDict = new();
        [SerializeField] AssetLabelReference assetLabelReference;
        
        public event EventHandler<EntitySpawnedContext> OnEntitySpawn;

        public Vector3 SpawnCoordinates = new(0f, 0f, 0f);
        
        internal void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            var startTime = Time.time;
            Game.Console.Log("Reading data: Entities...");
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
                        var go = Instantiate(a.Result, position, Quaternion.identity, transform);
                        
                        if (go.TryGetComponent(out Entity entity))
                        {
                            go.name = $"{entity.EntityData.Name} (Entity)";
                            entity.OnSpawn();
                        }
                        
                        OnEntitySpawn?.Invoke(this, new()
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
                        var go = Instantiate(a.Result, position, Quaternion.identity, transform);
                        
                        if (go.TryGetComponent(out Entity entity))
                        {
                            go.name = entity.EntityData.Name;
                            entity.OnSpawn();
                        }

                        
                        OnEntitySpawn?.Invoke(this, new()
                        {
                            Entity = entity
                        });
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

        public void SpawnItem(string itemId, int count = 1)
        {            
            if (_entitiesDict.ContainsKey("item")) /// this has a zero chance to fail >:(
            {
                Addressables.LoadAssetAsync<GameObject>(_entitiesDict["item"].AssetReference).Completed += (a) =>
                {
                    if (a.Status == AsyncOperationStatus.Succeeded)
                    {
                        Vector3 position = new(0f, 1f, 0f);
                        var go = Instantiate(a.Result, position, Quaternion.identity, transform);

                        if (go.TryGetComponent(out ItemEntity itemEntity)) /// this has a zero chance to fail >:(
                        {
                            var item = new Item(itemId, count);
                            go.name = $"Item ({item.Name})";
                            itemEntity.Item = item;
                            itemEntity.OnSpawn();
                        }
                        Game.Console.LogDebug($"Spawned item {itemId} at ({position.x}, {position.y}, {position.z})");
                        return;
                    }
                    else
                    {
                        Game.Console.Log($"Failed to spawn item {itemId}");
                    }
                };
            }
            else
            {
                /// Force load item asset
                Game.Console.Log($"Missing asset for Item (Entity)");
            }
        }

        public void Kill(Entity entity)
        {
            if (entity == null) return;
            Destroy(entity.gameObject);
        }
    }
}