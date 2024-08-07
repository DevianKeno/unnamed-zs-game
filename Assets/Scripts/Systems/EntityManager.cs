using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using UZSG.Data;
using UZSG.Entities;
using UZSG.Items;

namespace UZSG.Systems
{
    /// <summary>
    /// The entities should be only initialized upon entering a world.
    /// </summary>
    public class EntityManager : MonoBehaviour, IInitializeable
    {
        bool _isInitialized;
        public bool IsInitialized => _isInitialized;
        public bool EnableLogging;
        /// <summary>
        /// Contains the list of all spawnable entities in the game.
        /// </summary>
        Dictionary<string, EntityData> _entitiesDict = new();
        
        public event EventHandler<EntitySpawnedInfo> OnEntitySpawn;
                
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


        #region Public methods
    
        public delegate void OnEntitySpawnComplete(EntitySpawnedInfo info);
        public struct EntitySpawnedInfo
        {
            public Entity Entity { get; set; }
        }

        /// <summary>
        /// Spawn an entity in the game world.
        /// </summary>
        public void Spawn(string entityId, Vector3 position = default, OnEntitySpawnComplete callback = null)
        {
            if (!_entitiesDict.ContainsKey(entityId))
            {
                Game.Console.LogDebug($"Entity '{entityId}' does not exist!");
                return;
            }

            var ettyData = _entitiesDict[entityId];
            Addressables.LoadAssetAsync<GameObject>(ettyData.AssetReference).Completed += (a) =>
            {
                if (a.Status == AsyncOperationStatus.Succeeded)
                {
                    var go = Instantiate(a.Result, position, Quaternion.identity, transform);
                    go.name = $"{ettyData.Name} (Entity)";
                    if (go.TryGetComponent(out Entity entity)) /// what do making entity without an entity component!!
                    {
                        var info = new EntitySpawnedInfo()
                        {
                            Entity = entity
                        };
                        OnEntitySpawn?.Invoke(this, info);
                        callback?.Invoke(info);
                        entity.OnSpawnInternal();

                        if (EnableLogging)
                        {
                            Game.Console.Log($"Spawned entity {entityId} at ({position.x}, {position.y}, {position.z})");
                        }
                        return;
                    }
                    Destroy(go);
                }

                Game.Console.LogDebug($"Tried to spawn entity {entityId}, but failed miserably");
            };
        }
        
        public delegate void OnEntitySpawnComplete<T>(EntitySpawnedInfo<T> info);
        public struct EntitySpawnedInfo<T>
        {
            public T Entity { get; set; }
        }

        public void Spawn<T>(string entityId, Vector3 position = default, OnEntitySpawnComplete<T> callback = null) where T : Entity
        {
            if (!_entitiesDict.ContainsKey(entityId))
            {
                Game.Console.LogDebug($"Entity '{entityId}' does not exist!");
                return;
            }

            var ettyData = _entitiesDict[entityId];
            Addressables.LoadAssetAsync<GameObject>(ettyData.AssetReference).Completed += (a) =>
            {
                if (a.Status == AsyncOperationStatus.Succeeded)
                {
                    var go = Instantiate(a.Result, position, Quaternion.identity, transform);
                    go.name = $"{ettyData.Name} (Entity)";
                    if (go.TryGetComponent(out Entity entity))
                    {
                        var info = new EntitySpawnedInfo<T>()
                        {
                            Entity = entity as T
                        };
                        OnEntitySpawn?.Invoke(this, new()
                        {
                            Entity = entity
                        });
                        callback?.Invoke(info);
                        entity.OnSpawnInternal();
                        
                        if (EnableLogging)
                        {
                            Game.Console.Log($"Spawned entity {entityId} at ({position.x}, {position.y}, {position.z})");
                        }
                        return;
                    }
                    Destroy(go);
                }

                Game.Console.LogDebug($"Tried to spawn entity {entityId}, but failed miserably");
            };
        }
        
        public void SpawnItem(string id, int count = 1, Vector3 position = default)
        {            
            if (!_entitiesDict.ContainsKey("item")) /// this has a zero chance to fail >:(
            {
                Game.Console.LogDebug($"Entity '{id}' does not exist!");
                return;
            }

            var ettyData = _entitiesDict["item"];
            Addressables.LoadAssetAsync<GameObject>(ettyData.AssetReference).Completed += (a) =>
            {
                if (a.Status == AsyncOperationStatus.Succeeded)
                {
                    var go = Instantiate(a.Result, position, Quaternion.identity, transform);
                    go.name = $"Item '{id}' (Entity)";
                    if (go.TryGetComponent(out ItemEntity itemEntity)) /// this has a zero chance to fail >:(
                    {
                        itemEntity.Item = new Item(id, count);
                        itemEntity.OnSpawnInternal();
                        
                        if (EnableLogging)
                        {
                            Game.Console.LogDebug($"Spawned item {id} at ({position.x}, {position.y}, {position.z})");
                        }
                        return;
                    }
                    Destroy(go);
                }
            };
        }

        public void Kill(Entity entity)
        {
            if (entity == null) return;
            Destroy(entity.gameObject);
        }

        #endregion
    }
}