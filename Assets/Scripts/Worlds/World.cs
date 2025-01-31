using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Unity.Netcode;
using UnityEngine;

using Newtonsoft.Json;

using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;

using UZSG.Data;
using UZSG.EOS;
using UZSG.Entities;
using UZSG.Network;
using UZSG.Objects;
using UZSG.Saves;
using UZSG.Systems;
using UZSG.Worlds.Events;

namespace UZSG.Worlds
{
    public class World : MonoBehaviour, ISaveDataReadWrite<WorldSaveData>
    {
        public const string LOCAL_PLAYER_ID = "localplayer";
        public const string LOCAL_PLAYER_NAME = "Player";

        public bool IsPaused { get; private set; }
        public bool IsPausable { get; private set; }

        public LevelData LevelData => levelData;

        WorldAttributes worldAttributes;
        public WorldAttributes Attributes => worldAttributes;

        [SerializeField] TimeController timeController;
        public TimeController Time => timeController;

        [SerializeField] WeatherController weatherController;
        public WeatherController Weather => weatherController;

        [SerializeField] WorldEventController eventsController;
        public WorldEventController WorldEvents => eventsController;

        [Space]
        [SerializeField] WorldNetwork worldNetwork;
        [SerializeField] NetworkObject networkObject;

        public ProductUserId OwnerPUID { get; private set; }

        bool _isInitialized; /// to prevent initializing the world twice :)
        bool _isActive;
        bool _hasValidSaveData;
        internal string worldpath => Path.Join(Application.persistentDataPath, WorldManager.WORLDS_FOLDER, currentSaveData.WorldName); 
        LevelData levelData;
        WorldSaveData currentSaveData;
        Player localPlayer;
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
        /// <summary>
        /// List of online players. 
        /// <c>string</c> is ProductUserId as string.
        /// </summary>
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
        
        [Space]
        [SerializeField] internal Transform objectsContainer;
        [SerializeField] internal Transform entitiesContainer;
        [SerializeField] GameObject worldNetworkPrefab;
        [SerializeField] bool readPlayerDataAsBytes = false;
        [SerializeField] bool writePlayerDataAsBytes = false;

        void Awake()
        {
            timeController.GetComponentInChildren<TimeController>();
            weatherController.GetComponentInChildren<WeatherController>();
        }


        #region Initializing methods

        public delegate void OnInitializeCompletedCallback();
        public void Initialize(WorldSaveData saveData, OnInitializeCompletedCallback onCompleted = null)
        {
            if (_isInitialized) return;
            _isInitialized = true;

            Game.Console.LogInfo($"[World]: Initializing world...");
            var initializeTimer = new System.Diagnostics.Stopwatch();
            initializeTimer.Start();

            InitializeInternal();
            InitializeAttributes(saveData);
            InitializeEvents();

            if (NetworkManager.Singleton.IsListening && NetworkManager.Singleton.IsServer)
            {
                OwnerPUID = Game.EOS.GetProductUserId();
                this.networkObject = Instantiate(worldNetworkPrefab, parent: transform).GetComponent<NetworkObject>();
                this.networkObject.name = "World (NetworkObject)";
                this.networkObject.SpawnWithOwnership(NetworkManager.Singleton.LocalClientId); /// should be server's I think

                InitializeLobbyEvents_ServerMethod();
            }
            
            ReadSaveData(saveData);
            SpawnPlayers();

            initializeTimer.Stop();
            Game.Console.LogInfo($"[World]: Done world loading took {initializeTimer.ElapsedMilliseconds} ms");
            
            onCompleted?.Invoke();
        }
        
        void InitializeLobbyEvents_ServerMethod()
        {
            if (!EOSSubManagers.Lobbies.IsInLobby) return;

            EOSSubManagers.Lobbies.AddNotifyMemberStatusReceived(OnMemberStatusReceived);
        }

