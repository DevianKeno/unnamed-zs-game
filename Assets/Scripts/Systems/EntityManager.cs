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
                            go.name = entity.Data.Name;
                            entity.Spawn();
                        }

                        Game.Console?.Log($"Spawned entity {entityId} at ({position.x}, {position.y}, {position.z})");
                        return;
                    }

                    Game.Console?.LogDebug($"Failed to spawn entity {entityId}");
                };
            }            
        }
    }
}