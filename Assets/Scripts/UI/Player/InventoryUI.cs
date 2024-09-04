using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Inventory;
using UZSG.Items;
using UZSG.Entities;

using static UnityEngine.EventSystems.PointerEventData.InputButton;
using static UZSG.UI.ItemSlotUI.ClickType;

namespace UZSG.UI.Players
{
    public partial class InventoryUI : Window, IInitializeable
    {
        const string CraftingTitle = "Crafting"; /// this should be read from a Lang file
        
        public Player Player;
        public InventoryHandler Inventory => Player.Inventory;
        [Space]

        bool _isInitialized;
        public bool IsInitialized => _isInitialized;

        int _lastSelectedSlotIndex;
        bool _isHoldingItem = false;
        /// <summary>
        /// If the cursor is currently holding an item.
        /// </summary>
        public bool IsHoldingItem => _isHoldingItem;
        bool _isPutting;
        bool _isGetting;
        bool _hasStorageGuiOpen;

        Item _heldItem = Item.None;
        public Item HeldItem => _heldItem;
        ItemSlot _selectedSlot;
        ItemSlotUI _selectedSlotUI;
        Dictionary<int, ItemSlotUI> _bagSlotUIs = new();
        List<FadeableElement> _fadeableElements = new();
        
        ChoiceWindow itemOptions;
        public ChoiceWindow ItemOptions => itemOptions;
        bool _hasDisplayedItem;
        ItemDisplayUI displayedItem;
        Selector selector;
        public Selector Selector => selector;

        [Header("Inventory Components")]
        [SerializeField] HotbarUI hotbarUI;
        [SerializeField] ItemDetailsUI itemDetailsUI;
        [SerializeField] GameObject bag;
        [SerializeField] GameObject selectorPrefab;
        [SerializeField] Button closeButton;
        
        InputActionMap actionMap;
        Dictionary<string, InputAction> inputs = new();

        void Update()
        {
            if (_hasDisplayedItem)
            {
                displayedItem.transform.position = Input.mousePosition;
            }
        }
        
        public void Initialize(Player player)
        {
            if (player == null)
            {
                Game.Console.LogAndUnityLog($"Invalid player.");
                return;
            }
            if (_isInitialized) return;
            _isInitialized = true;
            
            Player = player;

            hotbarUI.Initialize(player);

            InitializeElements();
            InitializeEvents();
            InitializeBagSlotUIs();
            InitializeEquipmentSlotUIs();
            // InitializeCraftingGUI();
            InitializeSelector();
            // itemDetailsUI = Game.UI.Create<ItemDetailsUI>("Item Details UI");
            frameController.SwitchToFrame("bag", force: true);
            InitializeInputs();
            closeButton.onClick.AddListener(Hide);
            Hide();
        }

        void InitializeSelector()
        {
            selector = Game.UI.Create<Selector>("Selector", show: false);
            selector.Rect.SetParent(transform);
            
            OnOpen += () =>
            {
                selector.Show();
            };
            OnClose += () =>
            {
                selector.Hide();
            };
        }

        void InitializeElements()
        {
            _fadeableElements = new();
            foreach (Graphic graphic in GetComponentsInChildren<Graphic>())
            {
                if (!graphic.TryGetComponent<FadeableElement>(out var fadeableElement))
                {
                    fadeableElement = graphic.gameObject.AddComponent<FadeableElement>();
                }
                _fadeableElements.Add(fadeableElement);
            }
        }

        void InitializeEvents()
        {
            frameController.OnSwitchFrame += OnSwitchFrame;
        }

        void InitializeBagSlotUIs()
        {
            int maxSlots = Inventory.Bag.SlotCount;
            for (int i = 0; i < maxSlots; i++)
            {
                var slot = Game.UI.Create<ItemSlotUI>("Item Slot");
                slot.name = $"Slot ({i})";
                slot.transform.SetParent(bag.transform);
                slot.Index = i;
                slot.Link(Inventory.Bag[i]);
                slot.OnHoverStart += OnBagSlotHoverStart;
                slot.OnHoverEnd += OnBagSlotHoverEnd;
                slot.OnMouseDown += OnBagSlotClick;
                _bagSlotUIs.Add(i, slot);
                slot.Show();
            }
        }

