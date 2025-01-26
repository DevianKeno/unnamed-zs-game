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

        public delegate void OnLoadWorldCompletedCallback(LoadWorldResult result);
        
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
        
        public async void LoadWorldFromFilepath(string filepath, OnLoadWorldCompletedCallback callback = null)
        {
            await LoadWorldFromFilepathAsync(filepath, callback);
        }

        public async Task LoadWorldFromFilepathAsync(string filepath, OnLoadWorldCompletedCallback callback = null)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filepath)))
            {
                Debug.Log($"World in path '{filepath}' does not exist");
                callback(new LoadWorldResult()
                {
                    Result = Result.Failed
                });
                return;
            }
            
            var saveData = JsonConvert.DeserializeObject<WorldSaveData>(File.ReadAllText(filepath));
            var options = new LoadWorldOptions()
            {
                OwnerId = saveData.OwnerId,
                WorldSaveData = saveData,
            };

            await LoadWorldAsync(options, callback);
        }

        public async void LoadWorld(LoadWorldOptions options, OnLoadWorldCompletedCallback callback = null)
        {
            await LoadWorldAsync(options, callback);
        }

        public async Task LoadWorldAsync(LoadWorldOptions options, OnLoadWorldCompletedCallback callback = null)
        {
            try
            {
                if (options.WorldSaveData == null)
                {
                    Debug.LogError($"Error loading world. Save data is null");
                    callback(new LoadWorldResult()
                    {
                        Result = Result.Failed
                    });
                }

                if (TryLoadLevelScene(options.WorldSaveData.LevelId, out var asyncOp))
                {
                    await asyncOp.Task;
                    await Task.Delay(100); /// maybe just enough to give ample time
                    
                    currentWorld = FindObjectOfType<World>();
                    currentWorld.Initialize(options.WorldSaveData);
                    currentWorld.SetOwnerId(options.OwnerId);
                    currentWorld.filepath = options.Filepath;
                    IsInWorld = true;

                    Game.Particles.InitializePooledParticles();
                    InitializeWorldLoaded();

                    callback(new LoadWorldResult()
                    {
                        Result = Result.Success
                    });
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            Debug.LogError($"Unexpected error occured when loading world");
            callback(new LoadWorldResult()
            {
                Result = Result.Failed
            });
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
        
        bool TryLoadLevelScene(string levelId, out AsyncOperationHandle handle)
        {
            handle = default;
            var data = Resources.Load<LevelData>($"Data/Levels/{levelId}");
            if (data == null || data.Scene == null || !data.Scene.IsSet())
            {
                return false;
            }
            
            handle = Addressables.LoadSceneAsync(data.Scene, LoadSceneMode.Additive);
            return true;
        }

        /// <summary>
        /// Executed after world initialization is done.
        /// </summary>
        void InitializeWorldLoaded()
        {
            if (EOSSubManagers.Lobbies.IsInLobby)
            {
                if (EOSSubManagers.Lobbies.CurrentLobby.IsOwner(Game.EOS.GetProductUserId()))
                {
                    /// we are the server
                    Game.World.CurrentWorld.SpawnPlayer_mServer(Game.EOS.GetProductUserId());
                }
                else
                {
                    // EOSSubManagers.P2P.RequestPlayerSaveData(Game.EOS.GetProductUserId(), OnRequestPlayerSaveDataCompleted);
                    EOSSubManagers.P2P.RequestSpawnPlayer(EOSSubManagers.Lobbies.CurrentLobby.OwnerProductUserId, onCompleted: () => 
                    {
                        Game.Console.LogInfo("I think we're spawned on the server now");
                        // EOSSubManagers.P2P.RequestPlayerSaveData();
                    });
                }
            }
            else
            {
                if (EOSSubManagers.Auth.IsLoggedIn)
                {
                    /// getting local user info has zero chance to fail if we're logged in
                    // CurrentWorld.JoinPlayerByUserInfo(EOSSubManagers.UserInfo.GetLocalUserInfo()); 
                }
                else
                {
                    CurrentWorld.JoinLocalPlayer();
                }
            }
            
            pauseInput.Enable();
        }

        void OnRequestPlayerSaveDataCompleted(PlayerSaveData playerSaveData, Epic.OnlineServices.Result result)
        {
            var localUserInfo = EOSSubManagers.UserInfo.GetLocalUserInfo();
            if (result == Epic.OnlineServices.Result.Success)
            {
                // CurrentWorld.JoinPlayerByUserInfo(localUserInfo, playerSaveData);
            }
            else
            {
                Game.Console.LogDebug("Failed to request player save data");
                // CurrentWorld.JoinPlayerByUserInfo(localUserInfo, playerSaveData);
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
            if (saveData == null)
            {
                Game.Console.LogDebug($"Unable to construct world if save data is null");
                return string.Empty;
            }
            if (string.IsNullOrEmpty(saveData.WorldName))
            {
                Game.Console.LogDebug($"Unable to construct world if world name is null or empty");
                return string.Empty;
            }

            var serializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var contents = JsonConvert.SerializeObject(saveData, Formatting.Indented, serializerSettings);
            var externalWorldsDirectory = Path.Join(Application.persistentDataPath, "ExternalWorlds", saveData.WorldName);
            if (!Directory.Exists(externalWorldsDirectory)) Directory.CreateDirectory(externalWorldsDirectory);
            var filepath = Path.Join(externalWorldsDirectory, "level.dat");
            File.WriteAllText(filepath, contents);

            return filepath;
        }
        
        public WorldSaveData Deserialize(string filepath)
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