using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.WorldEvents;
using UZSG.Saves;
using UZSG.Objects;

namespace UZSG.Worlds
{
    public enum WorldMultiplayerType {
        Internet, Friends, LAN,
    }

    public struct WorldAttributes
    {
        public string LevelId;
        public string WorldName;
        public int MaxPlayers;
        public bool IsMultiplayer;
        public WorldMultiplayerType WorldMultiplayerType;
    }

    public class World : MonoBehaviour, ISaveDataReadWrite<WorldSaveData>
    {
        bool _isInitialized; /// to prevent initializing twice

        WorldAttributes worldAttributes;
        public WorldAttributes WorldAttributes => worldAttributes;

        [SerializeField] WorldTimeController timeController;
        public WorldTimeController Time => timeController;
        [SerializeField] WorldEventController eventsController;
        public WorldEventController Events => eventsController;

        WorldSaveData _saveData;

        /// <summary>
        /// Key is EntityData Id; Value is list of Entity instances of that Id.
        /// </summary>
        Dictionary<string, List<Entity>> _cachedIdEntities = new();

        [SerializeField] Transform entitiesContainer;
        [SerializeField] Transform userObjectsContainer;
        

        #region Initializing methods

        internal void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            timeController.Initialize();
            eventsController.Initialize();
            
            Game.Entity.OnEntitySpawned += OnEntitySpawned;
            Game.Entity.OnEntityKilled += OnEntityKilled;
            Game.Tick.OnTick += Tick;
        }

        public void ReadSaveJson(WorldSaveData saveData)
        {
            throw new NotImplementedException();
        }

        public WorldSaveData WriteSaveJson()
        {
            _saveData = new WorldSaveData();
            SaveUserObjects();

            return _saveData;
        }
        
        void SaveUserObjects()
        {
            foreach (Transform c in userObjectsContainer)
            {
                var obj = c.GetComponent<BaseObject>();
                _saveData.UserObjects.Add(obj.WriteSaveJson());
            }
        }
        
        void LoadUserObjects()
        {
            throw new NotImplementedException();
        }

        void SaveEntities()
        {
            foreach (Transform c in entitiesContainer)
            {
                var etty = c.GetComponent<Entity>();

                if (!etty.IsAlive) continue;
                if (etty is Player p) /// no n no nono this should save as soon as one player quits
                {
                    _saveData.PlayerSaves.Add(p.WriteSaveJson());
                }
                else
                {
                    _saveData.EntitySaves.Add(etty.WriteSaveJson());
                }
            }
        }

        void LoadEntities()
        {
            throw new NotImplementedException();
        }

        #endregion


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


        #region Public methods

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

        #endregion
    }
}
