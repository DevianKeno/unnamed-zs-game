using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UZSG.Items;
using UZSG.Systems;

namespace UZSG.UI
{
    /// <summary>
    /// UI Manager for URMG.
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
        [SerializeField] InventoryUI _inventoryUI;
        public InventoryUI InventoryUI => _inventoryUI;

        #region UI Events
        /// <summary>
        /// Called everytime the visibility of the cursor changes.
        /// </summary>
        public event Action<bool> OnCursorToggled;
        
        #endregion
        
        PlayerInput _input;
        InputAction toggleCursorInput;


        [Header("UI Prefabs")]
        [SerializeField] GameObject consoleWindowPrefab;
        [SerializeField] GameObject interactIndicatorPrefab;
        [SerializeField] GameObject inventoryPrefab;
        [SerializeField] GameObject itemDisplayPrefab;
        
        void Awake()
        {
            _input = GetComponent<PlayerInput>();
        }

        internal void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            GameObject go;            
            Game.Console?.Log("Initializing UI...");

            _input.actions.FindActionMap("Global").Enable();
            toggleCursorInput = _input.actions.FindAction("Toggle Cursor");
            toggleCursorInput.performed += ToggleCursor;

            go = Instantiate(consoleWindowPrefab, canvas.transform);
            go.name = "Console Window";
            _consoleUI = go.GetComponent<ConsoleUI>();
            _consoleUI.Initialize();

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
