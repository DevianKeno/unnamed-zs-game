using System;
using System.Threading.Tasks;
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
using Unity.VisualScripting;
using System.Text;
using UZSG.EOS;
using Epic.OnlineServices;
using UZSG.EOS.Lobbies;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Lobby;
using Unity.Netcode;

namespace UZSG.Worlds
{
    public class World : MonoBehaviour, ISaveDataReadWrite<WorldSaveData>
    {
        [SerializeField] bool readPlayerDataAsBytes = false;
        [SerializeField] bool writePlayerDataAsBytes = false;
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

        List<PlayerSaveData> _playerSaves = new();
        /// <summary>
        /// <c>string</c> is player Id.
        /// </summary>
        Dictionary<string, PlayerSaveData> _playerIdSaves = new();
        Player ownerPlayer;
        /// <summary>
        /// Key is EntityData Id; Value is list of Entity instances of that Id.
        /// </summary>
        Dictionary<string, List<Entity>> _cachedIdEntities = new();
        List<Player> _playerEntities = new();
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
        internal string filepath;
        internal string worldpath => Path.Join(Application.persistentDataPath, "SavedWorlds", _saveData.WorldName); 
        [SerializeField] internal Transform objectsContainer;
        [SerializeField] internal Transform entitiesContainer;
        

        #region Initializing methods

        event Action onInitializeCompleted;
        public void Initialize(WorldSaveData saveData, Action onInitializeCompleted = null)
        {
            if (_isInitialized) return;
            _isInitialized = true;

            this.onInitializeCompleted += onInitializeCompleted;

            Game.Console.LogInfo($"[World]: Initializing world...");
            var initializeTimer = new System.Diagnostics.Stopwatch();
            initializeTimer.Start();

            InitializeInternal();
            InitializeAttributes(saveData);
            InitializeEvents();
            ReadSaveData(saveData);

            initializeTimer.Stop();
            Game.Console.LogInfo($"[World]: Done world loading took {initializeTimer.ElapsedMilliseconds} ms");
            
            this.onInitializeCompleted?.Invoke();
            this.onInitializeCompleted = null;
        }

        void OnMemberStatusReceived(string lobbyId, ProductUserId memberId, LobbyMemberStatus status)
        {
            if (memberId == null || !memberId.IsValid())
            {
                Game.Console.LogDebug("[WorldManager/OnMemberStatusReceived()]: Member Id is invalid");
                return;
            }

            if (EOSSubManagers.Lobbies.CurrentLobby.Id != lobbyId) return;

            if (status == LobbyMemberStatus.Joined)
            {
                EOSSubManagers.UserInfo.QueryUserInfoByProductId(memberId, onCompleted: OnQueryUserInfoCompleted);
            }
            else if (status == LobbyMemberStatus.Disconnected)
            {

            }
        }

        void OnQueryUserInfoCompleted(ExternalAccountInfo userInfo, ProductUserId userId, Epic.OnlineServices.Result result)
        {
            if (result == Epic.OnlineServices.Result.Success)
            {
                JoinPlayerByExternalAccountInfo(userInfo);
            }
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
            timeController.Initialize();
            eventsController.Initialize();
            // RegisterExistingInstances();

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
            Game.Tick.OnTick += OnTick;
        }

        /// <summary>
        /// IMPORTANT NOTE: as it turns out, every time the level is loaded the instance id is differnet 
        /// Register object/entity instances from the scene to avoid duplicates.
        /// 
        /// </summary>
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
            Game.Console.LogInfo("[World]: Saving objects...");

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
            Game.Console.LogInfo("[World]: Loading objects...");
            if (_saveData.Objects == null) return;
            
            foreach (var objectSaveData in _saveData.Objects)
            {
                if (_objectInstanceIds.TryGetValue(objectSaveData.InstanceId, out BaseObject baseObject))
                {
                    PlaceExistingAsNew(baseObject);
                }
                else
                {
                    /// construct the object and initialize
                    var position = Utils.FromNumericVec3(objectSaveData.Transform.Position);
                    Game.Objects.PlaceNew(objectSaveData.Id, position: position, callback: (info) =>
                    {
                        info.Object.ReadSaveData(objectSaveData);
                    });
                }
            }

            static void PlaceExistingAsNew(BaseObject baseObject)
            {
                if (baseObject.IsPlaced) return;

                try
                {
                    baseObject.PlaceInternal();
                    baseObject.Place();
                }
                catch
                {
                    if (baseObject.ObjectData != null)
                    {
                        Game.Console.LogWarn($"An error occured when loading object '{baseObject.ObjectData.Id}'! Skipping.");
                    }
                }
            }
        }

