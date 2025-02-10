using UnityEngine;

using UZSG.Objects;
using UZSG.Inventory;

namespace UZSG.UI
{
    public class StorageGUI : ObjectGUI
    {
        /// <summary>
        /// The Storage tied to this Storage GUI.
        /// </summary>The 
        public StorageObject Storage
        {
            get => (StorageObject) BaseObject;
            set
            {
                BaseObject = value;
            }
        }

        ItemSlot _lastSelectedSlot;
        /// <summary>
        /// If the last action is "putting" Items to a slot.
        /// </summary>
        bool _isPutting;
        /// <summary>
        /// If the last action is "getting" Items to a slot.
        /// </summary>
        bool _isGetting;
        
        [SerializeField] Transform slotsHolder;

        protected override void OnHide()
        {
            PutBackHeldItem();
        }
                
        public void ReadStorage(StorageObject storage)
        {
            this.Storage = storage;
            CreateSlotUIs(storage.StorageData.Size);
        }
        
        void CreateSlotUIs(int size)
        {
            for (int i = 0; i < size; i++)
            {
                var slotUI = Game.UI.Create<ItemSlotUI>("Item Slot");
                slotUI.name = $"Storage Slot ({i})";
                slotUI.transform.SetParent(slotsHolder);
                slotUI.Link(Storage.Container[i]);

                slotUI.OnMouseDown += OnSlotClick;
                slotUI.OnHoverStart += OnSlotHoverStart;
                slotUI.Show();
            }
        }

        void OnSlotClick(object sender, ItemSlotUI.ClickedContext ctx)
        {
            Storage.Player.InventoryWindow.OnItemSlotClicked(sender, ctx);
            return;
        }

        void OnSlotHoverStart(object sender, ItemSlotUI.ClickedContext e)
        {
            var slot = (ItemSlotUI) sender;

            if (Player.InventoryWindow.IsVisible)
            {
                // player.InventoryGUI.Selector.Select(slot.Rect);
            }
        }

        void FastDepositToBag(ItemSlot slot)
        {
            // player.Inventory.Bag.OnExcessItem += PutBackExcess;

            var item = slot.TakeAll();
            Player.Inventory.Bag.TryPutNearest(item);

            // void PutBackExcess(Item excess)
            // {
            //     player.Inventory.Bag.OnExcessItem -= PutBackExcess;

            //     slot.Put(excess);
            // }
        }

        void PutBackHeldItem()
        {
            if (!Player.InventoryWindow.IsHoldingItem) return;

            Player.Inventory.DropItem(Player.InventoryWindow.TakeHeldItem());
        }
    }
}