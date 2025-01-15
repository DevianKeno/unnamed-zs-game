using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

using PlayEveryWare.EpicOnlineServices;

using UZSG.UI;
using UZSG.EOS;

namespace UZSG.Systems
{
    public class Game : MonoBehaviour
    {
        public static Game Main { get; private set; }

        [SerializeField] int targetFramerate = -1;

        public const uint VERSION = 1;
        public const uint BUILD_NUMBER = 0;
        public const uint PATCH_NUMBER = 0;


        #region Core

        static Console console;
        public static Console Console => console;
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
        static TimeManager timeManager;
        public static TimeManager Time => timeManager;

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
        public bool IsOnline { get; internal set; } = false;
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
                UIManager = GetComponentInChildren<UIManager>();
                inputManager = GetComponentInChildren<InputManager>();
                audioManager = GetComponentInChildren<AudioManager>();
                timeManager = GetComponentInChildren<TimeManager>();
                worldManager = GetComponentInChildren<WorldManager>();
                
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

            UIManager.Initialize();
            audioManager.Initialize();
            worldManager.Initialize();

            #region These should be only initialized upon entering worlds
            /// These are run only on scenes that are already "worlds"           
            tickSystem.Initialize();
            attrManager.Initialize();
            itemManager.Initialize();
            objectsManager.Initialize();
            entityManager.Initialize();
            particleManager.Initialize();
            recipeManager.Initialize();

            IsOnline = EOSManager.Instance.GetEOSPlatformInterface() != null;
            if (IsOnline)
            {
                Debug.Log("Currently online");
            }
            else
            {
                Debug.Log("Currently offline");
            }
        
            #endregion

            OnLateInit?.Invoke();
            Console.Log("Press F1 to hide/show console");
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

        event Action onLoadSceneCompleted;
        public void LoadScene(LoadSceneOptions options, Action onLoadSceneCompleted = null)
        {
            try
            {
                StartCoroutine(LoadSceneCoroutine(options, onLoadSceneCompleted));
            }
            catch (NullReferenceException e)
            {
                Debug.LogError($"Scene does not exist" + e);
            }
        }

        public void UnloadScene(string name, Action onLoadSceneCompleted = null)
        {
            try
            {
                SceneManager.UnloadSceneAsync(name);
            } catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public void ActivateScene(string name)
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(name));
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

        IEnumerator LoadSceneCoroutine(LoadSceneOptions options, Action onLoadSceneCompleted)
        {
            this.onLoadSceneCompleted += onLoadSceneCompleted;

            var asyncOp = SceneManager.LoadSceneAsync(options.SceneToLoad, options.Mode);
            while (asyncOp.progress < 0.9f)
            {
                yield return null;
            }
            asyncOp.allowSceneActivation = true;

            this.onLoadSceneCompleted?.Invoke();
            this.onLoadSceneCompleted = null;
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
    }
}
