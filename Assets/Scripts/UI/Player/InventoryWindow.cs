using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.PointerEventData.InputButton;

using UZSG.Data;
using UZSG.Inventory;
using UZSG.Items;
using UZSG.Entities;
using static UZSG.UI.ItemSlotUI.ClickType;

namespace UZSG.UI.Players
{
    public partial class InventoryWindow : Window, IInitializeable
    {
        const string CraftingTitle = "Crafting"; /// this should be read from a Lang file
        
        public Player Player { get; set; }
        public PlayerInventoryManager Inventory => Player.Inventory;
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
        bool _hasExternalContainerOpen;

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
        public bool EnableSelector { get; set; } = false;
        Selector selector;
        public Selector Selector => selector;

        [Header("UI Elements")]
        [SerializeField] HotbarUI hotbarUI;
        [SerializeField] ItemDetailsUI itemDetailsUI;
        [SerializeField] GameObject bag;
        [SerializeField] GameObject selectorPrefab;
        [SerializeField] Button closeButton;
        
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
                Game.Console.LogWithUnity($"Invalid player.");
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
            // InitializeSelector();
            // itemDetailsUI = Game.UI.Create<ItemDetailsUI>("Item Details UI");
            frameController.SwitchToFrame("bag", force: true);
            frameController.OnSwitchFrame += (context) =>
            {
                if (context.NextId != "bag" &&
                    IsVisible &&
                    EnableSelector)
                {
                    selector?.Hide();
                }
            };
            InitializeInputs();
            closeButton.onClick.AddListener(Hide);
            Hide();
        }
        
        void InitializeSelector()
        {
            selector = Game.UI.Create<Selector>("Selector");
            selector.Hide();
            selector.Rect.SetParent(transform);
            
            OnOpened += () =>
            {
                if (EnableSelector)
                {
                    selector?.Show();
                }
            };
            OnClosed += () =>
            {
                if (EnableSelector)
                {
                    selector?.Hide();
                }
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
                var slotUI = Game.UI.Create<ItemSlotUI>("Item Slot", parent: bag.transform);
                slotUI.name = $"Slot ({i})";
                slotUI.Link(Inventory.Bag[i]);
                slotUI.OnMouseDown += OnItemSlotClicked;
                _bagSlotUIs.Add(i, slotUI);
                slotUI.Show();
            }
        }

        void InitializeEquipmentSlotUIs()
        {
            foreach (var slot in Inventory.Equipment.Slots)
            {
                
            }
        }

        void InitializeInputs()
        {
            // actionMap = Game.Main.GetActionMap("Inventory");
            // inputs = Game.Main.GetActionsFromMap(actionMap);

            // inputs["Hide/Show"].performed += (context) =>
            // {
            //     if (IsVisible)
            //         Hide();
            //     else 
            //         Show();
            // };
        }

        protected override void OnShow()
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
                
            if (EnableSelector)
            {
                selector?.Hide();
            }
            // playerCraftingGUI.OnShow();
            // actionMap.Enable();
            Game.UI.SetCursorVisible(true);
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

        protected override void OnHide()
        {
            // actionMap.Disable();
            PutBackHeldItem();
            DestroyItemOptions();
            _selectedSlotUI = null;
            if (EnableSelector)
            {
                selector?.Hide();
            }
            Game.UI.SetCursorVisible(false);
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
            Item prevHeld = _heldItem;
            _heldItem = item;
            displayedItem.SetDisplayedItem(_heldItem);
            return prevHeld;
        }

        #endregion


        #region Event callbacks

        void OnHeldItemChanged(ItemData data)
        {
            
        }

