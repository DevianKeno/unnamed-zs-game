using UnityEngine;
using UnityEngine.EventSystems;

using UZSG.Systems;
using UZSG.Objects;

namespace UZSG.UI.Objects
{
    public class StorageGUI : ObjectGUI
    {
        protected Storage storage;
        public Storage Storage => storage;
        
        [SerializeField] Transform slotsHolder;
        
        
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
                slotUI.OnMouseDown += OnStorageSlotClick;

                slotUI.Show();
            }
        }
        
        void OnStorageSlotClick(object sender, ItemSlotUI.ClickedContext ctx)
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