using System;

using UnityEngine;

using UZSG.Data;
using UZSG.Items;
using UZSG.Saves;

namespace UZSG.Inventory
{
    /// <summary>
    /// Represents a container where Items can be put in.
    /// </summary>
    [Serializable]
    public class ItemSlot : ISaveDataReadWrite<ItemSlotSaveData>
    {
        public struct ItemChangedContext
        {
            public ItemSlot ItemSlot { get; set; }
            public Item OldItem { get; set; }
            public Item NewItem { get; set; }

            #region ??? idk
            public readonly bool AddedItem => NewItem != Item.None;
            public readonly bool RemovedItem => NewItem == Item.None;
            #endregion
        }
        
        [SerializeField] int index;
        public int Index
        {
            get { return index; }
            set { index = value; }
        }
        [SerializeField] Item item = Item.None;
        public Item Item => item;
        public ItemSlotType SlotType;
        [SerializeField] public bool IsEmpty
        {
            get
            {
                return item.IsNone;
            }
        }
        [SerializeField] public bool HasItem
        {
            get
            {
                return !item.IsNone;
            }
        }

        Item _previousItem = Item.None;

        /// <summary>
        /// Internal call before Container caching.
        /// </summary>
        internal event EventHandler<ItemChangedContext> OnItemChangedInternal;

        /// <summary>
        /// Called whenever the content of this Slot is changed.
        /// </summary>
        public event EventHandler<ItemChangedContext> OnItemChanged;

        public ItemSlot(int index, ItemSlotType slotType = ItemSlotType.All)
        {
            this.index = index;
            item = Item.None;
            SlotType = slotType;
        }
        

        #region Save read/write

        public void ReadSaveData(ItemSlotSaveData saveData)
        {
            
        }

        public ItemSlotSaveData WriteSaveData()
        {
            var saveData = new ItemSlotSaveData()
            {
                Index = index,
                Item = Item.WriteSaveData(),
            };

            return saveData;
        }

        #endregion


        void ItemChangedInternal()
        {
            var context = new ItemChangedContext()
            {
                ItemSlot = this,
                OldItem = _previousItem,
                NewItem = item,
            };
            OnItemChangedInternal?.Invoke(this, context);
            OnItemChanged?.Invoke(this, context);
        }


        #region Public methods

        public Item View()
        {
            return new(item);
        }

        public void Put(Item item)
        {
            _previousItem = this.item;
            this.item = item;
            ItemChangedInternal();
        }

        public bool TryPut(Item item)
        {
            /// Have trouble checking if the item's type fits the slot's type
            // if (!IsEmpty || !IsFits(item)) return false;
            if (!IsEmpty) return false;

            _previousItem = this.item;
            this.item = item;
            ItemChangedInternal();
            return true;
        }

        public bool IsFits(Item item)
        {
            return (SlotType & MapItemTypeToSlotType(item.Data.Type)) != 0;
        }
        
        /// <summary>
        /// Clear the item contents of this slot.
        /// </summary>
        public void Clear()
        {
            _previousItem = this.item;
            this.item = Item.None;
            ItemChangedInternal();
        }

        /// <summary>
        /// Take items from the Slot.
        /// -1 amount takes the entire stack.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public Item TakeItems(int amount = -1)
        {
            if (IsTakingAll(amount))
            {
                return TakeAll();
            }

            _previousItem = this.item;
            int remaining = item.Count - amount;
            Item toTake = new(item, amount);        /// Return a copy of the item with amount taken
            item = new(item, remaining);            /// Re-assign left items
            ItemChangedInternal();
            return toTake;
        }

        public Item TakeAll()
        {
            Item toTake = new(item, item.Count);
            Clear();
            return toTake;
        }

        bool IsTakingAll(int value)
        {
            return value > item.Count   /// Tried to take greater than current amount
                || item.Count == 1      /// One item left
                || value < 0;           /// Value is -1 (take entire stack)
        }

        /// <summary>
        /// <i>Tries to stack</i> the given Item with the Item in this slot.
        /// If possible, it stacks the Items as much the stack size* allows, on which the excess is outed.
        /// Returns false and does not stack if not possible.
        /// </summary>
        public bool TryStack(Item other, out Item excess, bool useMaxStackSize = false)
        {
            var previousItem = new Item(this.item);
            if (item.TryStack(other, out excess, useMaxStackSize))
            {
                _previousItem = previousItem;
                ItemChangedInternal(); 
                return true;
            }
            excess = Item.None;
            return false;
        }

        /// <summary>
        /// Swap contents :)
        /// </summary>
        public static void SwapContents(ItemSlot a, ItemSlot b)
        {
            var temp = a.TakeItems();
            a.Put(b.TakeItems());
            b.Put(temp);
        }

        #endregion

        
        ItemSlotType MapItemTypeToSlotType(ItemType itemType)
        {
            return itemType switch
            {
                ItemType.Item => ItemSlotType.Item,
                ItemType.Weapon => ItemSlotType.Weapon,
                ItemType.Tool => ItemSlotType.Tool,
                ItemType.Equipment => ItemSlotType.Equipment,
                ItemType.Accessory => ItemSlotType.Accessory,
                _ => ItemSlotType.All
            };
        }
    }
}