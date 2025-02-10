using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

using PlayEveryWare.EpicOnlineServices;

using UZSG.UI;
using UZSG.EOS;
using Epic.OnlineServices.Connect;
using System.Threading.Tasks;

namespace UZSG
{
    public class Game : MonoBehaviour, IConnectInterfaceEventListener
    {
        public static Game Main { get; private set; }

        [SerializeField] bool enableDebugMode = true;
        public bool EnableDebugMode => enableDebugMode;
        [SerializeField] int targetFramerate = -1;

        public const uint VERSION = 1;
        public const uint BUILD_NUMBER = 0;
        public const uint PATCH_NUMBER = 0;


        #region Core

        static Console console;
        public static Console Console => console;
        static LocalizationManager localizationManager;
        public static LocalizationManager Locale => localizationManager;
        static SettingsManager settingsManager;
        public static SettingsManager Settings => settingsManager;
        static UIManager UIManager;
        public static UIManager UI => UIManager;
        static InputManager inputManager;
        public static InputManager Input => inputManager;
        static AudioManager audioManager;
        public static AudioManager Audio => audioManager;
        /// <summary>
        /// EOS Interface singleton.
        /// Just a shortcut to 'EOSManager.Instance'
        /// </summary>
        public static EOSManager.EOSSingleton EOS => EOSManager.Instance;
        public static EOSSubManagers EOSManagers { get; private set; }
        static SavesManager savesManager;
        public static SavesManager Saves => savesManager;
        static WorldManager worldManager;
        public static WorldManager World => worldManager;
        static AttributesManager attrManager;
        public static AttributesManager Attributes => attrManager;
        static EntityManager entityManager;
        public static EntityManager Entity => entityManager;
        static ItemManager itemManager;
        public static ItemManager Items => itemManager;
        static RecipeManager recipeManager;
        public static RecipeManager Recipes => recipeManager;
        static ObjectsManager objectsManager;
        public static ObjectsManager Objects => objectsManager;

        #endregion


        #region World-entry

        static TickSystem tickSystem;
        public static TickSystem Tick => tickSystem;
        static ParticleManager particleManager;
        public static ParticleManager Particles => particleManager;

        #endregion

        
        /// <summary>
        /// Called after all Managers have been initialized.
        /// </summary>
        internal event Action OnLateInit;

        PlayerInput mainInput;
        public PlayerInput MainInput => mainInput;


        #region Public properties

        public bool IsAlive { get; private set; }
        public bool IsPaused { get; private set; }
        public bool IsOnline { get; private set; } = false;
        public bool IsHosting { get; private set; }
        public Scene CurrentScene
        {
            get
            {
                return SceneManager.GetActiveScene();
            }
        }

        #endregion

        void Awake()
        {
            DontDestroyOnLoad(this);

            if (Main != null && Main != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Main = this;

                console = GetComponentInChildren<Console>();
                localizationManager = GetComponentInChildren<LocalizationManager>();
                settingsManager = GetComponentInChildren<SettingsManager>();         
                UIManager = GetComponentInChildren<UIManager>();
                inputManager = GetComponentInChildren<InputManager>();
                audioManager = GetComponentInChildren<AudioManager>();
                worldManager = GetComponentInChildren<WorldManager>();
                savesManager = GetComponentInChildren<SavesManager>();
                
                tickSystem = GetComponentInChildren<TickSystem>();
                attrManager = GetComponentInChildren<AttributesManager>();
                objectsManager = GetComponentInChildren<ObjectsManager>();
                itemManager = GetComponentInChildren<ItemManager>();
                recipeManager = GetComponentInChildren<RecipeManager>();
                entityManager = GetComponentInChildren<EntityManager>();
                particleManager = GetComponentInChildren<ParticleManager>();
            }

            mainInput = GetComponent<PlayerInput>();
        }

        void Start()
        {
            IsAlive = true;
            InitializeManagers();
        }

