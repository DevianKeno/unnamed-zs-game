using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using UZSG.UI;
using UZSG.World;
using UZSG.World.Weather;
using UZSG.WorldBuilder;

namespace UZSG.Systems
{
    public class Game : MonoBehaviour
    {
        public static Game Main { get; private set; }

        public int TargetFramerate = -1;


        #region Core

        static Console console;
        public static Console Console { get => console; }
        static UIManager UIManager;
        public static UIManager UI { get => UIManager; }
        static AudioManager audioManager;
        public static AudioManager Audio { get => audioManager; }
        static WorldManager worldManager;
        public static WorldManager World { get => worldManager; }

        #endregion


        #region World-entry
        static TickSystem tickSystem;
        public static TickSystem Tick { get => tickSystem; }
        static TimeManager timeManager;
        public static TimeManager Time { get => timeManager; }
        static AttributesManager attrManager;
        public static AttributesManager Attributes { get => attrManager; }
        static ItemManager itemManager;
        public static ItemManager Items { get => itemManager; }
        static RecipeManager recipeManager;
        public static RecipeManager Recipes { get => recipeManager; }
        static EntityManager entityManager;
        public static EntityManager Entity { get => entityManager; }

        #endregion

        
        #region Debug
        public FreeLookCamera FreeLookCamera;

        #endregion


        /// <summary>
        /// Called after all Managers have been initialized.
        /// </summary>
        internal event Action OnLateInit;

        PlayerInput mainInput;
        public PlayerInput MainInput { get => mainInput; }        

        void Awake()
        {
            DontDestroyOnLoad(this);

            if (Main != null && Main != this)
            {
                Destroy(this);
            } else
            {
                Main = this;

                console = GetComponentInChildren<Console>();                
                UIManager = GetComponentInChildren<UIManager>();
                audioManager = GetComponentInChildren<AudioManager>();
                timeManager = GetComponentInChildren<TimeManager>();
                worldManager = GetComponentInChildren<WorldManager>();
                
                tickSystem = GetComponentInChildren<TickSystem>();
                attrManager = GetComponentInChildren<AttributesManager>();
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

        public WorldEventController worldEventController;

        void InitializeManagers()
        {
            /// The console is initialized first thing
            console.Initialize();

            UI.Initialize();
            audioManager.Initialize();
            worldManager.Initialize();

            #region These should be only initialized upon entering worlds
            /// These are run only on scenes that are already "worlds"           
            tickSystem.Initialize();
            attrManager.Initialize();
            itemManager.Initialize();
            entityManager.Initialize();
            recipeManager.Initialize();

            worldEventController.Initialize();

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
