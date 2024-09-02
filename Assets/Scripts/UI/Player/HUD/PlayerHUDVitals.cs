using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

using UZSG.Entities;
using UZSG.Inventory;
using UZSG.Systems;
using UZSG.Items;
using UZSG.UI.HUD;
using UZSG.Items.Weapons;
using UZSG.Data;

namespace UZSG.UI.HUD
{
    public class PlayerHUDVitals : Window
    {
        public Player Player;
        [Space]
        
        ItemSlot _lastSelectedSlot;
        Dictionary<int, ItemSlotUI> _equipmentSlotUIs = new();
        
        [Header("Elements")]
        public GameObject hotbar;
        public AmmoCounterHUD AmmoCounter;
        public GameObject equipment;
        public TextMeshProUGUI equippedWeaponTMP;
        public WeaponDetailsUI weaponDetails;
        public AttributeBar HealthBar;
        public AttributeBar StaminaBar;
        public AttributeBar HungerBar;
        public AttributeBar HydrationBar;
        public AttributeBar XPBar;

        internal void Initialize(Player player)
        {
            if (player == null)
            {
                Game.Console.LogAndUnityLog($"Invalid player.");
                return;
            }

            Player = player;
            BindPlayerAttributes();
            InitializeEquipmentSlots();
            InitializeHotbarSlots();
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
            }
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
            Player.FPP.OnChangeHeldItem += OnChangeHeldItem;
            Player.InventoryGUI.frameController.OnSwitchFrame += OnInvSwitchFrame;
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

        void OnHotbarSlotClicked(object sender, ItemSlotUI.ClickedContext e)
        {
            var slot = ((ItemSlotUI) sender).Slot;

            if (!Player.InventoryGUI.IsVisible) return;

            if (e.Button == PointerEventData.InputButton.Left)
            {
                if (Player.InventoryGUI.IsHoldingItem)
                {
                    var heldItem = Player.InventoryGUI.HeldItem;

                    if (slot.IsEmpty || slot.Item.CompareTo(heldItem))
                    {
                        slot.TryCombine(Player.InventoryGUI.TakeHeldItem(), out var excess);
                        if (!excess.IsNone)
                        {
                            Player.InventoryGUI.HoldItem(excess);
                        }
                    }
                    else /// item diff, swap
                    {
                        var tookItem = slot.TakeAll();
                        var prevHeld = Player.InventoryGUI.SwapHeldWith(tookItem);
                        slot.Put(prevHeld);
                    }
                }
                else
                {
                    if (slot.IsEmpty) return;

                    Player.InventoryGUI.HoldItem(slot.TakeAll());
                    _lastSelectedSlot = slot;
                }
            }
            else if (e.Button == PointerEventData.InputButton.Right)
            {

            }
        }

        void OnEquipmentSlotClick(object sender, ItemSlotUI.ClickedContext e)
        {
            if (!Player.InventoryGUI.IsVisible) return;

            var slotUI = (ItemSlotUI) sender; 
            var slot = slotUI.Slot; 

            if (slot.IsEmpty) return;

            if (Player.InventoryGUI.ItemOptions.IsVisible)
            {
                Player.InventoryGUI.ItemOptions.Destroy();
            }
            
            /// Create equipped Item options
            var options = Game.UI.Create<ChoiceWindow>("Choice Window", show: false);
            Game.UI.CreateBlocker(forElement: options, onClick: () =>
            {
                options.Destroy();
            });

            options.Pivot = UI.Pivot.BottomRight;
            options.Position = Vector2.zero;
            options.Label = slot.Item.Data.Name;
            
            options.AddChoice("Unequip")
            .AddCallback(() =>
            {
                if (Player.Inventory.Bag.IsFull) return;

            });

            options.Show();
        }

        void OnChangeHeldItem(HeldItemController heldItem)
        {
            if (heldItem == null) return;

            if (heldItem is GunWeaponController gun)
            {
                weaponDetails.SetGunVariant();
                AmmoCounter.DisplayWeaponStats(gun);
            }
            else
            {
                weaponDetails.SetMeleeVariant();
            }

            equippedWeaponTMP.text = heldItem.ItemData.Name;
        }

        void OnInvSwitchFrame(FrameController.SwitchFrameContext context)
        {
            if (context.Next == "equipment")
            {
                equipment.gameObject.SetActive(true);
            }
            else
            {
                equipment.gameObject.SetActive(false);
            }
        }

        #endregion
    }
}
