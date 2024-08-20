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

namespace UZSG.UI.HUD
{
    public class PlayerHUDVitals : Window
    {
        public Player Player;
        [Space]
        
        ItemSlot _lastSelectedSlot;
        
        [Header("Elements")]
        public GameObject hotbar;
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

        void InitializeEvents()
        {
            Player.Inventory.Hotbar.OnSlotItemChanged += OnHotbarSlotChanged;
        }

        void BindPlayerAttributes()
        {
            HealthBar.BindAttribute(Player.Attributes.Get("health"));
            StaminaBar.BindAttribute(Player.Attributes.Get("stamina"));
            HungerBar.BindAttribute(Player.Attributes.Get("hunger"));
            HydrationBar.BindAttribute(Player.Attributes.Get("hydration"));
            XPBar.BindAttribute(Player.Attributes.Get("experience"));
        }


        #region Callbacks

        void OnHotbarSlotChanged(object sender, ItemSlot.ItemChangedContext e)
        {
            var hotbarOffset = e.ItemSlot.Index + 3;
        }

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

        #endregion
    }
}
