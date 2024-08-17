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

        Dictionary<int, ItemSlotUI> _hotbarSlotUIs = new();
        
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
                slotUI.Index = index;
                
                slotUI.OnMouseDown += OnClickHotbarSlot;
                slotUI.OnHoverStart += OnStartHoverSlot;
                slotUI.OnHoverEnd += OnEndHoverSlot;
                _hotbarSlotUIs[index] = slotUI;
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
            _hotbarSlotUIs[hotbarOffset].SetDisplayedItem(e.ItemSlot.Item);
        }

        void OnStartHoverSlot(object sender, ItemSlotUI.ClickedContext e)
        {
            
        }

        void OnEndHoverSlot(object sender, ItemSlotUI.ClickedContext e)
        {
            
        }

        void OnClickHotbarSlot(object sender, ItemSlotUI.ClickedContext e)
        {
            ItemSlotUI slotUI = (ItemSlotUI) sender;
            // _selectedSlot = inventory.Bag[slotUI.Index];

            if (e.Pointer.button == PointerEventData.InputButton.Left)
            {

            } else if (e.Pointer.button == PointerEventData.InputButton.Right)
            {

            }
        }

        void UpdateAmmoCounter()
        {
            
        }

        #endregion


    }
}
