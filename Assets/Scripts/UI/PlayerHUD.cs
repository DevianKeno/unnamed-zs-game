using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UZSG.Entities;
using UZSG.Inventory;
using UZSG.Systems;

namespace UZSG.UI
{
    public interface IToggleable
    {
        public bool IsVisible { get; }
    }

    public class PlayerHUD : Window, IToggleable
    {
        public Player Player { get; private set; }
        Dictionary<int, ItemSlotUI> _hotbarSlotUIs = new();
        [SerializeField] InventoryHandler inventory;
        
        [Header("Elements")]
        public AttributeBar HealthBar;
        public AttributeBar StaminaBar;
        public AttributeBar HungerBar;
        public AttributeBar HydrationBar;
        public AttributeBar XPBar;

        [Header("Layout")]
        [SerializeField] GameObject container;

        internal void Initialize()
        {
            if (Player == null)
            {
                var msg = $"Failed to initialize Player HUD. Bind a Player first!";
                Game.Console.Log(msg);
                Debug.LogWarning(msg);
                return;
            }

            InitializeSlots();
            InitializeEvents();
        }

        void InitializeSlots()
        {
            /// Hotbar slots
            int maxSlots = inventory.Hotbar.SlotsCount;
            for (int i = 0; i < maxSlots; i++)
            {
                var slotUI = Game.UI.Create<ItemSlotUI>("item_slot");
                slotUI.name = $"Hotbar Slot ({i})";
                slotUI.transform.SetParent(container.transform);
                slotUI.Index = i;
                
                slotUI.OnMouseDown += OnClickHotbarSlot;
                slotUI.OnStartHover += OnStartHoverSlot;
                slotUI.OnEndHover += OnEndHoverSlot;
                _hotbarSlotUIs.Add(i, slotUI);
            }
            // _hotbarSlotUIs.transform.SetAsLastSibling();
        }

        void InitializeEvents()
        {
            Player.Inventory.Hotbar.OnSlotContentChanged += HotbarSlotChangedCallback;
        }

        public void BindPlayer(Player player)
        {
            Player = player;
            inventory = player.Inventory;
            BindPlayerAttributes();
            // StaminaBar.SetAttribute(Player.Vitals.GetAttributeFromId("stamina"));
        }

        void BindPlayerAttributes()
        {
            HealthBar.BindAttribute(Player.Vitals.GetAttributeFromId("health"));
            StaminaBar.BindAttribute(Player.Vitals.GetAttributeFromId("stamina"));
            HungerBar.BindAttribute(Player.Vitals.GetAttributeFromId("hunger"));
            HydrationBar.BindAttribute(Player.Vitals.GetAttributeFromId("hydration"));
        }

        #region Callbacks

        void HotbarSlotChangedCallback(object sender, SlotContentChangedArgs e)
        {
            _hotbarSlotUIs[e.Slot.Index].SetDisplayedItem(e.Slot.Item);
        }
        
        void OnStartHoverSlot(object sender, PointerEventData e)
        {
            
        }

        void OnEndHoverSlot(object sender, PointerEventData e)
        {
            
        }

        void OnClickHotbarSlot(object sender, PointerEventData e)
        {
            ItemSlotUI slotUI = (ItemSlotUI) sender;
            // _selectedSlot = inventory.Bag[slotUI.Index];

            if (e.button == PointerEventData.InputButton.Left)
            {

            } else if (e.button == PointerEventData.InputButton.Right)
            {

            }
        }

        #endregion
    }
}
