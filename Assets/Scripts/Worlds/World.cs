using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.WorldEvents;

namespace UZSG.Worlds
{
    public class World : MonoBehaviour
    {
        bool _isInitialized; /// to prevent initializing twice

        [SerializeField] WorldTimeController timeController;
        public WorldTimeController Time => timeController;
        [SerializeField] WorldEventController eventsController;
        public WorldEventController Events => eventsController;

        /// <summary>
        /// Key is EntityData Id; Value is list of Entity instances of that Id.
        /// </summary>
        Dictionary<string, List<Entity>> _cachedIdEntities = new();

        [SerializeField] Transform entitiesContainer;
        
        internal void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            
            Game.Entity.OnEntitySpawned += OnEntitySpawned;
            Game.Entity.OnEntityKilled += OnEntityKilled;
            Game.Tick.OnTick += Tick;
        }

        #region Event callbacks

        void OnEntitySpawned(EntityManager.EntitySpawnedInfo info)
        {
            info.Entity.transform.SetParent(entitiesContainer.transform, worldPositionStays: true);
            CacheEntityId(info.Entity);
        }

        void OnEntityKilled(EntityManager.EntityKilledInfo info)
        {
            UncacheEntityId(info.Entity);
        }

        #endregion


        void Tick(TickInfo t)
        {

        }

        void CacheEntityId(Entity etty)
        {
            if (!_cachedIdEntities.TryGetValue(etty.Id, out var list))
            {
                list = new();
                _cachedIdEntities[etty.Id] = list;
            }
            list.Add(etty);
        }

        void UncacheEntityId(Entity etty)
        {
            if (_cachedIdEntities.ContainsKey(etty.Id))
            {
                _cachedIdEntities[etty.Id].Remove(etty);
            }
        }

        /// <summary>
        /// Returns the list of Entities of Id present in the World.
        /// </summary>
        /// <param name="id"></param>
        public List<Entity> GetEntitiesById(string id)
        {
            if (_cachedIdEntities.TryGetValue(id, out var list))
            {
                return list;
            }
            return new();
        }
    }
}
