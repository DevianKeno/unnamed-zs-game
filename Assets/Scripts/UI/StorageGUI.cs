using UnityEngine;
using UnityEngine.EventSystems;

using UZSG.Systems;
using UZSG.Objects;
using UZSG.Items;
using UZSG.Inventory;

using static UnityEngine.EventSystems.PointerEventData.InputButton;
using static UZSG.UI.ItemSlotUI.ClickType;
using System;

namespace UZSG.UI.Objects
{
    public class StorageGUI : ObjectGUI
    {
        protected Storage storage;
        /// <summary>
        /// The Storage tied to this Storage GUI.
        /// </summary>The 
        public Storage Storage
        {
            get
            {
                return storage;
            }
            set
            {
                storage = value;
                baseObject = value;
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
                
        public void LinkStorage(Storage storage)
        {
            Storage = storage;

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
                slotUI.OnMouseDown += OnSlotClick;
                slotUI.OnHoverStart += OnSlotHoverStart;

                slotUI.Show();
            }
        }

        void OnSlotClick(object sender, ItemSlotUI.ClickedContext ctx)
        {
            var slot = ((ItemSlotUI) sender).Slot;

            if (ctx.Button == Left)
            {
                if (ctx.ClickType == ShiftClick)
                {
                    FastDepositToBag(slot);
                    return;
                }

                if (player.InventoryGUI.IsHoldingItem)
                {
                    var heldItem = player.InventoryGUI.HeldItem;

                    if (slot.IsEmpty || slot.Item.CompareTo(heldItem))
                    {
                        slot.TryCombine(player.InventoryGUI.TakeHeldItem(), out var excess);
                        if (!excess.IsNone)
                        {
                            player.InventoryGUI.HoldItem(excess);
                        }
                    }
                    else /// item diff, swap
                    {
                        var tookItem = slot.TakeAll();
                        var prevHeld = player.InventoryGUI.SwapHeldWith(tookItem);
                        slot.Put(prevHeld);
                    }
                }
                else
                {
                    if (slot.IsEmpty) return;

                    player.InventoryGUI.HoldItem(slot.TakeAll());
                    _lastSelectedSlot = slot;
                }
            }
            else if (ctx.Button == Right)
            {
                if (player.InventoryGUI.IsHoldingItem) /// put 1 to target slot
                {
                    var heldItem = player.InventoryGUI.HeldItem;

                    if (slot.IsEmpty)
                    {
                        _isPutting = true;
                        _isGetting = false;

                        slot.Put(heldItem.Take(1));
                    }
                    else
                    {
                        var toPut = new Item(heldItem, 1);
                        if (slot.Item.CanStackWith(toPut))
                        {
                            slot.TryCombine(toPut, out _);
                            heldItem.Take(1);
                        }
                    }
                }
                else /// take 1
                {
                    _isPutting = false;
                    _isGetting = true;
                }
            }
        }

        void OnSlotHoverStart(object sender, ItemSlotUI.ClickedContext e)
        {
            var slot = (ItemSlotUI) sender;

            if (player.InventoryGUI.IsVisible)
            {
                // player.InventoryGUI.Selector.Select(slot.Rect);
            }
        }

        void FastDepositToBag(ItemSlot slot)
        {
            // player.Inventory.Bag.OnExcessItem += PutBackExcess;

            var item = slot.TakeAll();
            player.Inventory.Bag.TryPutNearest(item);

            // void PutBackExcess(Item excess)
            // {
            //     player.Inventory.Bag.OnExcessItem -= PutBackExcess;

            //     slot.Put(excess);
            // }
        }

        void PutBackHeldItem()
        {
            if (!player.InventoryGUI.IsHoldingItem) return;

            player.Inventory.DropItem(player.InventoryGUI.TakeHeldItem());
        }
    }
}