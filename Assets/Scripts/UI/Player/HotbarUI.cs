using System;
using UnityEngine;

using UZSG.Data;
using UZSG.Entities;
using UZSG.Inventory;
using UZSG.Systems;
using UZSG.UI.Players;

using static UZSG.Data.ItemType;

namespace UZSG.UI
{
    public class HotbarUI : MonoBehaviour
    {
        public Player Player;
        public Container Hotbar => Player.Inventory.Hotbar;

        ItemSlot _selectedSlot;
        ItemSlotUI _selectedSlotUI;
        ChoiceWindow itemOptions;

        [SerializeField] Transform slotsContainer;
        
        internal void Initialize(Player player)
        {
            Player = player;
            InitializeHotbarSlotUIs();
        }
        
        void InitializeHotbarSlotUIs()
        {
            int maxSlots = Hotbar.SlotCount;
            for (int i = 0; i < maxSlots; i++)
            {
                var slot = Game.UI.Create<ItemSlotUI>("Item Slot");
                slot.name = $"Hotbar Slot ({i})";
                slot.transform.SetParent(slotsContainer);
                slot.Index = i;
                slot.Link(Hotbar[i]);
                slot.OnMouseDown += OnHotbarSlotClick;
                slot.Show();
            }
        }

        void OnHotbarSlotClick(object sender, ItemSlotUI.ClickedContext e)
        {
            var slotUI = (ItemSlotUI) sender;
            var slot = slotUI.Slot;

            if (Player.InventoryGUI.IsHoldingItem)
            {
                if (Player.InventoryGUI.HeldItem.Data.Type != Tool) return;

                if (slot.IsEmpty)
                {
                    slot.Put(Player.InventoryGUI.TakeHeldItem());
                }
                else
                {
                    var tookItem = slot.TakeAll();
                    var prevHeld = Player.InventoryGUI.SwapHeldWith(tookItem);
                    slot.Put(prevHeld);
                }
            }
            else
            {
                if (!slot.IsEmpty)
                {
                    var tookItem = slot.TakeAll();
                    Player.InventoryGUI.HoldItem(tookItem);
                }
            }
        }
    }
}