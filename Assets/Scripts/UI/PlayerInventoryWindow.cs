using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Inventory;
using UZSG.Items;
using UZSG.Entities;

namespace UZSG.UI
{
    public class PlayerInventoryWindow : Window, IInitializeable
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
        
        ChoiceWindow itemOptions;
        ItemDisplayUI displayedItem;
        Selector selector;

        [Header("Inventory Components")]
        [SerializeField] FrameController frameController;
        [SerializeField] GameObject bag;
        
        [Header("Prefabs")]
        [SerializeField] GameObject selectorPrefab;
        
        InputActionMap actionMap;
        Dictionary<string, InputAction> inputs = new();

        void Update()
        {
            if (displayedItem != null)
            {
                displayedItem.transform.position = Input.mousePosition;
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

            InitializeEvents();
            InitializeBagSlotUIs();
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

        void InitializeEvents()
        {
            frameController.OnSwitchFrame += OnSwitchFrame;
        }

        void InitializeBagSlotUIs()
        {
            int maxSlots = inventory.Bag.SlotCount;
            for (int i = 0; i < maxSlots; i++)
            {
                var slot = Game.UI.Create<ItemSlotUI>("Item Slot");
                slot.name = $"Slot ({i})";
                slot.transform.SetParent(bag.transform);
                slot.Index = i;
                slot.OnHoverStart += OnBagSlotHoverStart;
                slot.OnHoverEnd += OnBagSlotHoverEnd;
                slot.OnMouseDown += OnBagSlotClick;
                _bagSlotUIs.Add(i, slot);
                slot.Show();
            }
            
            inventory.Bag.OnSlotContentChanged += OnBagSlotContentChanged;
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
            actionMap.Disable();
            PutBackHeldItem();
            DestroyItemOptions();
            _selectedSlotUI = null;
            selector.Hide();
            gameObject.SetActive(false);
            Game.UI.ToggleCursor(false);
        }


        #region Event callbacks

        void OnSwitchFrame(FrameController.SwitchFrameContext context)
        {
            /// Idk about this, selector might be visible on other frames
            /// subject to change
            if (context.Time == FrameController.SwitchFrameTime.Started)
            {
                selector.Hide();
            }
            else if (context.Time == FrameController.SwitchFrameTime.Finished)
            {
                if (context.Next == "bag")
                {
                    selector.Show();
                }
            }
            
            PutBackHeldItem();
            DestroyItemOptions();
        }

        void OnBagSlotContentChanged(object sender, SlotContentChangedArgs e)
        {
            _bagSlotUIs[e.Slot.Index].SetDisplayedItem(e.Slot.Item);
        }

        void OnBagSlotHoverStart(object sender, PointerEventData e)
        {
            var slot = sender as ItemSlotUI;
            selector.Select(slot.transform as RectTransform);
        }

        void OnBagSlotHoverEnd(object sender, PointerEventData e)
        {
        }

        void OnBagSlotClick(object sender, PointerEventData e)
        {
            _selectedSlotUI = (ItemSlotUI) sender;
            _selectedSlot = inventory.Bag[_selectedSlotUI.Index];

            if (e.button == PointerEventData.InputButton.Left)
            {
                DestroyItemOptions();

                if (_isHoldingItem)
                {
                    if (inventory.Bag.TryPut(_selectedSlot.Index, _heldItem))
                    {
                        ReleaseHeldItem();
                    }
                    else /// swap items
                    {
                        Item tookItem = inventory.Bag.Take(_selectedSlot.Index);
                        inventory.Bag.TryPut(_selectedSlot.Index, SwapItemWithHeldItem(tookItem));
                    }
                }
                else
                {
                    if (_selectedSlot.IsEmpty) return;

                    Item taken = inventory.Bag.Take(_selectedSlot.Index);
                    if (taken == _selectedSlot.Item)
                    {
                        
                    }
                    HoldItem(taken);
                    _lastSelectedSlotIndex = _selectedSlot.Index;
                }
            }
            else if (e.button == PointerEventData.InputButton.Right)
            {
                if (_isHoldingItem) /// put 1 to selected slot
                {
                    if (_selectedSlot.IsEmpty)
                    {
                        _isPutting = true;

                        Item toPut = _heldItem.Take(1);
                        if (inventory.Bag.TryPut(_selectedSlot.Index, toPut))
                        {
                            HoldItem(_heldItem);
                        }
                        return;
                    }

                    if (_selectedSlot.Item.CompareTo(_heldItem)) /// put 1
                    {
                        _isPutting = true;
                        Item toPut = _heldItem.Take(1);
                        if (!_selectedSlot.TryCombine(toPut, out Item excess)) 
                        {
                            _heldItem.Combine(toPut); /// return item to hand
                        }
                        HoldItem(_heldItem);
                    }
                    else /// swap items
                    {
                        Item taken = inventory.Bag.Take(_selectedSlot.Index);
                        var itemToPut = SwapItemWithHeldItem(taken);
                        inventory.Bag.TryPut(_selectedSlot.Index, itemToPut);
                    }
                    _isPutting = false;
                }
                else
                {
                    if (_selectedSlot.IsEmpty)
                    {
                        DestroyItemOptions();
                        return;
                    }

                    CreateItemOptions();
                    
                    // _isGetting = true;
                    // HoldItem(inventory.Bag.TakeItems(_selectedSlot.Index, 1));
                    // _isGetting = false;
                }
            }
        }

        #endregion


        void CreateItemOptions()
        {
            if (itemOptions)
            {
                itemOptions.Destroy();
            }
            itemOptions = Game.UI.Create<ChoiceWindow>("Choice Window", show: false);
            itemOptions.Position = _selectedSlotUI.Rect.position;
            itemOptions.Label = _selectedSlot.Item.Data.Name;

            var item = _selectedSlot.Item;
            if (item.Data.Subtype == ItemSubtype.Weapon)
            {
                AddWeaponChoices();
            }
            if (item.Data.Subtype == ItemSubtype.Useable)
            {
                itemOptions.AddChoice("Use")
                .AddCallback(() =>
                {
                    
                });
            }
            if (item.Data.Subtype == ItemSubtype.Food)
            {
                itemOptions.AddChoice("Eat")
                .AddCallback(() =>
                {
                    
                });
            }
            if (item.Data.Subtype == ItemSubtype.Consumable)
            {
                itemOptions.AddChoice("Use")
                .AddCallback(() =>
                {
                    
                });
            }
            itemOptions.AddChoice("Grab")
            .AddCallback(() =>
            {
                
            });
            itemOptions.AddChoice("Drop")
            .AddCallback(() =>
            {
                Player.Inventory.DropItem(_selectedSlot.Index);
            });

            itemOptions.Show();
        }

        void AddWeaponChoices()
        {
            var item = _selectedSlot.Item;

            var equipMainhand = itemOptions.AddChoice("Equip Mainhand")
            .AddCallback(() =>
            {
                var equipped = Inventory.Equipment.Mainhand.Item;
                ItemSlot.SwapContents(Inventory.Equipment.Mainhand, _selectedSlot);
                Player.FPP.HoldItem(item.Data);
                Player.FPP.ReleaseItem(equipped.Data);
            });
            
            var equipOffhand = itemOptions.AddChoice("Equip Offhand")
            .AddCallback(() =>
            {
                var equipped = Inventory.Equipment.Mainhand.Item;
                ItemSlot.SwapContents(Inventory.Equipment.Offhand, _selectedSlot);
                Player.FPP.HoldItem(item.Data);
                Player.FPP.ReleaseItem(equipped.Data);
            });

            /// Disables Choices when performing FPP actions
            /// and re-enables it on finish
            if (Player.FPP.IsPerforming)
            {
                equipMainhand.SetEnabled(false);
                equipOffhand.SetEnabled(false);

                itemOptions.OnClose += () => /// be sure to unsubscribe on UI close
                {
                    Player.FPP.OnPerformFinish -= ReenableChoices;
                };
                Player.FPP.OnPerformFinish += ReenableChoices;

                void ReenableChoices()
                {
                    Player.FPP.OnPerformFinish -= ReenableChoices;
                    equipMainhand.SetEnabled(true);
                    equipOffhand.SetEnabled(true);
                }
            }
        }

        void DestroyItemOptions()
        {
            if (itemOptions != null) itemOptions.Destroy();
        }

        Item SwapItemWithHeldItem(Item item)
        {
            if (!_isHoldingItem) return Item.None;

            Item prevHeld = _heldItem;
            _heldItem = item;
            displayedItem.SetDisplayedItem(_heldItem);
            return prevHeld;
        }

        void HoldItem(Item item)
        {
            if (item == null || item.IsNone || item.Count < 1)
            {
                ReleaseHeldItem();
                return;
            }

            _isHoldingItem = true;
            _heldItem = item;
            CreateItemDisplay(item);
        }

        void ReleaseHeldItem()
        {
            _isHoldingItem = false;
            _heldItem = null;
            DestroyItemDisplay();
        }

        void PutBackHeldItem()
        {
            if (_heldItem == null) return;
            
            inventory.Bag.TryPut(_lastSelectedSlotIndex, _heldItem);
            ReleaseHeldItem();
            _lastSelectedSlotIndex = -1;
        }

        void CreateItemDisplay(Item item)
        {
            DestroyItemDisplay();
            displayedItem = Game.UI.Create<ItemDisplayUI>("Item Display");
            displayedItem.SetDisplayedItem(item);
        }

        void DestroyItemDisplay()
        {
            if (displayedItem != null) displayedItem.Destroy();
        }
    }
}