        public void OnItemSlotClicked(object sender, ItemSlotUI.ClickedContext click)
        {
            _selectedSlotUI = (ItemSlotUI) sender;
            _selectedSlot = _selectedSlotUI.Slot;

            if (!_isHoldingItem)
            {
                itemDetailsUI.Item = _selectedSlot.Item;
                itemDetailsUI.Show();
            }

            if (click.Button == Left)
            {
                DestroyItemOptions();
                
                if (click.ClickType == ShiftClick && _hasExternalContainerOpen)
                {
                    FastDepositToExternalContainer(_selectedSlot);
                    return;
                }

                if (_isHoldingItem)
                {
                    if (_selectedSlot.IsEmpty)
                    {
                        _selectedSlot.Put(TakeHeldItem());
                    }
                    else if (_selectedSlot.Item.Is(HeldItem))
                    {
                        var heldItem = TakeHeldItem();
                        if (_selectedSlot.TryStack(heldItem, out var excess))
                        {
                            if (!excess.IsNone) HoldItem(excess);
                        }
                        else /// swap items
                        {
                            HoldItem(_selectedSlot.TakeAll());
                            _selectedSlot.Put(heldItem);
                        }
                    }
                    else /// swap items
                    {
                        Item taken = _selectedSlot.TakeAll();
                        var previousHeld = SwapHeldWith(taken);
                        _selectedSlot.Put(previousHeld);
                    }
                }
                else
                {
                    if (!_selectedSlot.IsEmpty)
                    {
                        HoldItem(_selectedSlot.TakeAll());
                        _lastSelectedSlotIndex = _selectedSlot.Index;
                    }
                }
            }
            else if (click.Button == Right)
            {
                if (_isHoldingItem) /// put 1 to selected slot
                {
                    _isPutting = true;
                    if (_selectedSlot.IsEmpty)
                    {
                        _selectedSlot.Put(_heldItem.Take(1));
                    }
                    else if (_selectedSlot.Item.Is(HeldItem))
                    {
                        Item ofOne = TakeHeldItemSingle();
                        if (_selectedSlot.TryStack(ofOne, out var excess))
                        {
                            if (!excess.IsNone) HoldItem(excess);
                        } 
                        else /// swap items
                        {
                            _heldItem.Stack(ofOne); /// return the item we took earlier
                            Item taken = _selectedSlot.TakeAll();
                            var previousHeld = SwapHeldWith(taken);
                            _selectedSlot.Put(previousHeld);
                        }
                    }
                }
                else
                {
                    if (!_selectedSlot.IsEmpty)
                    {
                        DestroyItemOptions();
                        CreateItemOptions();
                    }
                    else
                    {
                        DestroyItemOptions();
                        return;
                    }
                    
                    // _isGetting = true;
                    // HoldItem(inventory.Bag.TakeItems(_selectedSlot.Index, 1));
                    // _isGetting = false;
                }
            }
        }

        void FastDepositToExternalContainer(ItemSlot selectedSlot)
        {
            Item toDeposit = selectedSlot.TakeAll();
            if (externalContainer.CanPutItem(toDeposit))
            {
                externalContainer.TryPutNearest(toDeposit);
            }
        }

        #endregion


        #region Item options

        void CreateItemOptions()
        {
            itemOptions = Game.UI.Create<ChoiceWindow>("Choice Window");
            itemOptions.Position = _selectedSlotUI.Rect.position;
            itemOptions.Label = _selectedSlot.Item.Data.DisplayNameTranslatable;
            Game.UI.CreateBlocker(itemOptions, onClick: () =>
            {
                DestroyItemOptions();
            });

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

                itemOptions.OnClosed += () => /// be sure to unsubscribe on UI close
                {
                    Player.FPP.OnPerformFinished -= ReenableChoices;
                };
                Player.FPP.OnPerformFinished += ReenableChoices;

                void ReenableChoices()
                {
                    Player.FPP.OnPerformFinished -= ReenableChoices;
                    equipMainhand.SetEnabled(true);
                    equipOffhand.SetEnabled(true);
                }
            }
        }

        void DestroyItemOptions()
        {
            if (itemOptions != null) itemOptions.Destruct();
        }

        #endregion

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
            
            var heldItem = TakeHeldItem();
            var lastSlot = Inventory.Bag[_lastSelectedSlotIndex];
            if (lastSlot.IsEmpty)
            {
                Inventory.Bag.TryPutAt(_lastSelectedSlotIndex, heldItem);
            } else
            {
                if (lastSlot.TryStack(heldItem, out Item excess))
                {
                    Player.Inventory.DropItem(excess);
                }
                else
                {
                    Player.Inventory.DropItem(heldItem);
                }
            }
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