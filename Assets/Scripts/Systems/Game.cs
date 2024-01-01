using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UZSG.UI;
using UZSG.World;

namespace UZSG.Systems
{
    public sealed class Game : MonoBehaviour
    {
        public static Game Main { get; private set; }
        static Console _console;
        public static Console Console { get => _console; }
        static UIManager _UI;
        public static UIManager UI { get => _UI; }
        // static AudioManager _audio;
        // public static AudioManager Audio { get => _audio; }
        static ItemManager _items;
        public static ItemManager Items { get => _items; }
        static WorldManager _worldManager;
        public static WorldManager World { get => _worldManager; }
        static TickSystem _tick;
        public static TickSystem Tick { get => _tick; }
        static EntityManager _entityHandler;
        public static EntityManager Entity { get => _entityHandler; }

        PlayerInput _input;
        InputAction _toggleConsoleInput;
        
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
                _UI = GetComponentInChildren<UIManager>();
                _tick = GetComponentInChildren<TickSystem>();
                _console = GetComponentInChildren<Console>();
                _worldManager = GetComponentInChildren<WorldManager>();
                _items = GetComponentInChildren<ItemManager>();
                _entityHandler = GetComponentInChildren<EntityManager>();
            }

            _input = GetComponent<PlayerInput>();
        }

        void Start()
        {
            InitializeManagers();
            InitializeGlobalControls();
        }

        void InitializeManagers()
        {
            // The console is initialized first thing
            _console.Initialize();

            _UI.Initialize();
            // _audio.Initialize();

            #region Theses should be only initialized upon entering worlds
            // _worldManager.Initialize();
            _tick.Initialize();
            _items.Initialize();
            // _entityHandler.Initialize();
            #endregion

            OnLateInit?.Invoke();
        }

        void InitializeGlobalControls()
        {
            _toggleConsoleInput = _input.actions.FindAction("Toggle Console Window");

            _toggleConsoleInput.performed += ToggleConsole;
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
