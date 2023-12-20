using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using URMG.Core;
using URMG.InventoryS;
using URMG.Items;

namespace URMG.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] InventoryHandler _inventory;
        Dictionary<int, SlotUI> _slotUIs = new();
        bool _isVisible = true;
        public bool IsVisible { get => _isVisible; }
        bool _isHolding = false;
        int _fromIndex;
        Item _heldItem;
        Slot selectedSlot;

        bool _isPutting;
        bool _isGetting;
        ItemDisplayUI _displayedItem;
        [SerializeField] GameObject bag;
        [SerializeField] GameObject slotPrefab;
        [SerializeField] GameObject itemDisplayPrefab;

        void Start()
        {
            int i = 0;
            foreach (Transform t in bag.transform)
            {
                SlotUI s = t.GetComponent<SlotUI>();
                s.Index = i;
                s.OnClick += OnClickSlot;
                _slotUIs.Add(i, s);
                i++;
            }

            Hide();
        }

        void OnEnable()
        {
            _inventory.OnSlotContentChanged += SlotContentChangedCallback;
        }

        private void SlotContentChangedCallback(object sender, SlotContentChangedArgs e)
        {
            _slotUIs[e.Slot.Index].SetDisplay(e.Slot.Item);
        }

        public void OnClickSlot(object slot, PointerEventData e)
        {
            SlotUI slotUI = (SlotUI) slot;
            selectedSlot = _inventory.GetSlot(slotUI.Index);

            if (e.button == PointerEventData.InputButton.Left)
            {
                if (_isHolding)
                {
                    if (_inventory.TryPut(selectedSlot.Index, _heldItem))
                    {
                        ReleaseItem();
                    } else // swap items
                    {
                        Item tookItem = _inventory.Take(selectedSlot.Index);
                        _inventory.TryPut(selectedSlot.Index, SwapHeld(tookItem));
                    }
                } else
                {
                    HoldItem(_inventory.Take(selectedSlot.Index));
                }
            
            } else if (e.button == PointerEventData.InputButton.Right)
            {                
                if (_isHolding) // put 1 to selected slot
                {                    
                    _isPutting = true;
                    
                    if (_inventory.TryPut(selectedSlot.Index, new(_heldItem, 1)))
                    {
                        HoldItem(new(_heldItem, _heldItem.Count - 1));
                        _displayedItem.SetDisplay(_heldItem);
                    } else // swap items
                    {
                        Item tookItem = _inventory.Take(selectedSlot.Index);
                        _inventory.TryPut(selectedSlot.Index, SwapHeld(tookItem));
                    }
                } else // get 1 from selected slot
                {
                    _isGetting = true;
                    HoldItem(_inventory.TakeItems(selectedSlot.Index, 1));
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
            _slotUIs[slotIndex].SetDisplay(item);
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