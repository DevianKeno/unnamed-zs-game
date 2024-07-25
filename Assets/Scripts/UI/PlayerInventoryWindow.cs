using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

using UZSG.Systems;
using UZSG.Inventory;
using UZSG.Items;
using UZSG.Entities;
using UnityEngine.UI;
using System.Collections;

namespace UZSG.UI
{
    public class PlayerInventoryWindow : Window, IInitializable
    {
        public Player Player;
        [SerializeField] InventoryHandler inventory;
        public InventoryHandler Inventory => inventory;
        bool _isInitialized;
        public bool IsInitialized => _isInitialized;
        /// <summary>
        /// If the cursor is currently holding an item.
        /// </summary>
        bool _isHoldingItem = false;
        int _lastSelectedSlotIndex;
        Item _heldItem;
        ItemSlot _selectedSlot;
        ItemSlotUI _selectedSlotUI;
        bool _isHoldingShift;
        bool _isPutting;
        bool _isGetting;
        Dictionary<int, ItemSlotUI> _bagSlotUIs = new();

        ItemDisplayUI _displayedItem;
        Selector selector;
        Graphic[] _UIElements;
        float[] _UIInitialAlphas;

        [Header("Inventory Components")]
        [SerializeField] FrameController frameController;
        [SerializeField] GameObject bag;
        [SerializeField] GameObject hotbar;

        [Header("Prefabs")]
        [SerializeField] GameObject slotPrefab;
        [SerializeField] GameObject weaponSlotPrefab;
        [SerializeField] GameObject itemDisplayPrefab;
        [SerializeField] GameObject selectorPrefab;
        
        InputActionMap actionMap;
        Dictionary<string, InputAction> inputs = new();

        void Update()
        {
            if (_displayedItem != null)
            {
                _displayedItem.transform.position = Input.mousePosition;
            }
        }
        
        public void Initialize()
        {
            InitializeElements();
            
            if (Player == null)
            {
                Game.Console.Log($"Failed to initialize Player HUD. Bind a Player first!");
                Debug.LogWarning($"Failed to initialize Player HUD. Bind a Player first!");
                return;
            }
            if (_isInitialized) return;
            _isInitialized = true;

            InitializeBag();
            selector = Instantiate(selectorPrefab, transform).GetComponent<Selector>();
            frameController.SwitchToFrame("bag", force: true);
            InitializeInputs();
            Hide();
        }

        void InitializeElements()
        {
            foreach (Graphic graphic in GetComponentsInChildren<Graphic>())
            {
                if (!graphic.TryGetComponent<FadeableElement>(out var fadeableElement))
                {
                    fadeableElement = graphic.gameObject.AddComponent<FadeableElement>();
                }
                fadeableElement.Initialize();
            }
        }

        void InitializeBag()
        {
            int maxSlots = inventory.Bag.SlotsCount;
            for (int i = 0; i < maxSlots; i++)
            {
                var go = Instantiate(slotPrefab);
                go.name = $"Slot ({i})";
                go.transform.SetParent(bag.transform);

                ItemSlotUI slot = go.GetComponent<ItemSlotUI>();
                slot.Index = i;
                slot.OnMouseUp += OnClickBagSlot;
                slot.OnStartHover += OnStartHoverSlot;
                slot.OnEndHover += OnEndHoverSlot;
                _bagSlotUIs.Add(i, slot);
            }
            inventory.Hotbar.OnChangeEquipped += HotbarChangeEquippedCallback;
            inventory.Bag.OnSlotContentChanged += BagSlotChangedCallback;
        }

        void InitializeInputs()
        {
            actionMap = Game.Main.GetActionMap("Inventory");
            inputs = Game.Main.GetActionsFromMap(actionMap);

            inputs["Shift Click"].performed += (context) =>
            {
                _isHoldingShift = context.ReadValue<float>() > 0; /// unsure
            };

            inputs["Hide/Show"].performed += (context) =>
            {
                ToggleVisibility();
            };

            inputs["Hide"].performed += (context) =>
            {
                SetVisible(false);
            };
        }

        
        public void BindPlayer(Player player)
        {
            Player = player;
            inventory ??= player.Inventory;
        }

