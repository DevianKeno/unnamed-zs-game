using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using Newtonsoft.Json;

using Epic.OnlineServices.UserInfo;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.Saves;
using UZSG.Objects;
using UZSG.Worlds.Events;
using UnityEngine.InputSystem;
using UZSG.UI;
using UZSG.Data;

namespace UZSG.Worlds
{
    public class World : MonoBehaviour, ISaveDataReadWrite<WorldSaveData>
    {
        public bool IsPaused { get; private set; }
        public bool IsPausable { get; private set; }

        LevelData levelData;
        public LevelData LevelData => levelData;

        WorldAttributes worldAttributes;
        public WorldAttributes Attributes => worldAttributes;
        [SerializeField] TimeController timeController;
        public TimeController Time => timeController;
        [SerializeField] WorldEventController eventsController;
        public WorldEventController WorldEvents => eventsController;

        public WorldEventCollection Events { get; private set; } = new();
        bool _isInitialized; /// to prevent initializing the world twice :)
        bool _isActive;
        bool _hasValidSaveData;
        string ownerId = "localhost";
        WorldSaveData _saveData;
        System.Diagnostics.Stopwatch _initializeTimer;

        Player ownerPlayer;
        /// <summary>
        /// Key is EntityData Id; Value is list of Entity instances of that Id.
        /// </summary>
        Dictionary<string, List<Entity>> _cachedIdEntities = new();
        Dictionary<string, Player> _playerEntities = new();
        /// <summary>
        /// Key is Instance Id.
        /// </summary>
        Dictionary<int, BaseObject> _objectInstanceIds = new();
        /// <summary>
        /// Key is Instance Id.
        /// </summary>
        Dictionary<int, Entity> _entityInstanceIds = new();

        #region Events

        public event Action OnPause;
        public event Action OnUnpause;

        #endregion

        [SerializeField] internal Transform objectsContainer;
        [SerializeField] internal Transform entitiesContainer;
        
        InputAction pauseInput;
        PauseMenuWindow pauseMenu;


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

            InitializeInternal();
            InitializeAttributes(saveData);
            InitializeEvents();
            ReadSaveData(saveData);

            _initializeTimer.Stop();
            Game.Console.Log($"[World]: Done world loading took {_initializeTimer.ElapsedMilliseconds} ms");
            
            this.onInitializeCompleted?.Invoke();
            this.onInitializeCompleted = null;
        }

        void InitializeAttributes(WorldSaveData saveData)
        {
            worldAttributes = new()
            {
                LevelId = saveData.LevelId,
                WorldName = saveData.WorldName,
                MaxPlayers = saveData.MaxPlayers,
                DifficultyLevel = saveData.DifficultyLevel,
            };
        }

        void InitializeInternal()
        {
            // Game.Main.OnLateInit -= InitializeInternal;
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
            foreach (Transform child in objectsContainer)
            {
                if (!child.TryGetComponent(out BaseObject baseObject)) continue;
                // if (!baseObject.IsDirty) continue;

                objectSaves.Add(baseObject.WriteSaveData());
            }
            _saveData.Objects = objectSaves;
        }
        