        void OnMemberStatusReceived(string lobbyId, ProductUserId memberId, LobbyMemberStatus status)
        {
            if (memberId == null || !memberId.IsValid())
            {
                Game.Console.LogDebug("[WorldManager/OnMemberStatusReceived()]: Member Id is invalid");
                return;
            }

            if (EOSSubManagers.Lobbies.CurrentLobby.Id != lobbyId) return;

            if (status == LobbyMemberStatus.Disconnected)
            {
                string uid = memberId.ToString();
                SaveDataForPlayer(uid);
                if (_playerEntities.TryGetValue(uid, out var player))
                {
                    player.NetworkObject.Despawn();
                }
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
            if (NetworkManager.Singleton.IsListening && NetworkManager.Singleton.IsServer)
            {
                /// Server handles time, clients are just synced
                timeController.Initialize();
                weatherController.Initialize();
                eventsController.Initialize();
            }
            else /// we're offline
            {
                timeController.Initialize();
                weatherController.Initialize();
                eventsController.Initialize();
            }
            // RegisterExistingInstances();
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
            currentSaveData.Objects = objectSaves;
        }
        
        void LoadObjects()
        {
            Game.Console.LogInfo("[World]: Loading objects...");
            if (currentSaveData.Objects == null) return;
            
            foreach (var objectSaveData in currentSaveData.Objects)
            {
                if (_objectInstanceIds.TryGetValue(objectSaveData.InstanceId, out BaseObject baseObject))
                {
                    PlaceExistingAsNew(baseObject);
                }
                else
                {
                    /// construct the object and initialize
                    var position = Utils.FromFloatArray(objectSaveData.Transform.Position);
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

            currentSaveData.EntitySaves = new();
            foreach (Transform c in entitiesContainer)
            {
                if (!c.TryGetComponent<Entity>(out var etty)) continue;
                // if (!etty.IsAlive) continue; 
                if (etty is Player) continue;/// no n no nono this should save as soon as they quit

                currentSaveData.EntitySaves.Add(etty.WriteSaveData());
            }
        }

        void LoadEntities()
        {
            Game.Console.LogInfo("[World]: Loading entities...");

            if (currentSaveData.EntitySaves == null) return;
                        
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
            
            foreach (var sd in currentSaveData.EntitySaves)
            {
                if (sd.Id == LOCAL_PLAYER_ID) continue; /// salt
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

        /// <summary>
        /// Executed after world initialization is done.
        /// </summary>
        void SpawnPlayers()
        {
            if (NetworkManager.Singleton.IsListening)
            {
                if (NetworkManager.Singleton.IsServer)
                {
                    /// spawn ourself
                    SpawnPlayer_ServerMethod(Game.EOS.GetProductUserId());
                }
                else if (NetworkManager.Singleton.IsClient)
                {
                    EOSSubManagers.P2P.RequestSpawnPlayer(EOSSubManagers.Lobbies.CurrentLobby.OwnerProductUserId, onCompleted: () => 
                    {
                        Game.Console.LogInfo("I think we're spawned on the server now");
                    });
                }
            }
            else /// we're offline
            {
                SpawnLocalPlayer();
            }
        }

        void DeinitializeEvents()
        {
            Game.Entity.OnEntitySpawned -= OnEntitySpawned;
            Game.Entity.OnEntityKilled -= OnEntityKilled;

            Game.Objects.OnObjectPlaced -= OnObjectPlaced;
            Game.Tick.OnTick -= OnTick;
        }

        #endregion


        #region Event callbacks

        void OnTick(TickInfo t)
        {
            float tickThreshold = Game.Tick.TPS / TickSystem.NormalTPS;
            float secondsCalculation = Game.Tick.SecondsPerTick * (Game.Tick.CurrentTick / 32f) * tickThreshold;

            Time.Tick(secondsCalculation);
            Weather.Tick(secondsCalculation);
        }

        void OnPlayerJoined(Player player)
        {
        }

        void OnPlayerLeft(Player player)
        {
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
            UncacheEntity(info.Entity);
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

        void UncacheEntity(Entity etty)
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
            EOSSubManagers.Lobbies.RemoveNotifyMemberStatusReceived(OnMemberStatusReceived);
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
        /// Spawns a local player to the world.
        /// </summary>
        internal void SpawnLocalPlayer()
        {
            string uid = LOCAL_PLAYER_ID;
            string displayName = LOCAL_PLAYER_NAME;
            
            if (EOSSubManagers.Auth.IsLoggedIn)
            {
                /// getting user id has zero chance to fail if we're logged in
                uid = Game.EOS.GetLocalUserId().ToString();
                displayName = EOSSubManagers.UserInfo.LocalUserDisplayName;
            }

            Game.Entity.Spawn<Player>("player", ValidateWorldSpawn(currentSaveData.WorldSpawn), (info) =>
            {
                Player player = info.Entity;

                this._playerIdSaves.TryGetValue(uid, out var playerSave);
                player.InitializeAsPlayer(playerSave, isLocalPlayer: true);
                player.DisplayName = displayName;

                _playerEntities[uid] = player;
                localPlayer = player;
                OnPlayerJoined(player);
            });

            Game.Console.LogInfo($"[World]: {displayName} has entered the world");
        }

        /// <summary>
        /// Spawneth a player.
        /// This method should only be called on the server. 
        /// </summary>
        /// <param name="userId">User id of to be owner</param>
        internal void SpawnPlayer_ServerMethod(ProductUserId userId)
        {
            Game.Entity.Spawn<Player>("player", position: ValidateWorldSpawn(currentSaveData.WorldSpawn), onCompleted: (info) =>
            {
                var player = info.Entity;
                var uid = userId.ToString();
                
                if (EOSSubManagers.Transport.GetEOSTransport().TryGetClientIdMapping(userId, out ulong ownerClientId))
                {
                    player.NetworkObject.SpawnAsPlayerObject(ownerClientId);

                    _playerEntities[uid] = player;
                    if (ownerClientId == 0) /// host's player,
                    {
                        localPlayer = player;
                    }
                    OnPlayerJoined(player);
                }
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

        public Player GetLocalPlayer()
        {
            return localPlayer;
        }

        public async void SaveWorldAsync()
        {
            await Task.Yield();
            
            SaveWorld();
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

            var manifest = new WorldManifest()
            {
                WorldName = saveData.WorldName,
                CreatedDate = saveData.CreatedDate,
                LastPlayedDate = saveData.LastPlayedDate,
                LevelId = saveData.LevelId,
                OwnerId = saveData.OwnerId,
            };
            File.WriteAllText(Path.Join(this.worldpath, "world.manifest"), JsonConvert.SerializeObject(manifest));
            
            var elapsedTime = UnityEngine.Time.time - time; 
            Game.Console.LogInfo($"[World]: World saved. Took {elapsedTime:0.##} ms");
            Debug.Log($"[World]: World saved to '{filepath}'");
        }

        public void ExitWorld(bool save = true)
        {
            DeinitializeEvents();
            Cleanup();
            
            if (save)
            {
                SaveWorld();
            }
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
            
            this.currentSaveData = saveData;
            _hasValidSaveData = true;
            timeController.InitializeFromSave(saveData);
            ReadPlayerData();
            LoadObjects();
            LoadEntities();
        }

        public WorldSaveData WriteSaveData()
        {
            currentSaveData.LastPlayedDate = DateTime.Now.ToShortDateString();
            
            if (NetworkManager.Singleton.IsListening && NetworkManager.Singleton.IsServer)
            {
                currentSaveData.OwnerId = Game.EOS.GetProductUserId().ToString();
            }
            else
            {
                currentSaveData.OwnerId = LOCAL_PLAYER_ID;
            }

            SaveTimeState();
            SavePlayerData();
            SaveObjects();
            SaveEntities();

            return currentSaveData;
        }

        #endregion


        void ReadPlayerData()
        {
            var playerDataPath = Path.Join(this.worldpath, "playerdata");
            if (!Directory.Exists(playerDataPath)) Directory.CreateDirectory(playerDataPath);

            foreach (var file in Directory.EnumerateFiles(playerDataPath, "*.dat"))
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

        void SaveTimeState()
        {
            currentSaveData.Day = Time.CurrentDay;
            currentSaveData.Hour = Time.Hour;
            currentSaveData.Minute = Time.Minute;
            currentSaveData.Second = Time.Second;
        }

        void SavePlayerData()
        {
            foreach (var kv in _playerEntities)
            {
                string uid = kv.Key;
                Player player = kv.Value;
                PlayerSaveData psd = player.WriteSaveData();
                
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

        void SaveDataForPlayer(string uid)
        {
            if (_playerEntities.TryGetValue(uid, out var player))
            {
                var psd = player.WriteSaveData();
                _playerIdSaves[uid] = psd;
            }
        }

        Vector3 ValidateWorldSpawn(System.Numerics.Vector3 coords)
        {
            if (SaveData.FieldIsNull(coords)) coords = new(0f, 65f, 0f);
            if (Physics.Raycast(new Vector3(coords.X, 300f, coords.Z), -Vector3.up, out var hit, 999f))
                return new(coords.X, hit.point.y, coords.Z);
            return Vector3Ext.FromNumerics(coords);
        }
    }
}
