using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UZSG.Systems;
using UZSG.Inventory;
using UZSG.Items;
using UnityEngine.InputSystem;

namespace UZSG.UI
{
    public class InventoryUI : MonoBehaviour, IInitializable
    {
        [SerializeField] InventoryHandler _inventory;
        [SerializeField] Dictionary<int, ItemSlotUI> _bagSlotUIs = new();
        [SerializeField] Dictionary<int, ItemSlotUI> _hotbarSlotUIs = new();
        bool _isInitialized;
        public bool IsInitialized => _isInitialized;
        bool _isVisible = true;
        public bool IsVisible { get => _isVisible; }
        /// <summary>
        /// If the cursor is currently holding an item.
        /// </summary>
        bool _isHoldingItem = false;
        int _fromIndex;
        Item _heldItem;
        ItemSlot _selectedSlot;
        ItemSlotUI _selectedSlotUI;
        bool _isPutting;
        bool _isGetting;
        ItemDisplayUI _displayedItem;
        Selector selector;

        [Header("Inventory Components")]
        [SerializeField] GameObject bag;
        [SerializeField] GameObject hotbar;

        [Header("Inventory Components")]
        [SerializeField] PlayerInput input;
        InputActionMap actionMap;
        InputAction shiftInput;
        InputAction closeInput;


        [Header("Prefabs")]
        [SerializeField] GameObject slotPrefab;
        [SerializeField] GameObject weaponSlotPrefab;
        [SerializeField] GameObject itemDisplayPrefab;
        [SerializeField] GameObject selectorPrefab;

        void Awake()
        {
            input = GetComponent<PlayerInput>();

            actionMap = input.actions.FindActionMap("Inventory Window");
            shiftInput = actionMap.FindAction("Shift Click");
            closeInput = actionMap.FindAction("Close");
        }

        void Start()
        {
            shiftInput.performed += (context) =>
            {

            };
            
            closeInput.performed += (context) =>
            {
                ToggleVisibility(false);
            };
        }

        void OnDestroy()
        {
            _inventory.Hotbar.OnSlotContentChanged -= HotbarSlotChangedCallback;
            _inventory.Hotbar.OnChangeEquipped -= HotbarChangeEquippedCallback;
            _inventory.Bag.OnSlotContentChanged -= BagSlotChangedCallback;
        }

        void Update()
        {
            if (_displayedItem != null)
            {
                _displayedItem.transform.position = Input.mousePosition;
            }
        }
        
        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            // Bag slots
            for (int i = 0; i < _inventory.Bag.SlotsCount; i++)
            {
                var go = Instantiate(slotPrefab);
                go.name = $"Slot ({i})";
                go.transform.SetParent(bag.transform);

                ItemSlotUI slot = go.GetComponent<ItemSlotUI>();
                slot.Index = i;
                slot.OnClick += OnClickBagSlot;
                slot.OnStartHover += OnStartHoverSlot;
                slot.OnEndHover += OnEndHoverSlot;
                _bagSlotUIs.Add(i, slot);
            }

            // Hotbar slots
            for (int i = 0; i < _inventory.Hotbar.SlotsCount; i++)
            {
                var go = Instantiate(i == 0 ? weaponSlotPrefab : slotPrefab);
                go.name = $"Hotbar Slot ({i})";
                go.transform.SetParent(hotbar.transform);
            
                ItemSlotUI slot = go.GetComponent<ItemSlotUI>();
                slot.Index = i;
                slot.OnClick += OnClickHotbarSlot;
                slot.OnStartHover += OnStartHoverSlot;
                slot.OnEndHover += OnEndHoverSlot;
                _hotbarSlotUIs.Add(i, slot);
            }
            // _hotbarSlotUIs[10].Index = 0;
            
            _inventory.Hotbar.OnSlotContentChanged += HotbarSlotChangedCallback;
            _inventory.Hotbar.OnChangeEquipped += HotbarChangeEquippedCallback;
            _inventory.Bag.OnSlotContentChanged += BagSlotChangedCallback;

            selector = Instantiate(selectorPrefab, transform).GetComponent<Selector>();
            Hide();
        }

        
        public void BindInventory(InventoryHandler inventory)
        {
            if (inventory != null)
            {
                _inventory = inventory;
            }
        }

        public void SetSlotDisplay(int slotIndex, Item item)
        {
            if (slotIndex > 18 )
            {
                Game.Console.LogDebug("Slot index out of bounds.");
                return;
            }
            _bagSlotUIs[slotIndex].SetDisplay(item);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            selector.Hide();
            Game.UI.ToggleCursor(true);
            actionMap.Enable();
        }

