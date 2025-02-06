using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;

using UZSG.Entities;
using UZSG.Inventory;

using UZSG.Items;
using UZSG.UI.HUD;
using UZSG.Items.Weapons;
using UZSG.Data;

namespace UZSG.UI.HUD
{
    public class PlayerHUDVitalsUI : UIElement
    {
        public Player Player { get; internal set; }
        [Space]
        
        Dictionary<int, ItemSlotUI> _equipmentSlotUIs = new();
        Dictionary<int, ItemSlotUI> _hotbarSlotUIs = new();
        Selector selector;
        
        [Header("Elements")]
        [SerializeField] GameObject hotbar;
        public AmmoCounterHUD AmmoCounter;
        [SerializeField] GameObject equipment;
        [SerializeField, FormerlySerializedAs("equippedWeaponTMP")] TextMeshProUGUI equippedItemTMP;
        [SerializeField] WeaponDetailsUI weaponDetails;
        [SerializeField] AttributeBar HealthBar;
        [SerializeField] StaminaBar StaminaBar;
        [SerializeField] AttributeBar HungerBar;
        [SerializeField] AttributeBar HydrationBar;
        [SerializeField] AttributeBar XPBar;

        internal void Initialize(Player player)
        {
            if (player == null)
            {
                Game.Console.LogWithUnity($"Invalid player.");
                return;
            }

            Player = player;
            BindPlayerAttributes();
            InitializeEquipmentSlots();
            InitializeHotbarSlots();
            InitializeSelector();
            InitializeEvents();
        }

        void InitializeHotbarSlots()
        {
            /// Hotbar slots are created in runtime
            for (int i = 0; i < Player.Inventory.Hotbar.SlotCount; i++)
            {
                int index = 3 + i; /// 3 is hotbar starting index
                var slotUI = Game.UI.Create<ItemSlotUI>("Item Slot");
                slotUI.name = $"Hotbar Slot ({i})";
                slotUI.transform.SetParent(hotbar.transform);
                slotUI.Link(Player.Inventory.Hotbar[i]);
                slotUI.OnMouseDown += OnHotbarSlotClicked;
                _hotbarSlotUIs[index] = slotUI;
            }
        }

        void InitializeSelector()
        {
            selector = Game.UI.Create<Selector>("Selector");
            selector.EnableAnimations = false;
            selector.Hide();
        }

        void InitializeEquipmentSlots()
        {
            /// Equipment slots are already set
            int index = 1; /// Only 1 (mainhand) and 2 (offhand), as 0 (arms) is not displayed :)
            foreach (Transform child in equipment.transform)
            {
                if (child.TryGetComponent<ItemSlotUI>(out var slotUI)) /// there might be other GameObjects
                {
                    slotUI.Link(Player.Inventory.Equipment[index]);
                    slotUI.OnMouseDown += OnEquipmentSlotClick;
                    _equipmentSlotUIs[index] = slotUI;
                    index++;
                }
            }
        }

        void InitializeEvents()
        {
            Player.Actions.OnEquipEquipment += OnEquipEquipment;
            Player.Actions.OnEquipHotbar += OnEquipHotbar;
            Player.FPP.OnHeldItemChanged += OnHeldItemChanged;
            Player.FPP.OnFPPControllerChanged += OnFPPControllerChanged;
            Player.FPP.OnSelectedHotbarChanged += OnSelectedHotbarChanged;
            Player.Inventory.Bag.OnSlotItemChanged += OnBagSlotItemChanged;

            /// Gui events
            Player.InventoryWindow.FrameController.OnSwitchFrame += OnInvSwitchFrame;
            Player.InventoryWindow.OnOpened += () =>
            {
                ToggleEquipmentHudVisibility(Player.InventoryWindow.FrameController.CurrentFrame.Id == "equipment");
            };
            Player.InventoryWindow.OnClosed += () =>
            {
                equipment.gameObject.SetActive(true);
                selector.Show();
            };
        }

        void OnDestroy()
        {
            selector.Destruct();
        }

        void BindPlayerAttributes()
        {
            HealthBar.BindAttribute(Player.Attributes.Get("health"));
            StaminaBar.BindAttribute(Player.Attributes.Get("stamina"));
            HungerBar.BindAttribute(Player.Attributes.Get("hunger"));
            HydrationBar.BindAttribute(Player.Attributes.Get("hydration"));
            XPBar.BindAttribute(Player.Attributes.Get("experience"));
        }


        #region Event callbacks (from all over)

