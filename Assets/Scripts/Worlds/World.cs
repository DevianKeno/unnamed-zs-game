using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.AddressableAssets;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.WorldEvents;
using UZSG.Saves;
using UZSG.Objects;
using MessagePack;
using System.IO;
using Newtonsoft.Json;

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
        
        /// testing
        public string WorldName = "testsavedworld";

        [SerializeField] WorldTimeController timeController;
        public WorldTimeController Time => timeController;
        [SerializeField] WorldEventController eventsController;
        public WorldEventController Events => eventsController;

        bool _hasValidSaveData;
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
            if (saveData == null)
            {
                _hasValidSaveData = false;
                return;
            }
            
            this._saveData = saveData;
            _hasValidSaveData = true;
        }

        public async Task<WorldSaveData> WriteSaveJsonAsync()
        {
            SaveObjects();

            return _saveData;
        }
        
        public WorldSaveData WriteSaveJson()
        {
            _saveData = new WorldSaveData();
            SaveObjects();
            SaveEntities();

            return _saveData;
        }
        

        void SaveObjects()
        {
            Game.Console.Log("Saving objects...");

            _saveData.Objects = new();
            foreach (Transform c in userObjectsContainer)
            {
                if (!c.TryGetComponent<BaseObject>(out var obj)) continue;
                
                _saveData.Objects.Add(obj.WriteSaveJson());
            }
        }
        
        void LoadUserObjects()
        {
            foreach (var c in _saveData.Objects)
            {
                // var obj = c.GetComponent<BaseObject>();
                // _saveData.Objects.Add(obj.WriteSaveJson());

                // var go = Game.O()
            }
        }

        void SaveEntities()
        {
            Game.Console.Log("Saving entities...");

            _saveData.PlayerSaves = new();
            _saveData.EntitySaves = new();
            foreach (Transform c in entitiesContainer)
            {
                if (!c.TryGetComponent<Entity>(out var etty)) continue;
                // if (!etty.IsAlive) continue; 

                if (etty is Player p) /// no n no nono this should save as soon as they quit
                {
                    // _saveData.PlayerSaves.Add(p.WriteSaveJson());
                }
                else
                {
                    _saveData.EntitySaves.Add(etty.WriteSaveJson());
                }
            }
        }

        void LoadEntities()
        {
            foreach (var sd in _saveData.EntitySaves)
            {
                if (sd is PlayerSaveData psd)
                {
                    
                }
                else
                {
                    Game.Entity.Spawn(sd.Id, callback: (info) =>
                    {
                        info.Entity.ReadSaveJson(sd);
                    });
                }
            }
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

        public void SaveWorld()
        {
            Game.Console.Log("Saving world...");
            var time = UnityEngine.Time.time;
            var json = WriteSaveJson();

            var settings = new JsonSerializerSettings()
            { 
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            var save = JsonConvert.SerializeObject(json, Formatting.Indented, settings);
            var path = Application.persistentDataPath + $"/SavedWorlds/{WorldName}";
            var filepath = path + "/data.json";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            File.WriteAllText(filepath, save);

            var elapsedTime = UnityEngine.Time.time - time; 
            Game.Console.Log($"World saved. Took {elapsedTime:0.##} ms");
            Debug.Log($"World saved to '{filepath}'");
        }

        public void SaveWorldAsync()
        {
                  
        }

        #endregion
    }
}