        void InitializeEquipmentSlotUIs()
        {
            foreach (var slot in Inventory.Equipment.Slots)
            {
                
            }
        }

        void InitializeCraftingGUI()
        {
            playerCraftingGUI.SetPlayer(Player);   
        }

        void InitializeInputs()
        {
            actionMap = Game.Main.GetActionMap("Inventory");
            inputs = Game.Main.GetActionsFromMap(actionMap);

            inputs["Hide/Show"].performed += (context) =>
            {
                ToggleVisibility();
            };

            inputs["Hide"].performed += (context) =>
            {
                SetVisible(false);
            };
        }

        public override void OnShow()
        {
            if (Game.UI.EnableScreenAnimations)
            {
                LeanTween.cancel(gameObject);
                // FadeIn();
                AnimateEntry();
            }
            else
            {
                rect.localScale = Vector3.one;
            }

            // playerCraftingGUI.OnShow();
            selector.Hide();
            actionMap.Enable();
            Game.UI.ToggleCursor(true);
        }


        void FadeIn()
        {
            foreach (var element in _fadeableElements)
            {
                element.FadeIn(FadeDuration);
            }
        }

        void AnimateEntry()
        {
            rect.localScale = new(0.95f, 0.95f, 0.95f);
            LeanTween.scale(gameObject, Vector3.one, FadeDuration)
            .setEase(LeanTweenType.easeOutExpo);
        }

        public override void OnHide()
        {
            actionMap.Disable();
            PutBackHeldItem();
            DestroyItemOptions();
            _selectedSlotUI = null;
            selector.Hide();
            Game.UI.ToggleCursor(false);
        }


        #region Public methods

        public void HoldItem(Item item)
        {
            if (item.IsNone || item.Count < 1)
            {
                ReleaseHeldItem();
                return;
            }

            _isHoldingItem = true;
            _heldItem = item;
            _heldItem.OnChanged += UpdateItemDisplay;
            ReplaceItemDisplay(item);
        }
        
        public Item TakeHeldItem()
        {
            Item toReturn = new(_heldItem);
            ReleaseHeldItem();
            return toReturn;
        }
        public Item TakeHeldItemSingle()
        {
            Item toReturn = _heldItem.Take(1);
            return toReturn;
        }
        /// <summary>
        /// Swaps held items, holding the new given Item, and returning the previously held Item.
        /// </summary>
        public Item SwapHeldWith(Item item)
        {
            if (!_isHoldingItem) return Item.None;

            Item prevHeld = _heldItem;
            _heldItem = item;
            displayedItem.SetDisplayedItem(_heldItem);
            return prevHeld;
        }

        #endregion


        #region Event callbacks

        void OnBagSlotContentChanged(object sender, ItemSlot.ItemChangedContext e)
        {
            _bagSlotUIs[e.ItemSlot.Index].SetDisplayedItem(e.ItemSlot.Item);
        }

        void OnBagSlotHoverStart(object sender, ItemSlotUI.ClickedContext e)
        {
            var slot = sender as ItemSlotUI;
            selector.Select(slot.transform as RectTransform);
        }

        void OnBagSlotHoverEnd(object sender, ItemSlotUI.ClickedContext e)
        {
        }

