using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UZSG.UI;
using UZSG.WorldBuilder;

namespace UZSG.Systems
{
    public sealed class Game : MonoBehaviour
    {
        public static Game Main { get; private set; }

        #region Core
        static Console _console;
        public static Console Console { get => _console; }
        static UIManager _UI;
        public static UIManager UI { get => _UI; }
        static AudioManager _audioManager;
        public static AudioManager Audio { get => _audioManager; }
        static WorldManager _worldManager;
        public static WorldManager World { get => _worldManager; }
        #endregion

        #region World-entry
        static TickSystem _tick;
        public static TickSystem Tick { get => _tick; }
        static AttributesManager _attrManager;
        public static AttributesManager Attributes { get => _attrManager; }
        static ItemManager _itemManager;
        public static ItemManager Items { get => _itemManager; }
        static RecipeManager _recipeManager;
        public static RecipeManager Recipes { get => _recipeManager; }
        static EntityManager _entityManager;
        public static EntityManager Entity { get => _entityManager; }
        #endregion
        
        #region Debug
        public FreeLookCamera FreeLookCamera;
        #endregion

        PlayerInput input;
        InputAction toggleConsoleInput;
        
        /// <summary>
        /// Called after all Managers have been initialized.
        /// </summary>
        public event Action OnLateInit;

        void Awake()
        {
            DontDestroyOnLoad(this);

            if (Main != null && Main != this)
            {
                Destroy(this);
            } else
            {
                Main = this;
                _console = GetComponentInChildren<Console>();
                
                _UI = GetComponentInChildren<UIManager>();
                _audioManager = GetComponentInChildren<AudioManager>();
                _worldManager = GetComponentInChildren<WorldManager>();
                
                _tick = GetComponentInChildren<TickSystem>();
                _attrManager = GetComponentInChildren<AttributesManager>();
                _itemManager = GetComponentInChildren<ItemManager>();
                _entityManager = GetComponentInChildren<EntityManager>();
            }

            input = GetComponent<PlayerInput>();
        }

        void Start()
        {
            InitializeManagers();
            InitializeGlobalControls();
            
            FreeLookCamera.Initialize();
        }

        void InitializeManagers()
        {
            // The console is initialized first thing
            _console.Initialize();

            _UI.Initialize();
            _audioManager.Initialize();
            _worldManager.Initialize();

            #region These should be only initialized upon entering worlds
            /// These are run only on scenes that are already "worlds"           
            _tick.Initialize();
            _attrManager.Initialize();
            _itemManager.Initialize();
            _entityManager.Initialize();

            #endregion            

            OnLateInit?.Invoke();
        }

        void InitializeGlobalControls()
        {
            toggleConsoleInput = input.actions.FindAction("Toggle Console Window");

            toggleConsoleInput.performed += ToggleConsole;
        }

        /// <summary>
        /// Honestly, these callbacks can be in a different partial file
        /// </summary>
        #region Input action callbacks

        void ToggleConsole(InputAction.CallbackContext context)
        {
            ToggleConsole(!_UI.ConsoleUI.IsVisible);
        }

        #endregion
        
        #region Public methods        
        /// <summary>
        /// Shows/hide the console window.
        /// </summary>
        public void ToggleConsole(bool isVisible)
        {
            _UI.ConsoleUI.ToggleWindow(isVisible);
        }
        public void ToggleConsole()
        {
            ToggleConsole(!_UI.ConsoleUI.IsVisible);
        }

        #endregion
    }
}
