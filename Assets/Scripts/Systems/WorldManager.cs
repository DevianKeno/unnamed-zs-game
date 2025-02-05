using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;

using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

using Newtonsoft.Json;
using TMPro;

using UZSG.Data;
using UZSG.EOS;
using UZSG.Saves;
using UZSG.UI;
using UZSG.Worlds;

namespace UZSG.Systems
{
    public class WorldManager : MonoBehaviour, IInitializeable
    {
        public const string WORLDS_FOLDER = "Worlds";
        public const string WORLD_SCREENSHOT_EXTENSION = ".png";
        public const int WORLD_SCREENSHOT_WIDTH = 576;
        public const int WORLD_SCREENSHOT_HEIGHT = 324;

        bool _isInitialized;
        public bool IsInitialized => _isInitialized;

        WorldSaveData currentSaveData;
        [SerializeField] World currentWorld = null;
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


        public struct CreateWorldResult
        {
            public string FilePath { get; set; }
            public Result Result { get; set; }
        }
        public delegate void OnCreateWorldCompletedCallback(CreateWorldResult result);
        public void CreateWorld(ref CreateWorldOptions options, OnCreateWorldCompletedCallback onCompleted = null)
        {
            var worldPath = Path.Join(Application.persistentDataPath, WorldManager.WORLDS_FOLDER, options.WorldName); 
            if (!Directory.Exists(worldPath)) Directory.CreateDirectory(worldPath);

            var manifest = new WorldManifest()
            {
                WorldName = options.WorldName,
                CreatedDate = options.CreatedDate.ToShortDateString(),
                LastPlayedDate = options.LastModifiedDate.ToShortDateString(),
                LevelId = options.MapId,
                OwnerId = options.OwnerId,
            };
            File.WriteAllText(Path.Join(worldPath, "world.manifest"), JsonConvert.SerializeObject(manifest));

            var saveData = new WorldSaveData
            {
                WorldName = options.WorldName,
                Seed = options.Seed,
                CreatedDate = options.CreatedDate.ToShortDateString(),
                LastPlayedDate = options.LastModifiedDate.ToShortDateString(),
                LevelId = options.MapId,
                OwnerId = options.OwnerId,
            };
            var datPath = Path.Join(worldPath, "level.dat");
            File.WriteAllText(datPath, JsonConvert.SerializeObject(saveData));

            onCompleted(new()
            {
                FilePath = datPath,
                Result = Result.Success,
            });
        }

        event OnLoadWorldCompletedCallback onLoadWorldCompleted;
        public delegate void OnLoadWorldCompletedCallback(LoadWorldResult result);
        
        public struct LoadWorldOptions
        {
            public string OwnerId { get; set; }
            public string Filepath { get; set; }
            public WorldSaveData WorldSaveData { get; set; }
        }

        public struct LoadWorldResult
        {
            public World World { get; set; }
            public Result Result { get; set; }
        }

        public async void LoadWorldFromFilepathAsync(string filepath, OnLoadWorldCompletedCallback onCompleted)
        {
            await Task.Yield();
            await LoadWorldFromFilepath(filepath, onCompleted);
        }

        async Task LoadWorldFromFilepath(string filepath, OnLoadWorldCompletedCallback onCompleted)
        {
            onLoadWorldCompleted -= onCompleted;
            onLoadWorldCompleted += onCompleted;

            var resultFailed = new LoadWorldResult() { Result = Result.Failed };

            if (!Directory.Exists(Path.GetDirectoryName(filepath)))
            {
                Debug.LogError($"World in path '{filepath}' does not exist");
                InvokeLoadLevelFailed();
                return;
            }
            
            currentSaveData = JsonConvert.DeserializeObject<WorldSaveData>(File.ReadAllText(filepath));
            if (currentSaveData == null)
            {
                Debug.LogError($"Error loading world {currentSaveData.WorldName}!");
                Debug.LogError($"Error loading world. Invalid save data");
                InvokeLoadLevelFailed();
                return;
            }

            if (currentSaveData == null)
            {
                Game.Console.LogError($"Cannot load a world if save data is null");
                InvokeLoadLevelFailed();
                return;
            }
            
            var data = Resources.Load<LevelData>($"Data/Levels/{currentSaveData.LevelId}");
            if (data == null || data.Scene == null || !data.Scene.IsSet())
            {
                Game.Console.LogError($"Level does not exist");
                InvokeLoadLevelFailed();
                return;
            }
            Game.Console.Assert(!string.IsNullOrEmpty(data.SceneName), $"SceneName property for LevelData '{data.name}' is null or empty! Please set the scene name.");

            if (NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneEvent;
                NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
                
                if (NetworkManager.Singleton.IsServer)
                {
                    NetworkManager.Singleton.SceneManager.LoadScene(data.SceneName, LoadSceneMode.Additive);
                }
            }
            else /// offline
            {     
                var asyncOp = Addressables.LoadSceneAsync(data.Scene, LoadSceneMode.Additive);
                while (!asyncOp.IsDone)
                {
                    await Task.Yield();
                }

                if (asyncOp.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"Error loading level scene '{currentSaveData.LevelId}'");
                    InvokeLoadLevelFailed();
                    return;
                }

                IsInWorld = FindWorldObject();
                InvokeLoadLevelSuccess();
            }
            
