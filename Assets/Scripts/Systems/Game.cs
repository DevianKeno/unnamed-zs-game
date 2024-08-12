using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using PlayEveryWare.EpicOnlineServices;

using UZSG.UI;
using UZSG.EOS;
using UZSG.WorldEvents;

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
        static AttributesManager attrManager;
        public static AttributesManager Attributes => attrManager;
        static EntityManager entityManager;
        public static EntityManager Entity => entityManager;
        static ItemManager itemManager;
        public static ItemManager Items => itemManager;
        static RecipeManager recipeManager;
        public static RecipeManager Recipes => recipeManager;
        static WorldManager worldManager;
        public static WorldManager World => worldManager;
        static TimeManager timeManager;
        public static TimeManager Time => timeManager;

        #endregion


        #region World-entry
        static TickSystem tickSystem;
        public static TickSystem Tick => tickSystem;
        static ParticleManager particleManager;
        public static ParticleManager Particles => particleManager;

        #endregion

        
        #region Debug

        public FreeLookCamera FreeLookCamera;

        #endregion


        /// <summary>
        /// Called after all Managers have been initialized.
        /// </summary>
        internal event Action OnLateInit;

        PlayerInput mainInput;
        public PlayerInput MainInput => mainInput;        

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
            }

            mainInput = GetComponent<PlayerInput>();
        }

        void Start()
        {
            InitializeManagers();
            
            FreeLookCamera.InitializeInputs(); /// DEBUGGING ONLY
        }

        void OnValidate()
        {
            Application.targetFrameRate = TargetFramerate;
        }

        /// This should be inside World
        public WorldEventController worldEventController;

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

            /// This should be inside World
            // worldEventController.Initialize();

            #endregion            

            OnLateInit?.Invoke();
        }

        
        #region Public methods
        
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
    }
}
