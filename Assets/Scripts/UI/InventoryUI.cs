using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UZSG.Systems;
using UZSG.Inventory;
using UZSG.Items;

namespace UZSG.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] InventoryHandler _inventory;
        [SerializeField] Dictionary<int, ItemSlotUI> _bagSlotUIs = new();
        [SerializeField] Dictionary<int, ItemSlotUI> _hotbarSlotUIs = new();
        bool _isVisible = true;
        public bool IsVisible { get => _isVisible; }
        bool _isHolding = false;
        int _fromIndex;
        Item _heldItem;
        ItemSlot _selectedSlot;
        ItemSlotUI _selectedSlotUI;
        bool _isPutting;
        bool _isGetting;
        ItemDisplayUI _displayedItem;
        Selector selector;

        [Header("Bag")]
        [SerializeField] GameObject bag;
        [SerializeField] GameObject hotbar;

        [Header("Prefabs")]
        [SerializeField] GameObject slotPrefab;
        [SerializeField] GameObject itemDisplayPrefab;
        [SerializeField] GameObject selectorPrefab;

        void Awake()
        {
            // Bag slots
            int i = 0;
            foreach (Transform t in bag.transform)
            {
                ItemSlotUI s = t.GetComponent<ItemSlotUI>();
                s.Index = i;
                s.OnClick += OnClickBagSlot;
                s.OnStartHover += OnStartHoverSlot;
                s.OnEndHover += OnEndHoverSlot;
                _bagSlotUIs.Add(i, s);
                i++;
            }

            // Hotbar slots
            i = 1;
            foreach (Transform t in hotbar.transform)
            {
                ItemSlotUI s = t.GetComponent<ItemSlotUI>();
                s.Index = i;
                s.OnClick += OnClickHotbarSlot;
                s.OnStartHover += OnStartHoverSlot;
                s.OnEndHover += OnEndHoverSlot;
                _hotbarSlotUIs.Add(i, s);
                i++;
            }
            _hotbarSlotUIs[10].Index = 0;
        }

        void Start()
        {
            selector = Instantiate(selectorPrefab, transform).GetComponent<Selector>();
            selector.Hide();
            Hide();
        }

        void OnEnable()
        {
            _inventory.Hotbar.OnSlotContentChanged += HotbarSlotChangedCallback;
            _inventory.Hotbar.OnChangeEquipped += HotbarChangeEquippedCallback;
            _inventory.Bag.OnSlotContentChanged += BagSlotChangedCallback;
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
                if (_isHolding)
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
                if (_isHolding) // put 1 to selected slot
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
                } else // get 1 from selected slot
                {
                    _isGetting = true;
                    HoldItem(_inventory.Bag.TakeItems(_selectedSlot.Index, 1));
                }
            }
        }

        Item SwapHeld(Item item)
        {
            if (!_isHolding) return Item.None;

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

            _isHolding = true;
            _heldItem = item;
            if (_displayedItem == null) _displayedItem = Instantiate(itemDisplayPrefab, Game.UI.Canvas.transform).GetComponent<ItemDisplayUI>();
            _displayedItem.SetDisplay(_heldItem);
        }

        void ReleaseItem()
        {
            _isHolding = false;
            _heldItem = null;
            Destroy(_displayedItem.gameObject);
        }

        void Update()
        {
            if (_displayedItem != null)
            {
                _displayedItem.transform.position = Input.mousePosition;
            }
        }

        public void SetSlotDisplay(int slotIndex, Item item)
        {
            if (slotIndex > 18 )
            {
                Debug.Log("Slot index out of bounds.");
                return;
            }
            _bagSlotUIs[slotIndex].SetDisplay(item);
        }

        public void Show()
        {
            if (_isVisible) return;
            _isVisible = true;
            gameObject.SetActive(true);
            Cursor.Show();
        }

        public void Hide()
        {
            if (!_isVisible) return;
            _selectedSlotUI?.SetState(UIState.Normal);
            _selectedSlotUI = null;
            _isVisible = false;
            gameObject.SetActive(false);
            Cursor.Hide();
        }

        public void ToggleVisibility()
        {
            if (_isVisible)
            {
                Hide();
            } else
            {
                Show();
            }
        }
    }
}