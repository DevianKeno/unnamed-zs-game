using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UZSG.Inventory;
using UZSG.Items;
using UZSG.Systems;

namespace UZSG.UI
{
    /// <summary>
    /// UI Manager for UZSG.
    /// </summary>
    public sealed class UIManager : MonoBehaviour
    {
        bool _isInitialized;

        [SerializeField] Canvas canvas;
        public Canvas Canvas => canvas;

        bool _isCursorVisible;
        public bool IsCursorVisible => _isCursorVisible;

        [Header("UIs")]
        [SerializeField] ConsoleUI _consoleUI;
        public ConsoleUI ConsoleUI => _consoleUI;
        [SerializeField] InteractionIndicator _interactIndicator;
        public InteractionIndicator InteractIndicator => _interactIndicator;
        [SerializeField] HUDHandler _HUD;
        public HUDHandler HUD => _HUD;
        [SerializeField] InventoryUI _inventoryUI;
        public InventoryUI InventoryUI => _inventoryUI;

        #region UI Events
        /// <summary>
        /// Called everytime the visibility of the cursor changes.
        /// Params:
        ///   bool: isVisible
        /// </summary>
        public event Action<bool> OnCursorToggled;
        
        #endregion
        
        PlayerInput input;
        InputAction toggleCursorInput;

        [Header("UI Prefabs")]
        [SerializeField] GameObject consoleWindowPrefab;
        [SerializeField] GameObject interactIndicatorPrefab;
        [SerializeField] GameObject HUDPrefab;
        [SerializeField] GameObject inventoryPrefab;
        [SerializeField] GameObject itemDisplayPrefab;
        
        void Awake()
        {
            input = GetComponent<PlayerInput>();
        }

        internal void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            GameObject go;            
            Game.Console?.Log("Initializing UI...");

            input.actions.FindActionMap("Global").Enable();
            toggleCursorInput = input.actions.FindAction("Toggle Cursor");
            toggleCursorInput.performed += ToggleCursor;

            go = Instantiate(consoleWindowPrefab, canvas.transform);
            go.name = "Console Window";
            _consoleUI = go.GetComponent<ConsoleUI>();
            _consoleUI.Initialize();
            
            go = Instantiate(HUDPrefab, canvas.transform);
            go.name = "HUD";
            _HUD = go.GetComponent<HUDHandler>();
            _HUD.Initialize();

            #region Should only appear inside worlds
            _inventoryUI?.Hide();
            _interactIndicator = Instantiate(interactIndicatorPrefab, canvas.transform).GetComponent<InteractionIndicator>();
            _interactIndicator.Hide();
            #endregion
        }

        void ToggleCursor(InputAction.CallbackContext context)
        {
            ToggleCursor(!_isCursorVisible);
        }

        public void ToggleCursor()
        {
            ToggleCursor(!_isCursorVisible);
        }

        public void ToggleCursor(bool isVisible)
        {
            if (isVisible)            
                Cursor.lockState = CursorLockMode.None;
            else            
                Cursor.lockState = CursorLockMode.Locked;            
            
            _isCursorVisible = isVisible;
            Cursor.visible = isVisible;
            OnCursorToggled?.Invoke(isVisible);
        }

        public ItemDisplayUI CreateItemDisplay(Item item)
        {
            return Instantiate(itemDisplayPrefab, Canvas.transform).GetComponent<ItemDisplayUI>();
        }
    }
}
