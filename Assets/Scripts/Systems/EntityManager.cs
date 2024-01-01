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
        public List<EntityData> _entityList;
        Dictionary<string, GameObject> _cachedEntities = new();

        internal void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            var startTime = Time.time;
            Game.Console?.Log("Initializing entities...");

            foreach (EntityData data in _entityList)
            {
                Game.Console?.LogDebug($"[EntityHandler]: Loading asset for entity: {data.Name}");

                Addressables.LoadAssetAsync<GameObject>(data.AssetReference).Completed += (a) =>
                {
                    if (a.Status == AsyncOperationStatus.Succeeded)
                    {
                        _cachedEntities[data.Name] = a.Result;
                    } else
                    {
                        Game.Console?.LogDebug($"Failed to load asset for entity: {data.Name}");
                    }
                };
            }

            Game.Console?.LogDebug($"Done initializing entities took {Time.time - startTime} ms");
        }

        /// <summary>
        /// Spawn an entity in the game world.
        /// </summary>
        public void Spawn(string[] args)
        {
            string entityId = args[0];
            string data = args[1];

            if (_cachedEntities.ContainsKey(entityId))
            {
                var go = Instantiate(_cachedEntities[entityId]);

                if (go.TryGetComponent(out Entity entity))
                {
                    entity.Data = new()
                    {

                    };
                }
            } else
            {
                Game.Console?.LogDebug($"Failed to spawn entity. Invalid entity id");
            }
        }
    }
}