        public void SetSlotDisplay(int slotIndex, Item item)
        {
            if (slotIndex > 18 )
            {
                Game.Console.LogDebug("Slot index out of bounds.");
                return;
            }
            _bagSlotUIs[slotIndex].SetDisplayedItem(item);
        }

        public override void OnShow()
        {
            if (Game.UI.EnableScreenAnimations)
            {
                LeanTween.cancel(gameObject);
                FadeIn();
                AnimateEntry();
            }
            else
            {
                rect.localScale = Vector3.one;
            }
            selector.Hide();
            actionMap.Enable();
            Game.UI.ToggleCursor(true);
        }


        void FadeIn()
        {
            foreach (var element in GetComponentsInChildren<FadeableElement>())
            {
                element.FadeIn(AnimationFactor);
            }
        }

        void AnimateEntry()
        {
            rect.localScale = new(0.95f, 0.95f, 0.95f);
            LeanTween.scale(gameObject, Vector3.one, AnimationFactor)
            .setEase(LeanTweenType.easeOutExpo);
        }

        public override void OnHide()
        {
            PutBackHeldItem();
            actionMap.Disable();
            _selectedSlotUI?.Refresh();
            _selectedSlotUI = null;
            selector.Hide();
            gameObject.SetActive(false);
            Game.UI.ToggleCursor(false);
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

        void HotbarChangeEquippedCallback(object sender, Hotbar.ChangeEquippedArgs e)
        {
            
        }

        void BagSlotChangedCallback(object sender, SlotContentChangedArgs e)
        {
            _bagSlotUIs[e.Slot.Index].SetDisplayedItem(e.Slot.Item);
        }

        void OnClickBagSlot(object sender, PointerEventData e)
        {
            _selectedSlotUI = (ItemSlotUI) sender;
            _selectedSlot = inventory.Bag[_selectedSlotUI.Index];

            if (e.button == PointerEventData.InputButton.Left)
            {
                if (_isHoldingItem)
                {
                    if (inventory.Bag.TryPut(_selectedSlot.Index, _heldItem))
                    {
                        ReleaseItem();
                    }
                    else /// swap items
                    {
                        Item tookItem = inventory.Bag.Take(_selectedSlot.Index);
                        inventory.Bag.TryPut(_selectedSlot.Index, SwapHeld(tookItem));
                    }
                }
                else
                {
                    if (_selectedSlot.IsEmpty) return;

                    HoldItem(inventory.Bag.Take(_selectedSlot.Index));
                    _lastSelectedSlotIndex = _selectedSlot.Index;
                }
            }
            else if (e.button == PointerEventData.InputButton.Right)
            {                
                if (_isHoldingItem) /// put 1 to selected slot
                {                    
                    _isPutting = true;
                    
                    if (inventory.Bag.TryPut(_selectedSlot.Index, new(_heldItem, 1)))
                    {
                        HoldItem(new(_heldItem, _heldItem.Count - 1));
                    }
                    else /// swap items
                    {
                        Item tookItem = inventory.Bag.Take(_selectedSlot.Index);
                        inventory.Bag.TryPut(_selectedSlot.Index, SwapHeld(tookItem));
                    }                    
                    _isPutting = false;
                }
                else /// get 1 from selected slot
                {
                    _isGetting = true;
                    HoldItem(inventory.Bag.TakeItems(_selectedSlot.Index, 1));
                    _isGetting = false;
                }
            }
        }

        Item SwapHeld(Item item)
        {
            if (!_isHoldingItem) return Item.None;

            Item prevHeld = _heldItem;
            _heldItem = item;
            _displayedItem.SetDisplayedItem(_heldItem);
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
            _displayedItem ??= Game.UI.Create<ItemDisplayUI>("item_display");
            _displayedItem.SetDisplayedItem(_heldItem);
        }

        void ReleaseItem()
        {
            _isHoldingItem = false;
            _heldItem = null;
            Destroy(_displayedItem.gameObject);
            _displayedItem = null;
        }

        void PutBackHeldItem()
        {
            if (_heldItem == null) return;
            
            inventory.Bag.TryPut(_lastSelectedSlotIndex, _heldItem);
            ReleaseItem();
            _lastSelectedSlotIndex = -1;
        }
    }
}