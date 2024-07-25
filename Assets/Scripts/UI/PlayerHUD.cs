using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

using UZSG.Entities;
using UZSG.Inventory;
using UZSG.Systems;
using UZSG.Items;

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

        public GameObject weapons;
        public TextMeshProUGUI equippedWeaponTMP;
        public GameObject hotbar;
        public GameObject pickupsIndicatorContainer;

        internal void Initialize()
        {
            if (Player == null)
            {
                var msg = $"Failed to initialize Player HUD. Bind a Player first!";
                Game.Console.Log(msg);
                Debug.LogWarning(msg);
                return;
            }

            InitializeHotbarSlots();
            InitializeEvents();
        }

        void InitializeHotbarSlots()
        {
            InitializeWeaponSlots();
            InitializeToolSlots();
        }

        void InitializeWeaponSlots()
        {
            int i = 1;
            foreach (Transform child in weapons.transform)
            {
                if (child.TryGetComponent<ItemSlotUI>(out var slotUI))
                {
                    _hotbarSlotUIs.Add(i, slotUI);
                    i++;
                }
            }
        }

        void InitializeToolSlots()
        {
            int maxSlots = inventory.Hotbar.SlotsCount;
            for (int i = 0; i < maxSlots; i++)
            {
                int index = 3 + i; /// 3 is hotbar starting index
                var slotUI = Game.UI.Create<ItemSlotUI>("item_slot");
                slotUI.name = $"Hotbar Slot ({i})";
                slotUI.transform.SetParent(hotbar.transform);
                slotUI.Index = index;
                
                slotUI.OnMouseDown += OnClickHotbarSlot;
                slotUI.OnStartHover += OnStartHoverSlot;
                slotUI.OnEndHover += OnEndHoverSlot;
                _hotbarSlotUIs.Add(index, slotUI);
            }
        }

        void InitializeEvents()
        {
            Player.Inventory.Hotbar.OnSlotContentChanged += OnHotbarSlotChanged;
            Player.Inventory.Hotbar.OnChangeEquipped += OnHotbarChangeEquipped;
            Player.Actions.OnPickupItem += (item) =>
            {
                var indicator = Game.UI.Create<PickupsIndicator>("pickups_indicator");
                indicator.transform.SetParent(pickupsIndicatorContainer.transform); 
                indicator.SetDisplayedItem(item);
                indicator.PlayAnimation();
            };
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

        void OnHotbarSlotChanged(object sender, SlotContentChangedArgs e)
        {
            _hotbarSlotUIs[e.Slot.Index].SetDisplayedItem(e.Slot.Item);
        }

        void OnHotbarChangeEquipped(object sender, Hotbar.ChangeEquippedArgs e)
        {
            if (e.ItemSlot == null) return;
            if (e.ItemSlot.Item == Item.None) return;

            equippedWeaponTMP.text = e.ItemSlot.Item.Name;
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

        void UpdateAmmoCounter()
        {
            
        }

        #endregion
    }
}
