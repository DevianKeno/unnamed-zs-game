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
        /// <summary>
        /// Contains the list of all spawnABLE entities in the game.
        /// Key is Entity Id, Value is EntityData.
        /// </summary>
        Dictionary<string, EntityData> _entitiesDict = new();

        [SerializeField] LayerMask defaultLayer;
        /// <summary>
        /// Default layer for all entities.
        /// </summary>
        public int DEFAULT_LAYER => defaultLayer;
        [SerializeField] LayerMask outlinedLayer;
        /// <summary>
        /// Entities in this layer render screen space outlines.
        /// </summary>
        public int OUTLINED_LAYER => outlinedLayer;

        public event Action<EntityInfo> OnEntitySpawned;
        /// <summary>
        /// Subscribe to this event if you need to make last changes before the entity is removed from the universe.
        /// </summary>
        public event Action<EntityInfo> OnEntityDespawned;
                
        internal void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            var startTime = Time.time;
            Game.Console.LogInfo("Reading data: Entities...");
            foreach (var etty in Resources.LoadAll<EntityData>("Data/Entities"))
            {
                _entitiesDict[etty.Id] = etty;
            }
        }
        
        void OnEntityDespawnInternal(Entity entity)
        {
            OnEntityDespawned?.Invoke(new()
            {
                Entity = entity
            });
        }


        #region Public methods
    
        public delegate void OnEntitySpawnComplete(EntityInfo info);
        public struct EntityInfo
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
                Game.Console.LogDebug($"Tried to spawn entity '{entityId}' but Id does not exists.");
                return;
            }

            try
            {
                var ettyData = _entitiesDict[entityId];
                Addressables.LoadAssetAsync<GameObject>(ettyData.AssetReference).Completed += (a) =>
                {
                    if (a.Status == AsyncOperationStatus.Succeeded)
                    {
                        var go = Instantiate(a.Result, position, Quaternion.identity, GetTransformParent());
                        go.name = $"{ettyData.DisplayName} (Entity)";
                        if (!go.TryGetComponent(out Entity entity)) /// what do making entity without an entity component!!
                        {
                            Destroy(go);
                            return;
                        }

                        entity.OnSpawnInternal();
                        entity.OnDespawned += OnEntityDespawnInternal;
                        go.transform.position = position;

                        var info = new EntityInfo()
                        {
                            Entity = entity
                        };
                        callback?.Invoke(info);
                        OnEntitySpawned?.Invoke(info);

                        Game.Console.LogDebug($"Spawned entity {entityId} at ({position.x}, {position.y}, {position.z})");
                        return;
                    }

                    Game.Console.LogDebug($"Tried to spawn entity {entityId}, but failed miserably");
                };
            }
            catch (Exception ex)
            {
                Game.Console.LogError($"An internal error occured when trying to spawn entity {entityId}.");
                Debug.LogException(ex);
            }
        }

        public delegate void OnSpawnCallback<T>(EntitySpawnedInfo<T> info);
        public struct EntitySpawnedInfo<T>
        {
            public T Entity { get; set; }
        }

        public void Spawn<T>(string entityId, Vector3 position = default, OnSpawnCallback<T> onCompleted = null) where T : Entity
        {
            if (!_entitiesDict.ContainsKey(entityId))
            {
                Game.Console.LogDebug($"Entity '{entityId}' does not exist!");
                return;
            }

            try
            {
                var ettyData = _entitiesDict[entityId];
                Addressables.LoadAssetAsync<GameObject>(ettyData.AssetReference).Completed += (a) =>
                {
                    if (a.Status == AsyncOperationStatus.Succeeded)
                    {
                        var go = Instantiate(a.Result, position, Quaternion.identity, GetTransformParent());
                        go.name = $"{ettyData.DisplayName} (Entity)";
                        if (!go.TryGetComponent(out Entity entity))
                        {
                            Destroy(go);
                            return;
                        }

                        entity.OnSpawnInternal();
                        entity.OnDespawned += OnEntityDespawnInternal;
                        go.transform.position = position;
                        
                        onCompleted?.Invoke(new EntitySpawnedInfo<T>()
                        {
                            Entity = entity as T
                        });
                        OnEntitySpawned?.Invoke(new()
                        {
                            Entity = entity
                        });
                        
                        Game.Console.LogDebug($"Spawned entity {entityId} at ({position.x}, {position.y}, {position.z})");
                        return;
                    }

                    Game.Console.LogDebug($"Tried to spawn entity {entityId}, but failed miserably");
                };
            }
            catch (Exception ex)
            {
                Game.Console.LogError($"An internal error occured when trying to spawn entity {entityId}.");
                Debug.LogException(ex);
            }
        }
        
        public void SpawnItem(Item item, Vector3 position = default)
        {
            if (item.IsNone || item.Count <= 0)
            {
                Game.Console.LogDebug($"Item to spawn is none item.");
                Game.Console.Assert(item.Count <= 0, $"Item count is less than or equal to zero.");
                return;
            }

            try
            {
                var ettyData = _entitiesDict["item"];
                Addressables.LoadAssetAsync<GameObject>(ettyData.AssetReference).Completed += (a) =>
                {
                    if (a.Status == AsyncOperationStatus.Succeeded)
                    {
                        var go = Instantiate(a.Result, position, Quaternion.identity, GetTransformParent());
                        go.name = $"Item Entity (Entity)";
                        if (!go.TryGetComponent(out ItemEntity itemEntity)) /// this has a zero chance to fail >:(
                        {
                            Destroy(go);
                            return;
                        }
                        
                        itemEntity.Item = item;
                        itemEntity.OnSpawnInternal();
                        itemEntity.OnDespawned += OnEntityDespawnInternal;
                        go.transform.position = position;
                        
                        Game.Console.LogDebug($"Spawned item {item.Data.DisplayName} at ({position.x}, {position.y}, {position.z})");
                        return;
                    }
                };
            }
            catch (Exception ex)
            {
                Game.Console.LogError($"An internal error occured when trying to spawn item.");
                Debug.LogException(ex);
            }
        }

        public void Kill(Entity entity)
        {
            if (entity == null) return;

            entity.Despawn();
        }

        public bool IsValidId(string id)
        {
            return _entitiesDict.ContainsKey(id);
        }

        #endregion


        Transform GetTransformParent()
        {
            return Game.World.IsInWorld ? Game.World.CurrentWorld.entitiesContainer : transform;
        }
    }
}