        void LoadObjects()
        {
            Game.Console.Log("[World]: Loading objects...");
            if (_saveData.Objects == null) return;
            
            foreach (var baseObject in _objectInstanceIds.Values)
            {
                try
                {
                    /// Initializing method
                    baseObject.Place();
                }
                catch
                {
                    if (baseObject.ObjectData != null)
                    {
                        Game.Console.Warn($"An error occured when loading object '{baseObject.ObjectData.Id}'!");
                    }
                    continue;
                }
            }

            foreach (var objectSaveData in _saveData.Objects)
            {
                if (_objectInstanceIds.TryGetValue(objectSaveData.InstanceId, out BaseObject baseObject))
                {
                    /// object already exists in world, just initialize it with the Place() method
                    baseObject.Place();
                }
                else
                {
                    /// construct the object and initialize
                    Game.Objects.PlaceNew(objectSaveData.Id, callback: (info) =>
                    {
                        baseObject = info.Object;
                        baseObject.ReadSaveData(objectSaveData);
                        baseObject.Place();
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
                        
            foreach (var etty in _entityInstanceIds.Values)
            {
                try
                {
                    /// Initializing method
                    etty.OnSpawn();
                }
                catch
                {
                    if (etty.EntityData != null)
                    {
                        Game.Console.Warn($"An error occured when loading object '{etty.EntityData.Id}'!");
                    }
                    continue;
                }
            }
            
            foreach (var sd in _saveData.EntitySaves)
            {
                if (sd.Id == "player") continue; /// salt
                if (_entityInstanceIds.TryGetValue(sd.InstanceId, out Entity etty))
                {
                    etty.OnSpawn();
                }
                else
                {
                    Game.Entity.Spawn(sd.Id, callback: (info) =>
                    {
                        info.Entity.ReadSaveData(sd);
                    });
                }
            }
        }

        Vector3 ValidateWorldSpawn(System.Numerics.Vector3 coords)
        {
            if (SaveData.FieldIsNull(coords)) coords = new(0f, 65f, 0f);
            if (Physics.Raycast(new Vector3(coords.X, 300f, coords.Z), -Vector3.up, out var hit, 999f))
                return new(coords.X, hit.point.y, coords.Z);
            return Vector3Ext.FromNumerics(coords);
        }

        #endregion

        internal void SetOwnerId(string id)
        {
            this.ownerId = id;
        }

        public bool IsOwner(Player player)
        {
            /// TODO: throws an error
            if (player == null && player.UserInfo.UserId == null) return false;
            return player.UserInfo.UserId.ToString() == this.ownerId;
        }


        #region Event callbacks

        void OnPlayerJoined(Player player)
        {
            // if (IsOwner(player))
            // {
            //     ownerPlayer = player;
            // }
            // else
            // {

            // }
        }

        void OnPlayerLeft(Player player)
        {
            if (player == null) return;

            var saveData = player.WriteSaveData();
            _saveData.PlayerSaves.Add(saveData);
            _saveData.PlayerIdSaves[player.UserInfo.UserId.ToString()] = player.WriteSaveData();
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
        
        public void Deinitialize()
        {
            eventsController.Deinitialize();     
        }

        /// <summary>
        /// Joins a player to the world.
        /// </summary>
        public void JoinPlayerId(UserInfoData id)
        {
            Game.Entity.Spawn<Player>("player", ValidateWorldSpawn(_saveData.WorldSpawn), (info) =>
            {
                Player player = info.Entity;
                var playerSaves = SaveData.FieldIsNull(_saveData.PlayerIdSaves) ? new() : _saveData.PlayerIdSaves;
                if (playerSaves.TryGetValue(id.UserId.ToString(), out var playerSave))
                {
                    player.ReadSaveData(playerSave);
                }
                player.DisplayName = id.DisplayName;
                _playerEntities[id.ToString()] = player;
                OnPlayerJoined(player);
            });

            Game.Console.Log($"[World]: {id.DisplayName} has entered the world");
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
            var settings = new JsonSerializerSettings(){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var save = JsonConvert.SerializeObject(json, Formatting.Indented, settings);
            var path = Application.persistentDataPath + $"/SavedWorlds/{WorldName}";
            var filepath = path + "/level.dat";
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
            
            pauseInput = null;

            if (save)
            {
                SaveWorld();
            }

            Game.Entity.OnEntitySpawned -= OnEntitySpawned;
            Game.Entity.OnEntityKilled -= OnEntityKilled;

            Game.Objects.OnObjectPlaced -= OnObjectPlaced;
            Game.Tick.OnTick -= Tick;
        }

        public void Pause()
        {
            if (IsPaused)
            {
                IsPaused = false;
                OnUnpause?.Invoke();
            }
            else
            {
                IsPaused = true;
                OnPause?.Invoke();
            }
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
            timeController.ReadSaveData(saveData);
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

        public string WorldName = "testsavedworld";
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
