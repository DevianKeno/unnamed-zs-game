using System;
using System.IO;
using System.Threading.Tasks;
using System.Text;

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

namespace UZSG.Systems
{
    public enum Result { Success, Failed }

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
        public bool IsInWorld { get; private set; } = false;
        public bool IsPaused => IsInWorld && currentWorld.IsPaused;

        #region WorldManager Events
        /// <summary>
        /// Called after the World has been successfully initialized.
        /// </summary>
        public event Action OnDoneLoadWorld;
        public event Action OnExitWorld;

        #endregion

        TextMeshProUGUI loadingMessage;
        PauseMenuWindow pauseMenuWindow;
        InputAction pauseInput;

        #region Initialization

        void Awake()
        {
            if (currentWorld != null) IsInWorld = true;
        }

        internal void Initialize()
        {
            RetrieveSavedWorlds();
            SetupPauseMenu();
        }

        void SetupPauseMenu()
        {
            pauseMenuWindow = Game.UI.Create<PauseMenuWindow>("Pause Menu Window", show: false);
            pauseInput = Game.Main.GetInputAction("Pause", "World");
            pauseInput.performed += OnInputPause;
        }

        #endregion


        void RetrieveSavedWorlds()
        {
            
        }


        #region Event callbacks

        void OnInputPause(InputAction.CallbackContext context)
        {
            /// Pause only when no other windows are visible
            if (!IsInWorld || Game.UI.HasActiveWindow) return;

            if (CurrentWorld.IsPaused)
            {
                UnpauseCurrentWorld();
            }
            else
            {
                PauseCurrentWorld();
            }
        }

        #endregion

        public void PauseCurrentWorld()
        {
            currentWorld.Pause();
            pauseMenuWindow.Show();
        }

        public void UnpauseCurrentWorld()
        {
            pauseMenuWindow.Hide();
            currentWorld.Unpause();
        }

        public struct CreateWorldResult
        {
            public string Savepath { get; set; }
            public Result Result { get; set; }
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
                CreatedDate = options.CreatedDate.ToShortDateString(),
                LastModifiedDate = options.LastModifiedDate.ToShortDateString(),
                LevelId = options.MapId,
                OwnerId = options.OwnerId,
            };
            File.WriteAllText(savepath, JsonConvert.SerializeObject(saveData));

            this.onCreateWorldCompleted?.Invoke(new()
            {
                Savepath = savepath,
                Result = Result.Success,
            });
            this.onCreateWorldCompleted = null;
        }

        public struct LoadWorldOptions
        {
            public string OwnerId { get; set; }
            public string Filepath { get; set; }
            public WorldSaveData WorldSaveData { get; set; }
        }
        public struct LoadWorldResult
        {
            public Result Result { get; set; }
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
                        currentWorld.filepath = options.Filepath;
                        IsInWorld = true;
                        currentWorld.SetOwnerId(options.OwnerId);

                        InitializeWorldLoaded();

                        this.onLoadWorldCompleted?.Invoke(new()
                        {
                            Result = Result.Success
                        });
                        this.onLoadWorldCompleted = null;

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
                Result = Result.Failed
            });
            this.onLoadWorldCompleted = null;
        }
        
        public void LoadWorldFromLevelDat(string filepath, Action<LoadWorldResult> onLoadWorldCompleted = null)
        {
            this.onLoadWorldCompleted += onLoadWorldCompleted;

            if (!Directory.Exists(Path.GetDirectoryName(filepath)))
            {
                Debug.Log($"World in path '{filepath}' does not exist");
                this.onLoadWorldCompleted?.Invoke(new()
                {
                    Result = Result.Failed
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

        /// <summary>
        /// Executed after world initialization is done.
        /// </summary>
        void InitializeWorldLoaded()
        {
            if (EOSSubManagers.Lobbies.IsInLobby)
            {
                var currentLobby = EOSSubManagers.Lobbies.CurrentLobby;
                currentLobby.RequestPlayerSaveData(Game.EOS.GetProductUserId(), OnRequestPlayerSaveDataCompleted);
            }
            else
            {
                if (EOSSubManagers.Auth.IsLoggedIn)
                {
                    CurrentWorld.JoinPlayerByUserInfo(EOSSubManagers.UserInfo.GetLocalUserInfo());
                }
                else
                {
                    CurrentWorld.JoinLocalPlayer();
                }
            }
            
            pauseInput.Enable();
        }

        void OnRequestPlayerSaveDataCompleted(byte[] data, Epic.OnlineServices.Result result)
        {
            if (result == Epic.OnlineServices.Result.Success)
            {
                var contents = Encoding.UTF8.GetString(data);
                var psd = JsonConvert.DeserializeObject<PlayerSaveData>(contents);

                var localUserInfo = EOSSubManagers.UserInfo.GetLocalUserInfo();
                CurrentWorld.JoinPlayerByUserInfo(localUserInfo, psd);
            }
            else
            {
                Game.Console.LogDebug("Failed to request player save data");
            }
        }


        public void ExitCurrentWorld()
        {
            if (!IsInWorld) return;

            Game.Console.LogInfo("[World]: Exiting world...");
            pauseMenuWindow.Hide();
            
            /// TODO: file size too big
            // var screenshotPath = Path.Join(CurrentWorld.worldpath, "world.png");
            // ScreenCapture.CaptureScreenshot(screenshotPath, );

            var loadOptions = new Game.LoadSceneOptions()
            {
                SceneToLoad = "LoadingScreen",
                Mode = LoadSceneMode.Additive,
                ActivateOnLoad = true,
            };
            Game.Main.LoadScene(loadOptions, onLoadSceneCompleted: OnLoadingScreenLoad);
        }
        
        async void OnLoadingScreenLoad()
        {
            await Task.Yield();
            
            currentWorld.Deinitialize();
            currentWorld.SaveWorld();
            currentWorld = null;
            IsInWorld = false;
            pauseInput.Disable();
            OnExitWorld?.Invoke();
            
            var loadOptions = new Game.LoadSceneOptions()
            {
                SceneToLoad = "TitleScreen",
                Mode = LoadSceneMode.Single,
                ActivateOnLoad = true,
            };
            Game.Main.LoadScene(loadOptions);
        }

        /// <summary>
        /// Constructs a world save file from saveData. Used primarily to construct worlds from received data in multiplayer.
        /// </summary>
        public string ConstructWorldFromExternal(WorldSaveData saveData)
        {
            var settings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var save = JsonConvert.SerializeObject(saveData, Formatting.Indented, settings);
            var externalWorldsDirectory = Path.Join(Application.persistentDataPath, "ExternalWorlds", saveData.WorldName);
            if (!Directory.Exists(externalWorldsDirectory)) Directory.CreateDirectory(externalWorldsDirectory);

            var filepath = Path.Join(externalWorldsDirectory, "level.dat");
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
                Game.Console.LogError($"Failed to deserialize level data for world '{Path.GetFileName(filepath)}'!");
                return null;
            }
        }
    }
}