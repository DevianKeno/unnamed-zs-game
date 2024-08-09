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
    public class PlayerHUD : Window
    {
        public Player Player;
        [Space]

        Dictionary<int, ItemSlotUI> _equipmentSlotUIs = new();
        Dictionary<int, ItemSlotUI> _hotbarSlotUIs = new();

        public event Action<Player> OnInitializePlayer;
        
        [Header("Elements")]
        public GameObject equipment;
        public GameObject hotbar;
        public AttributeBar HealthBar;
        public AttributeBar StaminaBar;
        public AttributeBar HungerBar;
        public AttributeBar HydrationBar;
        public AttributeBar XPBar;
        public AmmoCounterHUD AmmoCounter;
        public TextMeshProUGUI equippedWeaponTMP;
        public GameObject pickupsIndicatorContainer;
        public DynamicCrosshair crosshair;
        public SwitchCrosshair allCrosshair;
        public Compass compass;
        public Image vignette;

        internal void Initialize(Player player)
        {
            if (player == null)
            {
                Game.Console.LogAndUnityLog($"Invalid player.");
                return;
            }

            Player = player;
            BindPlayerAttributes();
            InitializeItemSlots();
            InitializeEvents();
            crosshair.Initialize(player);
            allCrosshair.Initialize(player);
            compass.Initialize(player);
        }

        void InitializeItemSlots()
        {
            InitializeHotbarSlots();
            InitializeEquipmentSlots();
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

        void InitializeEquipmentSlots()
        {
            /// Equipment slots are already set
            int index = 1; /// Only 1 (mainhand) and 2 (offhand), as 0 (arms) is not displayed :)
            foreach (Transform child in equipment.transform)
            {
                if (child.TryGetComponent<ItemSlotUI>(out var slotUI)) /// there might be other GameObjects
                {
                    _equipmentSlotUIs[index] = slotUI;
                    index++;
                }
            }
        }

        void InitializeEvents()
        {
            Player.Inventory.Hotbar.OnSlotContentChanged += OnHotbarSlotChanged;
            Player.Inventory.Equipment.OnSlotContentChanged += OnEquipmentSlotChanged;
            Player.FPP.OnChangeHeldItem += OnChangeHeldItem;

            Player.Actions.OnPickupItem += (item) =>
            {
                var indicator = Game.UI.Create<PickupsIndicator>("Pickups Indicator", show: false);
                indicator.transform.SetParent(pickupsIndicatorContainer.transform); 
                indicator.SetDisplayedItem(item);
                indicator.PlayAnimation();
            };
        }

        public void BindPlayer(Player player)
        {
        }

        void BindPlayerAttributes()
        {
            HealthBar.BindAttribute(Player.Vitals.Get("health"));
            StaminaBar.BindAttribute(Player.Vitals.Get("stamina"));
            HungerBar.BindAttribute(Player.Vitals.Get("hunger"));
            HydrationBar.BindAttribute(Player.Vitals.Get("hydration"));

            XPBar.BindAttribute(Player.Generic.Get("experience"));
        }


        #region Callbacks

        void OnHotbarSlotChanged(object sender, SlotContentChangedArgs e)
        {
            var hotbarOffset = e.Slot.Index + 3;
            _hotbarSlotUIs[hotbarOffset].SetDisplayedItem(e.Slot.Item);
        }

        void OnEquipmentSlotChanged(object sender, SlotContentChangedArgs e)
        {
            _equipmentSlotUIs[e.Slot.Index].SetDisplayedItem(e.Slot.Item);
        }

        void OnChangeHeldItem(HeldItemController heldItem)
        {
            if (heldItem == null) return;
            equippedWeaponTMP.text = heldItem.ItemData.Name;
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