        void OnBagSlotClick(object sender, ItemSlotUI.ClickedContext e)
        {
            _selectedSlotUI = (ItemSlotUI) sender;
            _selectedSlot = _selectedSlotUI.Slot;

            if (!_isHoldingItem)
            {
                itemDetailsUI.Item = _selectedSlot.Item;
                itemDetailsUI.Show();
            }

            if (e.Button == Left)
            {
                DestroyItemOptions();
                
                if (e.ClickType == ShiftClick && _hasStorageGuiOpen)
                {
                    FastDepositToStorage(_selectedSlot);
                    return;
                }

                if (_isHoldingItem)
                {
                    if (_selectedSlot.IsEmpty || _selectedSlot.Item.CompareTo(_heldItem))
                    {
                        var heldItem = TakeHeldItem();
                        _selectedSlot.TryCombine(heldItem, out var excess);
                        if (!excess.IsNone)
                        {
                            HoldItem(excess);
                        }
                    }
                    else /// swap items
                    {
                        Item taken = _selectedSlot.TakeAll();
                        var prevHeld = SwapHeldWith(taken);
                        _selectedSlot.Put(prevHeld);
                    }
                }
                else
                {
                    if (_selectedSlot.IsEmpty) return;

                    HoldItem(_selectedSlot.TakeAll());
                    _lastSelectedSlotIndex = _selectedSlot.Index;
                }
            }
            else if (e.Button == Right)
            {
                if (_isHoldingItem) /// put 1 to selected slot
                {
                    if (_selectedSlot.IsEmpty || _selectedSlot.Item.CompareTo(_heldItem))
                    {
                        _isPutting = true;

                        Item ofOne = _heldItem.Take(1);
                        if (_selectedSlot.Item.CanStackWith(ofOne))
                        {
                            _selectedSlot.TryCombine(ofOne, out _);
                        }
                        else /// return item
                        {
                            _heldItem.Combine(ofOne);
                        }

                        return;
                    }
                    else /// swap items
                    {
                        Item taken = _selectedSlot.TakeAll();
                        var prevHeld = SwapHeldWith(taken);
                        _selectedSlot.Put(prevHeld);
                    }
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

        void FastDepositToStorage(ItemSlot selectedSlot)
        {
            Item toDeposit = selectedSlot.TakeAll();
            if (externalContainer.CanPutItem(toDeposit))
            {
                externalContainer.TryPutNearest(toDeposit);
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
            // itemOptions.Position = _selectedSlotUI.Rect.position;
            itemOptions.Position = Vector2.zero;
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
                if (!equipped.IsNone) Player.FPP.ReleaseItem(equipped.Data);
            });
            
            var equipOffhand = itemOptions.AddChoice("Equip Offhand")
            .AddCallback(() =>
            {
                var equipped = Inventory.Equipment.Mainhand.Item;
                ItemSlot.SwapContents(Inventory.Equipment.Offhand, _selectedSlot);
                Player.FPP.HoldItem(item.Data);
                if (!equipped.IsNone) Player.FPP.ReleaseItem(equipped.Data);
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


        #region Held item methods

        /// <summary>
        /// Releases the Held Item from the grasp of the Player.
        /// </summary>
        void ReleaseHeldItem()
        {
            _isHoldingItem = false;
            _heldItem = Item.None;
            _heldItem.OnChanged -= UpdateItemDisplay;
            DestroyItemDisplay();
        }

        /// <summary>
        /// Puts back the Held Item to the Slot where it came from.
        /// </summary>
        void PutBackHeldItem()
        {
            if (_heldItem.IsNone) return;
            
            Inventory.Bag.TryPutAt(_lastSelectedSlotIndex, _heldItem);
            ReleaseHeldItem();
            _lastSelectedSlotIndex = -1;
        }

        /// <summary>
        /// Create a new Item Display element. Destroys the old one.
        /// </summary>
        void ReplaceItemDisplay(Item item)
        {
            DestroyItemDisplay();
            displayedItem = Game.UI.Create<ItemDisplayUI>("Item Display");
            displayedItem.SetDisplayedItem(item);
            _hasDisplayedItem = true;
        }

        void UpdateItemDisplay()
        {
            if (_heldItem.IsNone || _heldItem.Count <= 0)
            {
                ReleaseHeldItem();
            }
            else
            {
                if (displayedItem != null)
                {
                    displayedItem.SetDisplayedItem(_heldItem);
                }
            }
        }

        void DestroyItemDisplay()
        {
            if (displayedItem != null)
            {
                Destroy(displayedItem.gameObject);
            }
            _hasDisplayedItem = false;
        }

        #endregion
    }
}