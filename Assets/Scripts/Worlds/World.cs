using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

using UnityEngine;
using Newtonsoft.Json;

using Epic.OnlineServices.UserInfo;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.Saves;
using UZSG.Objects;
using UZSG.Worlds.Events;

namespace UZSG.Worlds
{
    public class World : MonoBehaviour, ISaveDataReadWrite<WorldSaveData>
    {
        bool _isInitialized; /// to prevent initializing the world twice :)

        WorldAttributes worldAttributes;
        public WorldAttributes WorldAttributes => worldAttributes;
        
        /// testing
        public string WorldName = "testsavedworld";
        public WorldEventCollection Events { get; private set; } = new();

        [SerializeField] WorldTimeController timeController;
        public WorldTimeController Time => timeController;
        [SerializeField] WorldEventController eventsController;
        public WorldEventController WorldEvents => eventsController;

        bool _isActive;
        bool _hasValidSaveData;
        WorldSaveData _saveData;
        System.Diagnostics.Stopwatch _initializeTimer;

        /// <summary>
        /// Key is EntityData Id; Value is list of Entity instances of that Id.
        /// </summary>
        Dictionary<string, List<Entity>> _cachedIdEntities = new();
        /// <summary>
        /// Key is Instance Id.
        /// </summary>
        Dictionary<int, BaseObject> _objectInstanceIds = new();
        /// <summary>
        /// Key is Instance Id.
        /// </summary>
        Dictionary<int, Entity> _entityInstanceIds = new();

        [SerializeField] internal Transform objectsContainer;
        [SerializeField] internal Transform entitiesContainer;
        

        #region Initializing methods

        event Action onInitializeCompleted;
        public void Initialize(WorldSaveData saveData, Action onInitializeCompleted = null)
        {
            if (_isInitialized) return;
            _isInitialized = true;

            this.onInitializeCompleted += onInitializeCompleted;
            Game.Console.Log($"[World]: Initializing world...");
            _initializeTimer = new();
            _initializeTimer.Start();

            // Game.Main.OnLateInit += OnLateInit;
            InitializeInternal();
            ReadSaveData(saveData);
            InitializeEvents();
            
            _initializeTimer.Stop();
            Game.Console.Log($"[World]: Done world loading took {_initializeTimer.ElapsedMilliseconds} ms");
            this.onInitializeCompleted?.Invoke();
            this.onInitializeCompleted = null;
        }

        void InitializeInternal()
        {
            //Game.Main.OnLateInit -= InitializeInternal;
            timeController.Initialize();
            eventsController.Initialize();

            RegisterExistingInstances();
            if (LoadOnEnterPlayMode)
            {
                LoadFromPath();
            }
        }

        void InitializeEvents()
        {
            Events.OnPlayerJoined += OnPlayerJoined;
            Events.OnPlayerLeft += OnPlayerLeft;

            Game.Entity.OnEntitySpawned += OnEntitySpawned;
            Game.Entity.OnEntityKilled += OnEntityKilled;
            Game.Objects.OnObjectPlaced += OnObjectPlaced;
            Game.Tick.OnTick += Tick;
        }

        /// <summary>
        /// Register object/entity instances from the scene to avoid duplicates.
        /// </summary>g
        void RegisterExistingInstances()
        {
            /// Objects
            foreach (Transform c in objectsContainer)
            {
                if (c.TryGetComponent<BaseObject>(out var obj))
                {
                    _objectInstanceIds[obj.GetInstanceID()] = obj;
                }
            }

            /// Entities
            foreach (Transform c in entitiesContainer)
            {
                if (c.TryGetComponent<Entity>(out var etty))
                {
                    _entityInstanceIds[etty.GetInstanceID()] = etty;
                    CacheEntity(etty);
                }
            }
        }
        
        void SaveObjects()
        {
            Game.Console.Log("[World]: Saving objects...");

            var objectSaves = new List<ObjectSaveData>();
            foreach (Transform c in objectsContainer) /// c is child
            {
                if (!c.TryGetComponent<BaseObject>(out var obj)) continue;
                
                objectSaves.Add(obj.WriteSaveData());
            }
            _saveData.Objects = objectSaves;
        }
        
        void LoadObjects()
        {
            Game.Console.Log("[World]: Loading objects...");

            if (_saveData.Objects == null) return;
            foreach (var sd in _saveData.Objects) /// sd is saveData ;)
            {
                if (_objectInstanceIds.ContainsKey(sd.InstanceId)) continue;

                if (Game.Objects.TryGetData(sd.Id, out var data))
                {
                    Game.Objects.Place(sd.Id, callback: (info) =>
                    {
                        info.Object.ReadSaveData(sd);
                    });
                }
            }
        }

        void SaveEntities()
        {
            Game.Console.Log("[World]: Saving entities...");

            _saveData.PlayerSaves = new();
            _saveData.PlayerIdSaves = new();
            _saveData.EntitySaves = new();
            foreach (Transform c in entitiesContainer)
            {
                if (!c.TryGetComponent<Entity>(out var etty)) continue;
                // if (!etty.IsAlive) continue; 
                if (etty is Player) continue;/// no n no nono this should save as soon as they quit

                _saveData.EntitySaves.Add(etty.WriteSaveData());
            }
        }

