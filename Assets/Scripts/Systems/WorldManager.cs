using System;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus;
using UZSG.Levels;
using UZSG.Saves;
using UZSG.Data;
using UZSG.Worlds;
using System.Threading.Tasks;
using UZSG.Entities;
using PlayEveryWare.EpicOnlineServices;
using UZSG.EOS;

namespace UZSG.Systems
{
    public enum Status { Success, Failed }

    public struct CreateWorldOptions
    {
        public string WorldName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }

    public class WorldManager : MonoBehaviour, IInitializeable
    {
        bool _isInitialized;
        public bool IsInitialized => _isInitialized;
        [SerializeField] World currentWorld;
        /// <summary>
        /// The currently loaded world.
        /// </summary>
        public World CurrentWorld => currentWorld;
        public bool HasWorld { get; private set; } = false;

        /// <summary>
        /// Called after the World has been successfully initialized.
        /// </summary>
        public event Action OnDoneInit;

        void Awake()
        {
            if (currentWorld != null) HasWorld = true;
        }

        internal void Initialize()
        {
            RetrieveSavedWorlds();

            currentWorld?.Initialize(null);///testing

            OnDoneInit?.Invoke();
        }

        void RetrieveSavedWorlds()
        {
            
        }

        public struct CreateWorldResult
        {
            public string Savepath { get; set; }
            public Status Status { get; set; }
        }
        event Action<CreateWorldResult> onCreateWorldCompleted;
        public void CreateWorld(ref CreateWorldOptions options, Action<CreateWorldResult> onCreateWorldCompleted = null)
        {
            this.onCreateWorldCompleted += onCreateWorldCompleted;

            var worldpath = Path.Join(Application.persistentDataPath, "SavedWorlds", options.WorldName); 
            Directory.CreateDirectory(worldpath);
            var savepath = Path.Join(worldpath, "level.dat");
            var saveData = new WorldSaveData
            {
                Name = options.WorldName,
                CreatedDate = options.CreatedDate,
                LastModifiedDate = options.LastModifiedDate,
                LevelId = "DEMO" /// replace with Level Id
            };
            File.WriteAllText(savepath, JsonConvert.SerializeObject(saveData));

            this.onCreateWorldCompleted?.Invoke(new()
            {
                Savepath = savepath,
                Status = Status.Success,
            });
            this.onCreateWorldCompleted = null;
        }

        public struct LoadWorldResult
        {
            public Status Status { get; set; }
        }
        event Action<LoadWorldResult> onLoadWorldCompleted;
        public async void LoadWorldAsync(WorldSaveData saveData, Action<LoadWorldResult> onLoadWorldCompleted = null)
        {
            this.onLoadWorldCompleted += onLoadWorldCompleted;

            if (saveData != null)
            {
                var asset = Resources.Load<GameObject>($"Prefabs/WORLD");
                var go = Instantiate(asset, transform);
                go.name = saveData.Name + " (World)";
                currentWorld = go.GetComponent<World>();
                
                await LoadLevelAsync(saveData.LevelId);

                currentWorld.Initialize(saveData);

                this.onLoadWorldCompleted?.Invoke(new()
                {
                    Status = Status.Success
                });
                this.onLoadWorldCompleted = null;
                JoinLocalPlayer(); /// this should not be here
            }
            else
            {
                Debug.LogError($"WorldSaveData cannot be null");
                this.onLoadWorldCompleted?.Invoke(new()
                {
                    Status = Status.Failed
                });
                this.onLoadWorldCompleted = null;
            }
        }
        
        public async void LoadWorldAsync(string filepath, Action<LoadWorldResult> onLoadWorldCompleted = null)
        {
            this.onLoadWorldCompleted += onLoadWorldCompleted;

            if (!Directory.Exists(Path.GetDirectoryName(filepath)))
            {
                Debug.Log($"World in path '{filepath}' does not exist");
                this.onLoadWorldCompleted?.Invoke(new()
                {
                    Status = Status.Failed
                });
                this.onLoadWorldCompleted = null;
                return;
            }
            
            var saveData = JsonConvert.DeserializeObject<WorldSaveData>(File.ReadAllText(filepath));
            if (saveData != null)
            {
                var asset = Resources.Load<GameObject>($"Prefabs/WORLD");
                var go = Instantiate(asset, transform);
                go.name = saveData.Name + " (World)";
                currentWorld = go.GetComponent<World>();
                
                await LoadLevelAsync(saveData.LevelId);

                currentWorld.Initialize(saveData);

                this.onLoadWorldCompleted?.Invoke(new()
                {
                    Status = Status.Success
                });
                this.onLoadWorldCompleted = null;
            }
            else
            {
                Debug.LogError($"Encountered an error when deserializing world save data at '{filepath}'");
                this.onLoadWorldCompleted?.Invoke(new()
                {
                    Status = Status.Failed
                });
                this.onLoadWorldCompleted = null;
            }
        }
        
        public async Task<Level> LoadLevelAsync(string levelId)
        {
            Level level = null;
            // var data = Resources.Load<LevelData>($"Data/Levels/{levelId}");
            // if (data != null)
            // {
                var asyncOp = Addressables.LoadAssetAsync<GameObject>("Assets/AddressableAssets/Levels/DEMO.prefab");
                await asyncOp.Task;
                if (asyncOp.Status == Succeeded)
                {
                    var go = Instantiate(asyncOp.Result, currentWorld.transform);
                    level = go.GetComponent<Level>();
                    go.name = levelId + " (Level)";
                }
                else if (asyncOp.Status == Failed)
                {
                    Debug.LogError($"Encountered an error when loading level id '{levelId}'");
                }
            // }
            // else
            // {
            //     Debug.Log($"There is no data that exists for level id '{levelId}'");
            // }
                    
            return level;
        }

        void JoinLocalPlayer()
        {
            if (Game.Main.IsOnline)
            {
                Game.World.CurrentWorld.JoinPlayerId(EOSSubManagers.UserInfo.GetLocalUserInfo());
            }
            else
            {
                
            }
        }
    }
}