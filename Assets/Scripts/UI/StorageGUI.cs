using UZSG.Systems;
using UZSG.Entities;
using UZSG.Objects;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UZSG.UI
{
    public class StorageGUI : Window, IObjectGUI
    {
        Player player;
        Storage storage;
        
        [SerializeField] Transform slotsHolder;

        public void SetPlayer(Player player)
        {
            this.player = player;
            // InitializePlayerEvents()
            // player.Inventory.Bag.OnSlotItemChanged += OnPlayerBagItemChanged;
        }
        
        public void LinkStorage(Storage storage)
        {
            this.storage = storage;

            CreateSlotUIs(storage.StorageData.Size);
        }
        
        void CreateSlotUIs(int size)
        {
            for (int i = 0; i < size; i++)
            {
                var slotUI = Game.UI.Create<ItemSlotUI>("Item Slot");
                slotUI.name = $"Output Slot ({i})";
                slotUI.transform.SetParent(slotsHolder);
                slotUI.Index = i;

                slotUI.Link(storage.Container[i]);
                slotUI.OnMouseDown += OnOutputSlotClick;

                slotUI.Show();
            }
        }
        
        void OnOutputSlotClick(object sender, ItemSlotUI.ClickedContext ctx)
        {
            var slot = ((ItemSlotUI) sender).Slot;

            if (ctx.Pointer.button == PointerEventData.InputButton.Left)
            {
                if (slot.IsEmpty) return;
                if (player.InventoryGUI.IsHoldingItem) return;

                if (ctx.ClickType == ItemSlotUI.ClickType.FastDeposit)
                {
                    player.Inventory.Bag.TryPutNearest(slot.TakeAll());
                }
                else
                {
                    player.InventoryGUI.HoldItem(slot.TakeAll());
                }
            }
        }
    }
}