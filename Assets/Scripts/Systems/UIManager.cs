using System;
using System.Collections.Generic;
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
    public class UIManager : MonoBehaviour
    {
        [Serializable]
        public struct PrefabId
        {
            public string Id;
            public GameObject Prefab;
        }

        [Serializable]
        public struct IconId
        {
            public string Id;
            public Sprite Sprite;
        }

        bool _isInitialized;

        public bool EnableScreenAnimations = true;

        [SerializeField] Canvas canvas;
        public Canvas Canvas => canvas;

        bool _isCursorVisible;
        public bool IsCursorVisible => _isCursorVisible;


        #region UI Events
        public event Action<bool> OnCursorToggled;
        
        #endregion

        
        #region Inputs
        PlayerInput input;
        InputAction toggleCursorInput;
        InputAction closeCurrentWindowInput;

        #endregion


        [Header("UI Prefabs")]
        public List<PrefabId> Prefabs;

        internal void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            Game.Main.OnLateInit += OnLateInit;
            
            Game.Console.Log("Initializing UI...");

            input = Game.Main.MainInput;
            toggleCursorInput = input.actions.FindAction("Toggle Cursor");
            closeCurrentWindowInput = input.actions.FindAction("Close Current Window");
                     
            toggleCursorInput.performed += ToggleCursor;
                        
            input.actions.FindActionMap("Global").Enable();
        }

        void InitializeUIPrefabs()
        {
            var a = Resources.LoadAll<GameObject>("Prefabs/UI");
            foreach (var b in a)
            {
                if (b.TryGetComponent<IUIElement>(out var element))
                {

                }
            }
        }
      
        void OnLateInit()
        {
        }

        void ToggleCursor(InputAction.CallbackContext context)
        {
            ToggleCursor();
        }

        public void ToggleCursor()
        {
            ToggleCursor(!_isCursorVisible);
        }

        public void ToggleCursor(bool enabled)
        {
            if (enabled)            
                Cursor.lockState = CursorLockMode.None;
            else            
                Cursor.lockState = CursorLockMode.Locked;            
            
            _isCursorVisible = enabled;
            Cursor.visible = enabled;
            OnCursorToggled?.Invoke(enabled);
        }

        // public ItemDisplayUI CreateItemDisplay(Item item)
        // {
        //     return Instantiate(itemDisplayPrefab, Canvas.transform).GetComponent<ItemDisplayUI>();
        // }

        // public void InitInventoryWindow(InventoryHandler inventory)
        // {
        //     GameObject go = Instantiate(inventoryPrefab, canvas.transform);
        //     go.name = "Inventory Window";

        //     _inventoryUI = go.GetComponent<PlayerInventoryWindow>();
        //     _inventoryUI.BindInventory(inventory);
        //     _inventoryUI.Initialize();
        //     _inventoryUI.Hide();
        // }

        public T Create<T>(string id, bool inSafeArea = true) where T : IUIElement
        {
            Transform parent = Canvas.transform;
            if (!inSafeArea) parent = Canvas.transform; /// lewl
            foreach (PrefabId p in Prefabs)
            {
                if (p.Id == id)
                {
                    var go = Instantiate(p.Prefab, parent);
                    go.name = p.Prefab.name;

                    if (go.TryGetComponent(out T element))
                    {
                        return element;
                    }
                    return default;
                }
            }

            var msg = $"Unable to create UI element, it does not exist";
            Game.Console.Log(msg);
            Debug.LogWarning(msg);
            return default;
        }
    }
}