        void OnHotbarSlotClicked(object sender, ItemSlotUI.ClickedContext click)
        {
            var slot = ((ItemSlotUI) sender).Slot;

            if (!Player.InventoryWindow.IsVisible) return;

            if (click.Button == PointerEventData.InputButton.Left)
            {
                if (Player.InventoryWindow.IsHoldingItem)
                {
                    if (slot.IsEmpty)
                    {
                        slot.Put(Player.InventoryWindow.TakeHeldItem());
                    }
                    else if (slot.Item.Is(Player.InventoryWindow.HeldItem))
                    {
                        var heldItem = Player.InventoryWindow.TakeHeldItem();
                        if (slot.TryStack(heldItem, out var excess))
                        {
                            if (!excess.IsNone) Player.InventoryWindow.HoldItem(excess);
                        } else /// swap items
                        {
                            Player.InventoryWindow.HoldItem(slot.TakeAll());
                        }
                    }
                    else /// swap items
                    {
                        Item taken = slot.TakeAll();
                        var previousHeld = Player.InventoryWindow.SwapHeldWith(taken);
                        slot.Put(previousHeld);
                    }
                }
                else
                {
                    if (!slot.IsEmpty)
                    {
                        Player.InventoryWindow.HoldItem(slot.TakeAll());
                    }
                }
            }
            else if (click.Button == PointerEventData.InputButton.Right)
            {
                if (Player.InventoryWindow.IsHoldingItem) /// put 1 to selected slot
                {
                    if (slot.IsEmpty)
                    {
                        slot.Put(Player.InventoryWindow.HeldItem.Take(1));
                    }
                    else if (slot.Item.Is(Player.InventoryWindow.HeldItem))
                    {
                        Item ofOne = Player.InventoryWindow.HeldItem.Take(1);
                        if (slot.TryStack(ofOne, out var excess))
                        {
                            if (!excess.IsNone) Player.InventoryWindow.HoldItem(excess);
                        } else
                        {
                            Player.InventoryWindow.HeldItem.Stack(ofOne);
                        }
                    }
                }
            }
        }

        void OnEquipmentSlotClick(object sender, ItemSlotUI.ClickedContext e)
        {
            if (!Player.InventoryWindow.IsVisible) return;

            var slotUI = (ItemSlotUI) sender; 
            var slot = slotUI.Slot; 

            if (slot.IsEmpty) return;

            if (Player.InventoryWindow.ItemOptions.IsVisible)
            {
                Player.InventoryWindow.ItemOptions.Destruct();
            }
            
            /// Create equipped Item options
            var options = Game.UI.Create<ChoiceWindow>("Choice Window", show: false);
            Game.UI.CreateBlocker(forElement: options, onClick: () =>
            {
                options.Destruct();
            });

            options.Pivot = UI.Pivot.BottomRight;
            options.Position = Vector2.zero;
            options.Label = slot.Item.Data.DisplayName;
            
            options.AddChoice("Unequip")
            .AddCallback(() =>
            {
                if (Player.Inventory.Bag.IsFull) return;

            });

            options.Show();
        }

        void OnBagSlotItemChanged(object sender, ItemSlot.ItemChangedContext e)
        {
            if (e.OldItem.Data is AmmoData || e.NewItem.Data is AmmoData)
            if (Player.FPP.FPPItemController is GunWeaponController gun)
            {
                AmmoCounter.SetReserve(gun.Reserve);
            }
        }

        void OnEquipEquipment(ItemData data, EquipmentIndex index)
        {
            if (_equipmentSlotUIs.TryGetValue((int) index, out var slotUI))
            {
                selector.Select(slotUI.transform as RectTransform);
                selector.Show();
            }
        }

        void OnEquipHotbar(ItemData data, int index)
        {
            if (_hotbarSlotUIs.TryGetValue(index + 3, out var slotUI))
            {
                selector.Select(slotUI.transform as RectTransform);
                selector.Show();
            }
        }

        void OnHeldItemChanged(ItemData itemData)
        {
            if (itemData == null) return;

            if (!string.IsNullOrEmpty(itemData.DisplayName))
            {
                equippedItemTMP.text = itemData.DisplayName;
            }
            else
            {
                equippedItemTMP.text = string.Empty;
            }   
        }

        void OnFPPControllerChanged(FPPItemController fppItemController)
        {
            if (fppItemController == null) return;

            if (fppItemController is GunWeaponController gun)
            {
                weaponDetails.SetGunVariant();
                AmmoCounter.DisplayWeaponStats(gun);
            }
            else
            {
                weaponDetails.SetMeleeVariant();
            }

            equippedItemTMP.text = fppItemController.ItemData.DisplayName;
        }

        void OnSelectedHotbarChanged(int index)
        {
            if (index < 3)
            {
                if (_equipmentSlotUIs.ContainsKey(index))
                {
                    selector.Select(_equipmentSlotUIs[index].transform as RectTransform);
                    selector.Show();
                }
            }
            else
            {
                if (_hotbarSlotUIs.ContainsKey(index))
                {
                    selector.Select(_hotbarSlotUIs[index].transform as RectTransform);
                    selector.Show();
                }
            }

        }

        void OnInvSwitchFrame(FrameController.SwitchFrameContext context)
        {
            if (context.Status == FrameController.SwitchStatus.Started)
            {
                ToggleEquipmentHudVisibility(context.Next == "equipment");
            }
        }

        #endregion
        
        
        void ToggleEquipmentHudVisibility(bool visible)
        {
            if (visible)
            {
                equipment.gameObject.SetActive(true);
                selector.Show();
            }
            else
            {
                equipment.gameObject.SetActive(false);
                if (Player.FPP.SelectedHotbarIndex < 3)
                {
                    selector.Hide();
                }
            }
        }
    }
}
