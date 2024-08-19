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

        public int TargetFramerate = -1;


        #region Core

        static Console console;
        public static Console Console => console;
        static UIManager UIManager;
        public static UIManager UI => UIManager;
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
        public bool IsOnline
        {
            get
            {
                return EOSManager.Instance.GetEOSPlatformInterface() != null;
            }
        }
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
                Destroy(this);
            }
            else
            {
                Main = this;

                console = GetComponentInChildren<Console>();                
                UIManager = GetComponentInChildren<UIManager>();
                audioManager = GetComponentInChildren<AudioManager>();
                timeManager = GetComponentInChildren<TimeManager>();
                worldManager = GetComponentInChildren<WorldManager>();
                
                tickSystem = GetComponentInChildren<TickSystem>();
                attrManager = GetComponentInChildren<AttributesManager>();
                particleManager = GetComponentInChildren<ParticleManager>();
                itemManager = GetComponentInChildren<ItemManager>();
                recipeManager = GetComponentInChildren<RecipeManager>();
                entityManager = GetComponentInChildren<EntityManager>();
                objectsManager = GetComponentInChildren<ObjectsManager>();
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
            Application.targetFrameRate = TargetFramerate;
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
            entityManager.Initialize();
            particleManager.Initialize();
            recipeManager.Initialize();

            #endregion            

            OnLateInit?.Invoke();
            
            Console.Log("Press F1 to hide/show console");
        }

        
        #region Public methods

        #region Scene management

        event Action OnLoadSceneCompleted;

        public void LoadScene(string sceneName, bool playTransition = true, float delayInSeconds = 0f, Action OnLoadSceneCompletedCallback = null)
        {
            try
            {
                OnLoadSceneCompleted += OnLoadSceneCompletedCallback;
                // StartCoroutine(Load(sceneName, playTransition, Game.UI.TransitionOptions.AnimationSpeed / 2f));
            }
            catch (NullReferenceException e)
            {
                Debug.LogError($"Scene does not exist" + e);
            }
        }

        IEnumerator Load(string scene, bool playTransition, float delayInSeconds)
        {
            var asyncOp = SceneManager.LoadSceneAsync(scene);
            asyncOp.allowSceneActivation = false;

            while (asyncOp.progress < 0.9f)
            {
                yield return null;
            }

            if (playTransition)
            {
                // Game.UI.PlayTransitionHalf(() =>
                // {
                //     asyncOp.allowSceneActivation = true;
                //     Game.UI.PlayTransitionEnd(() =>
                //     {
                //         OnLoadSceneCompleted?.Invoke();
                //         OnLoadSceneCompleted = null;
                //     });
                // });
            }
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
                if (enable) action.Enable();
                else action.Disable();
            }
            return inputs;
        }

        #endregion

        #endregion
    }
}