        void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                Application.targetFrameRate = targetFramerate;
            }
        }

        void InitializeManagers()
        {
            /// The console is initialized first thing
            console.Initialize();
            localizationManager.Initialize();
            settingsManager.Initialize();
            UIManager.Initialize();
            audioManager.Initialize();
            worldManager.Initialize();
            EOSSubManagers.Initialize();

            #region These should be only initialized upon entering worlds
            /// These are run only on scenes that are already "worlds"           
            tickSystem.Initialize();
            attrManager.Initialize();
            itemManager.Initialize();
            objectsManager.Initialize();
            entityManager.Initialize();
            particleManager.Initialize();
            recipeManager.Initialize();

            #endregion

            OnLateInit?.Invoke();
            Console.LogInfo("Press F1 to hide/show console");
        }

        
        #region Public methods

        #region Scene management

        public class LoadSceneOptions
        {
            public string SceneToLoad { get; set; }
            public LoadSceneMode Mode { get; set; }
            public bool ActivateOnLoad { get; set; } = true;
            public float DelaySeconds { get; set; }
            public bool PlayTransition { get; set; }
            public SceneTransitionOptions TransitionOptions { get; set; }
        }

        public delegate void OnLoadSceneCallback();

        public async void LoadSceneAsync(LoadSceneOptions options, OnLoadSceneCallback onCompleted = null)
        {
            try
            {
                await Task.Delay((int) Math.Clamp(options.DelaySeconds, 0, options.DelaySeconds) * 1000);
                await LoadScene(options, onCompleted);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        Dictionary<string, Scene> unactivatedScenes = new();

        async Task LoadScene(LoadSceneOptions options, OnLoadSceneCallback onCompleted = null)
        {
            var asyncOp = SceneManager.LoadSceneAsync(options.SceneToLoad, options.Mode);
            asyncOp.allowSceneActivation = false;
            while (asyncOp.progress < 0.9f)
            {
                await Task.Yield();
            }
            if (options.ActivateOnLoad)
            {
                asyncOp.allowSceneActivation = true;
            }
            else
            {
                unactivatedScenes[options.SceneToLoad] = SceneManager.GetSceneByName(options.SceneToLoad);
            }

            onCompleted?.Invoke();
            return;
        }
        
        public void UnloadScene(string name, Action onUnlLoadSceneCompleted = null)
        {
            try
            {
                SceneManager.UnloadSceneAsync(name);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        public void ActivateScene(string name)
        {
            if (unactivatedScenes.ContainsKey(name))
            {
                SceneManager.SetActiveScene(unactivatedScenes[name]);
                unactivatedScenes.Remove(name);
            }
        }

        public class UnloadSceneOptions
        {
            public string Name { get; set; }
            public float DelaySeconds { get; set; }
            public bool PlayTransition { get; set; }
            public SceneTransitionOptions TransitionOptions { get; set; }
        }
        public void UnloadScene(UnloadSceneOptions options, Action onLoadSceneCompleted = null)
        {
            SceneManager.UnloadSceneAsync(options.Name);
        }

        #endregion

        
        #region Global input
        
        public InputAction GetInputAction(string actionName, string actionMapName)
        {
            return GetActionMap(actionMapName).FindAction(actionName);
        }

        public InputActionMap GetActionMap(string name)
        {
            return MainInput.actions.FindActionMap(name);
        }

        public Dictionary<string, InputAction> GetActions(string actionMapName)
        {
            var inputs = new Dictionary<string, InputAction>();
            var actionMap = GetActionMap(actionMapName);
            foreach (var action in actionMap.actions)
            {
                inputs[action.name] = action;
            }
            return inputs;
        }

        public Dictionary<string, InputAction> GetActionsFromMap(InputActionMap map, bool enable = true)
        {
            var inputs = new Dictionary<string, InputAction>();
            foreach (var action in map.actions)
            {
                inputs[action.name] = action;
                if (enable)
                {
                    action.Enable();
                }
                else
                {
                    action.Disable();
                }
            }
            return inputs;
        }

        #endregion

        #endregion

        public string GetVersionString()
        {
            return $"{VERSION}.{BUILD_NUMBER}.{PATCH_NUMBER}";
        }


        #region 

        public void OnConnectLogin(LoginCallbackInfo info)
        {
            this.IsOnline = info.ResultCode == Epic.OnlineServices.Result.Success;
            if (IsOnline) Game.Console.LogInfo($"Currently online");
            else Game.Console.LogInfo($"Currently offline");
        }

        #endregion
    }
}