            void InvokeLoadLevelFailed()
            {
                onLoadWorldCompleted?.Invoke(resultFailed);
                onLoadWorldCompleted = null;
            }
        }

        void OnSceneEvent(SceneEvent sceneEvent)
        {
            if (sceneEvent.SceneEventType == SceneEventType.LoadComplete)
            {
                IsInWorld = FindWorldObject();
                InvokeLoadLevelSuccess();
            }
        }

        void InvokeLoadLevelSuccess()
        {
            onLoadWorldCompleted?.Invoke(new LoadWorldResult()
            {
                World = this.currentWorld,
                Result = Result.Success,
            });
            onLoadWorldCompleted = null;
        }

        bool FindWorldObject()
        {
            currentWorld = FindObjectOfType<World>();
            if (currentWorld == null)
            {
                /// TODO: instantiate new World prefab
                Debug.LogError($"Error loading world {currentSaveData.WorldName}!");
                Debug.LogError($"Level '{currentSaveData.LevelId}' does not have any objects containing a World component");
            }
            return currentWorld != null; 
        }

        public void InitializeWorld()
        {
            if (currentSaveData == null || currentWorld == null)
            {
                Game.Console.LogError("Failed to initialize world");
                Game.Console.Assert(currentSaveData != null, $"World save data is null");
                Game.Console.Assert(currentWorld != null, $"Current world is null");
                return;
            }
            
            Game.Console.StartListenForLocalPlayer();
            Game.Particles.InitializePooledParticles();
            currentWorld.Initialize(currentSaveData);
            pauseInput.Enable();
        }


        #region Public methods

        /// <summary>
        /// Gets the current world. <c>null</c> if no world is loaded yet.
        /// </summary>
        public World GetWorld()
        {
            return currentWorld;
        }
        
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

        public void ExitCurrentWorld()
        {
            if (!IsInWorld) return;

            pauseMenuWindow.Hide();
            
            StartCoroutine(TakeScreenshotCoroutine());

            var loadOptions = new Game.LoadSceneOptions()
            {
                SceneToLoad = "LoadingScreen",
                Mode = LoadSceneMode.Additive,
                ActivateOnLoad = true,
            };
            Game.Main.LoadSceneAsync(loadOptions, onCompleted: OnLoadingScreenLoaded);
        }

        async void OnLoadingScreenLoaded()
        {
            await Task.Yield();
            
            currentWorld.ExitWorld();
            currentWorld = null;
            IsInWorld = false;
            pauseInput.Disable();
            OnExitWorld?.Invoke();
            
            if (EOSSubManagers.Lobbies.IsHosting)
            {
                EOSSubManagers.Lobbies.LeaveCurrentLobby();
            }
            
            var loadOptions = new Game.LoadSceneOptions()
            {
                SceneToLoad = "TitleScreen",
                Mode = LoadSceneMode.Single,
                ActivateOnLoad = true,
            };
            Game.Main.LoadSceneAsync(loadOptions);
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

        #endregion
        

        IEnumerator TakeScreenshotCoroutine()
        {
            var mainCamera = Camera.main;

            // Create full resolution RenderTexture
            RenderTexture fullResRT = new(Screen.width, Screen.height, 24);
            RenderTexture downscaleRT = new(WORLD_SCREENSHOT_WIDTH, WORLD_SCREENSHOT_HEIGHT, 24);
            
            mainCamera.targetTexture = fullResRT;
            mainCamera.Render();

            // Downscale by blitting the fullResRT to downscaleRT
            Graphics.Blit(fullResRT, downscaleRT);

            // Read pixels from the downscaled RenderTexture
            Texture2D screenshot = new(WORLD_SCREENSHOT_WIDTH, WORLD_SCREENSHOT_HEIGHT, TextureFormat.RGB24, false);
            RenderTexture.active = downscaleRT;
            screenshot.ReadPixels(new Rect(0, 0, WORLD_SCREENSHOT_WIDTH, WORLD_SCREENSHOT_HEIGHT), 0, 0);
            screenshot.Apply();

            // Cleanup
            mainCamera.targetTexture = null;
            RenderTexture.active = null;
            fullResRT.Release();
            downscaleRT.Release();
            Destroy(fullResRT);
            Destroy(downscaleRT);

            // Save the screenshot
            File.WriteAllBytes(Path.Join(CurrentWorld.worldpath, $"world{WORLD_SCREENSHOT_EXTENSION}"), screenshot.EncodeToPNG());
            Destroy(screenshot);

            yield return null;
        }
    }
}