        public void Hide()
        {
            actionMap.Disable();
            _selectedSlotUI?.SetState(UIState.Normal);
            _selectedSlotUI = null;
            selector.Hide();
            gameObject.SetActive(false);
            Game.UI.ToggleCursor(false);
        }

        public void ToggleVisibility()
        {
            ToggleVisibility(!_isVisible);
        }

        public void ToggleVisibility(bool isVisible)
        {
            _isVisible = isVisible;
            
            if (_isVisible)
            {
                Hide();
            } else
            {
                Show();
            }
        }

        void OnStartHoverSlot(object sender, PointerEventData e)
        {
            selector.Select((ItemSlotUI) sender);
            _selectedSlotUI = (ItemSlotUI) sender;
            // other animation stuff
        }

        void OnEndHoverSlot(object sender, PointerEventData e)
        {
            selector.Hide();
            _selectedSlotUI = null;
            _selectedSlot = null;
        }

        void HotbarSlotChangedCallback(object sender, SlotContentChangedArgs e)
        {
            _hotbarSlotUIs[e.Slot.Index].SetDisplay(e.Slot.Item);
        }

        void HotbarChangeEquippedCallback(object sender, Hotbar.ChangeEquippedArgs e)
        {
            
        }

        void OnClickHotbarSlot(object sender, PointerEventData e)
        {
            ItemSlotUI slotUI = (ItemSlotUI) sender;
            _selectedSlot = _inventory.Bag[slotUI.Index];

            if (e.button == PointerEventData.InputButton.Left)
            {

            } else if (e.button == PointerEventData.InputButton.Right)
            {

            }
        }

        void BagSlotChangedCallback(object sender, SlotContentChangedArgs e)
        {
            _bagSlotUIs[e.Slot.Index].SetDisplay(e.Slot.Item);
        }

        void OnClickBagSlot(object sender, PointerEventData e)
        {
            _selectedSlotUI = (ItemSlotUI) sender;
            _selectedSlot = _inventory.Bag[_selectedSlotUI.Index];

            if (e.button == PointerEventData.InputButton.Left)
            {
                if (_isHoldingItem)
                {
                    if (_inventory.Bag.TryPut(_selectedSlot.Index, _heldItem))
                    {
                        ReleaseItem();
                    } else // swap items
                    {
                        Item tookItem = _inventory.Bag.Take(_selectedSlot.Index);
                        _inventory.Bag.TryPut(_selectedSlot.Index, SwapHeld(tookItem));
                    }
                } else
                {
                    HoldItem(_inventory.Bag.Take(_selectedSlot.Index));
                }
            
            } else if (e.button == PointerEventData.InputButton.Right)
            {                
                if (_isHoldingItem) // put 1 to selected slot
                {                    
                    _isPutting = true;
                    
                    if (_inventory.Bag.TryPut(_selectedSlot.Index, new(_heldItem, 1)))
                    {
                        HoldItem(new(_heldItem, _heldItem.Count - 1));
                        _displayedItem.SetDisplay(_heldItem);
                    } else // swap items
                    {
                        Item tookItem = _inventory.Bag.Take(_selectedSlot.Index);
                        _inventory.Bag.TryPut(_selectedSlot.Index, SwapHeld(tookItem));
                    }                    
                    _isPutting = false;
                } else // get 1 from selected slot
                {
                    _isGetting = true;
                    HoldItem(_inventory.Bag.TakeItems(_selectedSlot.Index, 1));
                    _isGetting = false;
                }
            }
        }

        Item SwapHeld(Item item)
        {
            if (!_isHoldingItem) return Item.None;

            Item prevHeld = _heldItem;
            _heldItem = item;
            _displayedItem.SetDisplay(_heldItem);
            return prevHeld;
        }

        void HoldItem(Item item)
        {
            if (item == Item.None) return;
            if (item.Count < 1)
            {
                ReleaseItem();
                return;
            }

            _isHoldingItem = true;
            _heldItem = item;
            if (_displayedItem == null) _displayedItem = Instantiate(itemDisplayPrefab, Game.UI.Canvas.transform).GetComponent<ItemDisplayUI>();
            _displayedItem.SetDisplay(_heldItem);
        }

        void ReleaseItem()
        {
            _isHoldingItem = false;
            _heldItem = null;
            Destroy(_displayedItem.gameObject);
        }
    }
}