        void LoadEntities()
        {
            Game.Console.Log("[World]: Loading entities...");

            if (_saveData.EntitySaves == null) return;
            foreach (var sd in _saveData.EntitySaves)
            {
                if (_entityInstanceIds.ContainsKey(sd.InstanceId)) continue;
                if (sd.Id == "player") continue;

                Game.Entity.Spawn(sd.Id, callback: (info) =>
                {
                    info.Entity.ReadSaveData(sd);
                });
            }
        }

        #endregion


        #region Event callbacks

        void OnPlayerJoined()
        {
            Game.Entity.Spawn<Player>("player", callback: Spawned);

            void Spawned(EntityManager.EntitySpawnedInfo<Player> info)
            {
                var name = "testname";
                if (_saveData.PlayerIdSaves.TryGetValue(name, out var sd))
                {
                    info.Entity.ReadSaveData(sd);
                }
            }
        }

        void OnPlayerLeft(Player player)
        {
            _saveData.PlayerSaves.Add(player.WriteSaveData());

            var name = "testname";
            _saveData.PlayerIdSaves[name] = player.WriteSaveData();
        }

        void OnObjectPlaced(ObjectsManager.ObjectPlacedInfo info)
        {
            info.Object.transform.SetParent(objectsContainer.transform, worldPositionStays: true);
            // CacheObjectId(info.Object); /// idk if we need to cache objects thoughhhhhh, wireless workbench?? sheeeesh
        }

        void OnEntitySpawned(EntityManager.EntityInfo info)
        {
            info.Entity.transform.SetParent(entitiesContainer.transform, worldPositionStays: true);
            CacheEntity(info.Entity);
        }

        void OnEntityKilled(EntityManager.EntityInfo info)
        {
            UncacheEntityId(info.Entity);
        }

        #endregion


        void Tick(TickInfo t)
        {

        }

        void CacheEntity(Entity etty)
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
                if (_cachedIdEntities[etty.Id].Count == 0)
                {
                    _cachedIdEntities.Remove(etty.Id);
                }
            }
        }


        void Cleanup()
        {
            Game.Console.Log("[World]: Cleaning up...");

            foreach (var c in _entityInstanceIds.Values)
            {
                if (c is IWorldCleanupable etty)
                {
                    etty.Cleanup();
                }
            }
        }

        #region Public methods

        /// <summary>
        /// Joins a player to the world.
        /// </summary>
        public void JoinPlayerId(UserInfoData id)
        {
            Vector3 spawnpoint = SaveData.IsNull(_saveData.WorldSpawn) ? new(0, 64, 0) : Vector3Ext.FromNumerics(_saveData.WorldSpawn);
            Game.Entity.Spawn<Player>("player", spawnpoint, (info) =>
            {
                Player player = info.Entity;
                var playerSaves = SaveData.IsNull(_saveData.PlayerIdSaves) ? new() : _saveData.PlayerIdSaves;
                if (playerSaves.TryGetValue(id.UserId.ToString(), out var playerSave))
                {
                    player.ReadSaveData(playerSave);
                }
                player.DisplayName = id.DisplayName;
            });

            Game.Console.Log($"[World]: {id.DisplayName} joined the game");
        }

        /// <summary>
        /// Returns the list of Entities of Id present in the World.
        /// Returns an empty list if no Entity/s exists of Id.
        /// </summary>
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
            Game.Console.Log("[World]: Saving world...");
            var time = UnityEngine.Time.time;

            var json = WriteSaveData();
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
            Game.Console.Log($"[World]: World saved. Took {elapsedTime:0.##} ms");
            Debug.Log($"[World]: World saved to '{filepath}'");
        }

        public void SaveWorldAsync()
        {
                  
        }

        public void LoadWorld()
        {
            if (_isActive)
            {
                Game.Console.Log("[World]: Cannot load a World on top of an existing one.");
                return;
            }
            
            Game.Console.Log("[World]: Loading world...");
        }

        public void ExitWorld(bool save = true)
        {
            Cleanup();

            if (save)
            {
                SaveWorld();
            }

            Game.Entity.OnEntitySpawned -= OnEntitySpawned;
            Game.Entity.OnEntityKilled -= OnEntityKilled;

            Game.Objects.OnObjectPlaced -= OnObjectPlaced;
            Game.Tick.OnTick -= Tick;
        }

        public void ReadSaveData(WorldSaveData saveData)
        {
            if (saveData == null)
            {
                _hasValidSaveData = false;
                return;
            }
            
            this._saveData = saveData;
            _hasValidSaveData = true;
            LoadObjects();
            LoadEntities();
        }

        public async Task<WorldSaveData> WriteSaveJsonAsync()
        {
            SaveObjects();

            return _saveData;
        }
        
        public WorldSaveData WriteSaveData()
        {
            _saveData.LastModifiedDate = DateTime.Now;
            _saveData.LastPlayedDate = DateTime.Now;

            SaveObjects();
            SaveEntities();

            return _saveData;
        }

        #endregion
        

        #region Debug

        [SerializeField] bool LoadOnEnterPlayMode;
        [SerializeField] bool SaveOnExitPlayMode;

        public void LoadFromPath()
        {
            var path = Application.persistentDataPath + $"/SavedWorlds/{WorldName}";
            var filepath = path + "/data.json";
            var dataJson = File.ReadAllText(filepath);
            var wsd = JsonConvert.DeserializeObject<WorldSaveData>(dataJson);
            ReadSaveData(wsd);
        }
        
        void OnApplicationQuit()
        {
            ExitWorld(SaveOnExitPlayMode);
        }

        #endregion
    }
}
