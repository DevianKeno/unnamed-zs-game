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
        static Console _console;
        public static Console Console { get => _console; }
        static UIManager _UI;
        public static UIManager UI { get => _UI; }
        // static AudioManager _audio;
        // public static AudioManager Audio { get => _audio; }
        static ItemManager _items;
        public static ItemManager Items { get => _items; }
        static AttributesManager _attrs;
        public static AttributesManager AttributesManager { get => _attrs; }
        static WorldManager _worldManager;
        public static WorldManager World { get => _worldManager; }
        static TickSystem _tick;
        public static TickSystem Tick { get => _tick; }
        static EntityManager _entityManager;
        public static EntityManager Entity { get => _entityManager; }

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
                _console = GetComponentInChildren<Console>();
                _UI = GetComponentInChildren<UIManager>();
                _tick = GetComponentInChildren<TickSystem>();
                _worldManager = GetComponentInChildren<WorldManager>();
                _items = GetComponentInChildren<ItemManager>();
                _entityManager = GetComponentInChildren<EntityManager>();
                _attrs = GetComponentInChildren<AttributesManager>();
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
            _worldManager.Initialize();

            #region Theses should be only initialized upon entering worlds
            _tick.Initialize();
            _items.Initialize();
            _entityManager.Initialize();
            _attrs.Initialize();
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