        void SaveEntities()
        {
            Game.Console.LogInfo("[World]: Saving entities...");

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
            Game.Console.LogInfo("[World]: Loading entities...");

            if (_saveData.EntitySaves == null) return;
                        
            // foreach (var etty in _entityInstanceIds.Values)
            // {
            //     try
            //     {
            //         /// Initializing method
            //         etty.OnSpawn();
            //     }
            //     catch
            //     {
            //         if (etty.EntityData != null)
            //         {
            //             Game.Console.LogWarn($"An error occured when loading object '{etty.EntityData.Id}'!");
            //         }
            //         continue;
            //     }
            // }
            
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

        void OnTick(TickInfo t)
        {
        }

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
            // _saveData.PlayerSaves.Add(saveData);
            // _saveData.PlayerIdSaves[player.UserInfo.UserId.ToString()] = player.WriteSaveData();
        }

        void OnObjectPlaced(ObjectsManager.ObjectPlacedInfo info)
        {
            // CacheObjectId(info.Object); /// idk if we need to cache objects thoughhhhhh, wireless workbench?? sheeeesh
        }

        void OnEntitySpawned(EntityManager.EntityInfo info)
        {
            CacheEntity(info.Entity);
        }

        void OnEntityKilled(EntityManager.EntityInfo info)
        {
            UncacheEntityId(info.Entity);
        }

        #endregion


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
            Game.Console.LogInfo("[World]: Cleaning up...");

