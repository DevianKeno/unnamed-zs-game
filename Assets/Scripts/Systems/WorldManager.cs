using System;
using System.IO;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using TMPro;

using UZSG.Saves;
using UZSG.Data;
using UZSG.Worlds;
using UZSG.EOS;
using UnityEngine.InputSystem;
using UZSG.UI;
using PlayEveryWare.EpicOnlineServices;

namespace UZSG.Systems
{
    public enum Status { Success, Failed }

    public struct CreateWorldOptions
    {
        public string WorldName { get; set; }
        public string MapId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string OwnerId { get; set; }
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
        public event Action OnDoneLoadWorld;
        public event Action OnExitWorld;

        TextMeshProUGUI loadingMessage;


        void Awake()
        {
            if (currentWorld != null) HasWorld = true;
        }

        internal void Initialize()
        {
            RetrieveSavedWorlds();

            currentWorld?.Initialize(null);///testing

            OnDoneLoadWorld?.Invoke();
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
                WorldName = options.WorldName,
                CreatedDate = options.CreatedDate,
                LastModifiedDate = options.LastModifiedDate,
                LevelId = options.MapId,
                OwnerId = options.OwnerId,
            };
            File.WriteAllText(savepath, JsonConvert.SerializeObject(saveData));

            this.onCreateWorldCompleted?.Invoke(new()
            {
                Savepath = savepath,
                Status = Status.Success,
            });
            this.onCreateWorldCompleted = null;
        }

        public struct LoadWorldOptions
        {
            public string OwnerId { get; set; }
            public WorldSaveData WorldSaveData { get; set; }
        }
        public struct LoadWorldResult
        {
            public Status Status { get; set; }
        }
        event Action<LoadWorldResult> onLoadWorldCompleted;
        public async void LoadWorld(LoadWorldOptions options, Action<LoadWorldResult> onLoadWorldCompleted = null)
        {
            this.onLoadWorldCompleted = null;
            this.onLoadWorldCompleted += onLoadWorldCompleted;

            try
            {
                if (options.WorldSaveData != null)
                {
                    if (TryLoadLevelSceneAsync(options.WorldSaveData.LevelId, out var asyncOp))
                    {
                        // while (!asyncOp.IsDone)
                        // {
                        //     LoadingScreenHandler.Instance.Progress = asyncOp.PercentComplete;
                        // }
                        // LoadingScreenHandler.Instance.Message = "Loading map...";
                        
                        await asyncOp.Task;

                        currentWorld = GameObject.FindWithTag("World").GetComponent<World>();
                        currentWorld.Initialize(options.WorldSaveData);
                        HasWorld = true;
                        currentWorld.SetOwnerId(options.OwnerId);

                        this.onLoadWorldCompleted?.Invoke(new()
                        {
                            Status = Status.Success
                        });
                        this.onLoadWorldCompleted = null;

                        InitializeWorldLoaded();
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            Debug.LogError($"Unexpected error occured when loading world");
            this.onLoadWorldCompleted?.Invoke(new()
            {
                Status = Status.Failed
            });
            this.onLoadWorldCompleted = null;
        }
        
        public void LoadWorld(string filepath, Action<LoadWorldResult> onLoadWorldCompleted = null)
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
            
            var options = new LoadWorldOptions()
            {
                OwnerId = GetLocalUserId(),
                WorldSaveData = JsonConvert.DeserializeObject<WorldSaveData>(File.ReadAllText(filepath)),
            };

            LoadWorld(options, onLoadWorldCompleted);
        }

        public string GetLocalUserId()
        {
            try
            {
                if (Game.Main.IsOnline)
                {
                    return EOSSubManagers.UserInfo.GetLocalUserInfo().UserId.ToString();
                }
                else
                {
                    return "local";
                }
            }
            catch
            {
                return "local";
            }
        }
        
        public bool TryLoadLevelSceneAsync(string levelId, out AsyncOperationHandle handle)
        {
            var data = Resources.Load<LevelData>($"Data/Levels/{levelId}");
            if (data != null)
            {
                handle = Addressables.LoadSceneAsync(data.Scene, LoadSceneMode.Additive);
                return true;
            }
            
            handle = default;
            return false;
        }

        void InitializeWorldLoaded()
        {
            JoinLocalPlayer(); /// this should not be here
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

        public void ExitCurrentWorld()
        {
            if (!HasWorld) return;

            Game.Console.Log("[World]: Exiting world...");
            Game.Main.LoadScene(
                new(){
                    SceneToLoad = "LoadingScreen",
                    Mode = LoadSceneMode.Additive,
                },
                onLoadSceneCompleted: () =>
                {
                    currentWorld.Deinitialize();
                    currentWorld.SaveWorld();

                    Game.Main.UnloadScene(currentWorld.gameObject.scene.name);
                    currentWorld = null;
                    HasWorld = false;
                    OnExitWorld?.Invoke();
                    
                    Game.Main.LoadScene(
                        new(){
                            SceneToLoad = "TitleScreen",
                            Mode = LoadSceneMode.Additive,
                        },
                        onLoadSceneCompleted: () =>
                        {
                            Game.Main.UnloadScene("LoadingScreen");
                        });
                });
                

        }

        public void Pause()
        {
            if (!HasWorld) return;
            currentWorld.Pause();
        }
        
        public void Unpause()
        {
            if (!HasWorld) return;
            currentWorld.Pause();
        }

        public string ConstructWorld(WorldSaveData saveData)
        {
            var settings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var save = JsonConvert.SerializeObject(saveData, Formatting.Indented, settings);
            var path = Application.persistentDataPath + $"/SavedWorlds/{saveData.WorldName}";
            var filepath = path + "/level.dat";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            File.WriteAllText(filepath, save);

            return filepath;
        }
        
        public WorldSaveData DeserializeWorldData(string filepath)
        {
            try
            {
                return JsonConvert.DeserializeObject<WorldSaveData>(File.ReadAllText(filepath));
            }
            catch
            {
                Game.Console.Error($"Failed to deserialize level data for world '{Path.GetFileName(filepath)}'!");
                return null;
            }
        }

        public bool IsPaused => currentWorld.IsPaused;

        // public async Task<Level> LoadLevelSceneAsync(string levelId)
        // {
        //     Level level = null;
        //     // var data = Resources.Load<LevelData>($"Data/Levels/{levelId}");
        //     // if (data != null)
        //     // {
        //         var asyncOp = Addressables.LoadAssetAsync<GameObject>("Assets/AddressableAssets/Levels/DEMO.prefab");
        //         await asyncOp.Task;
        //         if (asyncOp.Status == Succeeded)
        //         {
        //             var go = Instantiate(asyncOp.Result, currentWorld.transform);
        //             level = go.GetComponent<Level>();
        //             go.name = levelId + " (Level)";
        //         }
        //         else if (asyncOp.Status == Failed)
        //         {
        //             Debug.LogError($"Encountered an error when loading level id '{levelId}'");
        //         }
        //     // }
        //     // else
        //     // {
        //     //     Debug.Log($"There is no data that exists for level id '{levelId}'");
        //     // }

        //     return level;
        // }
    }
}