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
using TMPro;
using UZSG.Objects;
using UZSG.Crafting;

namespace UZSG.UI
{
    public partial class PlayerInventoryUI : Window, IInitializeable
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
        bool _isHoldingShift;
        bool _isHoldingCtrl;
        bool _isHoldingAlt;
        Item _heldItem;
        ItemSlot _selectedSlot;
        ItemSlotUI _selectedSlotUI;
        Dictionary<int, ItemSlotUI> _bagSlotUIs = new();
        List<FadeableElement> _fadeableElements = new();
        
        ChoiceWindow itemOptions;
        bool _hasDisplayedItem;
        ItemDisplayUI displayedItem;
        Selector selector;

        [Header("Inventory Components")]
        [SerializeField] FrameController frameController;
        [SerializeField] GameObject bag;
        [SerializeField] Transform craftingFrame;
        [SerializeField] CraftingGUI playerCraftingGUI;
        [SerializeField] TextMeshProUGUI titleText;
        
        [Header("Prefabs")]
        [SerializeField] GameObject selectorPrefab;
        
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
            InitializeElements();
            InitializeEvents();
            InitializeBagSlotUIs();
            InitializeCraftingGUI();
            selector = Instantiate(selectorPrefab, transform).GetComponent<Selector>();
            frameController.SwitchToFrame("bag", force: true);
            InitializeInputs();
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

        void InitializeCraftingGUI()
        {
            playerCraftingGUI.SetPlayer(Player);   
        }

        void InitializeInputs()
        {
            actionMap = Game.Main.GetActionMap("Inventory");
            inputs = Game.Main.GetActionsFromMap(actionMap);

            inputs["Shift"].performed += (context) =>
            {
                _isHoldingShift = context.ReadValue<float>() > 0; /// unsure
            };
            inputs["Ctrl"].performed += (context) =>
            {
                _isHoldingCtrl = context.ReadValue<float>() > 0; /// unsure
            };
            inputs["Alt"].performed += (context) =>
            {
                _isHoldingAlt = context.ReadValue<float>() > 0; /// unsure
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

            playerCraftingGUI.ResetDisplayed();
            playerCraftingGUI.SetPlayer(Player);
            playerCraftingGUI.AddRecipesById(Player.SaveData.KnownRecipes);

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
            CreateItemDisplay(item);
        }

        /// <summary>
        /// Replace the Player Crafting GUI with the Workstation GUI.
        /// </summary>
        public void SetWorkstation(Workstation workstation)
        {
            if (workstation == null) return; /// why would be null tho

            playerCraftingGUI.Hide();
            workstation.GUI.transform.SetParent(craftingFrame, false);
            frameController.SwitchToFrame("crafting", instant: true);
            /// Set the title text to the workstation's name
            titleText.text = workstation.WorkstationData.WorkstationName;
            Show();
        }

        public void ResetToPlayerCraftingGUI()
        {
            titleText.text = CraftingTitle;
            playerCraftingGUI.Show();
        }

        #endregion


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
            _selectedSlot = Inventory.Bag[_selectedSlotUI.Index];

            if (e.Pointer.button == PointerEventData.InputButton.Left)
            {
                DestroyItemOptions();

                if (_isHoldingItem)
                {
                    if (Inventory.Bag.TryPutAt(_selectedSlot.Index, _heldItem))
                    {
                        ReleaseHeldItem();
                    }
                    else /// swap items
                    {
                        Item tookItem = Inventory.Bag.TakeFrom(_selectedSlot.Index);
                        Inventory.Bag.TryPutAt(_selectedSlot.Index, SwapItemWithHeldItem(tookItem));
                    }
                }
                else
                {
                    if (_selectedSlot.IsEmpty) return;

                    var itemTaken = Inventory.Bag.TakeFrom(_selectedSlot.Index);
                    if (itemTaken.CompareTo(_selectedSlot.Item))
                    {
                        
                    }
                    HoldItem(itemTaken);
                    _lastSelectedSlotIndex = _selectedSlot.Index;
                }
            }
            else if (e.Pointer.button == PointerEventData.InputButton.Right)
            {
                if (_isHoldingItem) /// put 1 to selected slot
                {
                    if (_selectedSlot.IsEmpty)
                    {
                        _isPutting = true;

                        Item toPut = _heldItem.Take(1);
                        if (Inventory.Bag.TryPutAt(_selectedSlot.Index, toPut))
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
                            _heldItem.TryCombine(toPut, out excess); /// return item to hand
                        }
                        HoldItem(_heldItem);
                    }
                    else /// swap items
                    {
                        Item taken = Inventory.Bag.TakeFrom(_selectedSlot.Index);
                        var itemToPut = SwapItemWithHeldItem(taken);
                        Inventory.Bag.TryPutAt(_selectedSlot.Index, itemToPut);
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

        Item SwapItemWithHeldItem(Item item)
        {
            if (!_isHoldingItem) return Item.None;

            Item prevHeld = _heldItem;
            _heldItem = item;
            displayedItem.SetDisplayedItem(_heldItem);
            return prevHeld;
        }

        void ReleaseHeldItem()
        {
            _isHoldingItem = false;
            _heldItem = Item.None;
            DestroyItemDisplay();
        }

        void PutBackHeldItem()
        {
            if (_heldItem.IsNone) return;
            
            Inventory.Bag.TryPutAt(_lastSelectedSlotIndex, _heldItem);
            ReleaseHeldItem();
            _lastSelectedSlotIndex = -1;
        }

        void CreateItemDisplay(Item item)
        {
            DestroyItemDisplay();
            displayedItem = Game.UI.Create<ItemDisplayUI>("Item Display");
            displayedItem.SetDisplayedItem(item);
            _hasDisplayedItem = true;
        }

        void DestroyItemDisplay()
        {
            if (displayedItem != null)
            {
                Destroy(displayedItem.gameObject);
            }
            _hasDisplayedItem = false;
        }
    }
}