            foreach (var c in _entityInstanceIds.Values)
            {
                if (c is IWorldCleanupable etty)
                {
                    etty.Cleanup();
                }
            }
        }

        #region Public methods
        
        internal void Deinitialize()
        {
            eventsController.Deinitialize();     
        }
        
        public bool CheckIfPlayerHasSave(ProductUserId userId)
        {
            return userId != null && userId.IsValid() && _playerIdSaves.ContainsKey(userId.ToString());
        }

        public PlayerSaveData GetPlayerSaveData(ProductUserId userId)
        {
            if (_playerIdSaves.ContainsKey(userId.ToString()))
            {
                return _playerIdSaves[userId.ToString()];
            }
            else
            {
                return PlayerSaveData.Empty;
            }
        }

        public byte[] GetPlayerDataBytesFromUID(string uid)
        {
            var filepath = Path.Join(this.worldpath, "playerdata", $"{uid}.dat");
            var contents = ReadPlayerDataFile(filepath);
            return Encoding.UTF8.GetBytes(contents);
        }

        /// <summary>
        /// Joins a player to the world.
        /// </summary>
        internal void JoinLocalPlayer()
        {
            Game.Entity.Spawn<Player>("player", ValidateWorldSpawn(_saveData.WorldSpawn), (info) =>
            {
                Player player = info.Entity;

                var uid = "localplayer";
                this._playerIdSaves.TryGetValue(uid, out var playerSave);
                player.InitializeAsClient();

                _playerEntities.Add(player);
                OnPlayerJoined(player);
            });

            Game.Console.LogInfo($"[World]: Player has entered the world");
        }

        /// <summary>
        /// Request to the server to spawn our player client.
        /// mServer means that this method should only be called on the server. 
        /// mServer just stands for "method Server" lol
        /// </summary>
        /// <param name="userId">User id of to be owner</param>
        internal void SpawnPlayer_mServer(ProductUserId userId)
        {
            Game.Entity.Spawn<Player>("player", position: ValidateWorldSpawn(_saveData.WorldSpawn), onCompleted: (info) =>
            {
                var player = info.Entity;
                var uid = userId.ToString();
                // PlayerSaveData psdToLoad;
                // if (saveData == null)
                // {
                //     this._playerIdSaves.TryGetValue(uid, out psdToLoad);
                // }
                // else
                // {
                //     psdToLoad = saveData;
                // }
                // player.Initialize(psdToLoad);
                // player.UserInfo = userInfo;
                
                player.NetworkObject.Spawn();

                _playerEntities.Add(info.Entity);
                OnPlayerJoined(player);
            });
        }

        internal void JoinPlayerByExternalAccountInfo(ExternalAccountInfo accountInfo, PlayerSaveData saveData = null)
        {
            Game.Entity.Spawn<Player>("player", ValidateWorldSpawn(_saveData.WorldSpawn), (info) =>
            {
                Player player = info.Entity;

                var uid = accountInfo.AccountId.ToString();
                PlayerSaveData psdToLoad;
                if (saveData == null)
                {
                    this._playerIdSaves.TryGetValue(uid, out psdToLoad);
                }
                else
                {
                    psdToLoad = saveData;
                }
                player.InitializeAsClient();

                _playerEntities.Add(player);
                OnPlayerJoined(player);
            });
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
            Game.Console.LogInfo("[World]: Saving world...");

            var time = UnityEngine.Time.time;
            WorldSaveData saveData = WriteSaveData();
            var settings = new JsonSerializerSettings(){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var save = JsonConvert.SerializeObject(saveData, Formatting.Indented, settings);
            var filepath = Path.Join(this.worldpath, "level.dat");
            if (!Directory.Exists(this.worldpath)) Directory.CreateDirectory(this.worldpath);
            File.WriteAllText(filepath, save);

            var elapsedTime = UnityEngine.Time.time - time; 
            Game.Console.LogInfo($"[World]: World saved. Took {elapsedTime:0.##} ms");
            Debug.Log($"[World]: World saved to '{filepath}'");
        }

        public async void SaveWorldAsync()
        {
            await Task.Yield();
            
            SaveWorld();
        }

        public void LoadWorld()
        {
            if (_isActive)
            {
                Game.Console.LogInfo("[World]: Cannot load a World on top of an existing one.");
                return;
            }
            
            Game.Console.LogInfo("[World]: Loading world...");
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
            Game.Tick.OnTick -= OnTick;
        }

        public void Pause()
        {
            IsPaused = true;
            OnPause?.Invoke();
        }

        public void Unpause()
        {
            IsPaused = false;
            OnUnpause?.Invoke();
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
            timeController.InitializeFromSave(saveData);
            ReadPlayerData();
            LoadObjects();
            LoadEntities();
        }

        public WorldSaveData WriteSaveData()
        {
            _saveData.LastModifiedDate = DateTime.Now.ToShortDateString();
            _saveData.LastPlayedDate = DateTime.Now.ToShortDateString();

            /// Save time state
            _saveData.Day = Time.CurrentDay;
            _saveData.Hour = Time.Hour;
            _saveData.Minute = Time.Minute;
            _saveData.Second = Time.Second;

            SavePlayerData();
            SaveObjects();
            SaveEntities();

            return _saveData;
        }

        void ReadPlayerData()
        {
            var playerDataPath = Path.Join(this.worldpath, "playerdata");
            if (!Directory.Exists(playerDataPath)) Directory.CreateDirectory(playerDataPath);

            var files = Directory.GetFiles(playerDataPath);
            foreach (var file in files)
            {
                try
                {
                    var contents = ReadPlayerDataFile(file);
                    PlayerSaveData psd = JsonConvert.DeserializeObject<PlayerSaveData>(contents);
                    this._playerIdSaves[psd.UID] = psd;
                }
                catch (Exception ex)
                {
                    string message = $"Encountered an internal error when loading a player data";
                    Game.Console.LogError(message);
                    Debug.LogException(ex);
                    continue;
                }
            }
        }

        string ReadPlayerDataFile(string filepath)
        {
            if (!File.Exists(filepath)) return string.Empty;

            try
            {
                if (readPlayerDataAsBytes)
                {
                    var bytes = File.ReadAllBytes(filepath);
                    return Encoding.UTF8.GetString(bytes);
                }
                else
                {
                    return File.ReadAllText(filepath);
                }
            }
            catch
            {
                Game.Console.LogError($"An error occured when reading player data file: '{filepath}'");
            }

            return string.Empty;
        }

        void SavePlayerData()
        {
            foreach (Player player in _playerEntities)
            {
                var psd = player.WriteSaveData();
                _playerIdSaves[psd.UID] = psd;
            }

            foreach (var kv in _playerIdSaves)
            {
                string uid = kv.Key;
                PlayerSaveData psd = kv.Value;
                
                var playerDataPath = Path.Join(this.worldpath, "playerdata");
                if (!Directory.Exists(playerDataPath)) Directory.CreateDirectory(playerDataPath);

                var filepath = Path.Join(playerDataPath, $"{uid}.dat");
                var bakpath = Path.Join(playerDataPath, $"{uid}.bak");
                if (File.Exists(filepath))
                {
                    File.Copy(filepath, bakpath, overwrite: true);
                }

                var contents = JsonConvert.SerializeObject(psd);
                if (writePlayerDataAsBytes)
                {
                    var bytes = Encoding.UTF8.GetBytes(contents);
                    File.WriteAllBytes(filepath, bytes);
                }
                else
                {
                    File.WriteAllText(filepath, contents);
                }
            }
        }

        public string GetPath()
        {
            return filepath;
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
