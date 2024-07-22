using System;
using UnityEngine;
using UZSG.Items;

namespace UZSG.Inventory
{
    [Flags]
    public enum SlotType
    {
        All = 1,
        Item = 2,
        Tool = 4,
        Weapon = 8,
        Equipment = 16,
        Accessory = 32
    }

    /// <summary>
    /// Represents a container where Items can be put in.
    /// </summary>
    [Serializable]
    public class ItemSlot
    {
        public struct ContentChangedArgs
        {
            public Item Item;
        }
        
        public static ItemSlot Empty { get => null; }

        [SerializeField] int index;
        public int Index => index;
        [SerializeField] Item item;
        public Item Item => item;
        public SlotType SlotType;
        [SerializeField] public bool IsEmpty
        {
            get { return item == Item.None; }
        }

        /// <summary>
        /// Called whenever the content of this Slot is changed.
        /// </summary>
        public event EventHandler<ContentChangedArgs> OnContentChanged;

        public ItemSlot(int index)
        {
            this.index = index;
            item = Item.None;
            SlotType = SlotType.All;
        }
        
        public ItemSlot(int index, SlotType slotType)
        {
            this.index = index;
            item = Item.None;
            SlotType = slotType;
        }

        void ContentChanged()
        {        
            OnContentChanged?.Invoke(this, new()
            {
                Item = item
            });
        }

        public Item View()
        {
            return new(item);
        }

        public void Put(Item item)
        {
            this.item = item;
            ContentChanged();
        }

        public bool TryPut(Item item)
        {
            /// Have trouble checking if the item's type fits the slot's type
            // if (!IsEmpty || !IsFits(item)) return false;
            if (!IsEmpty) return false;
            this.item = item;
            ContentChanged();
            return true;
        }

        public bool IsFits(Item item)
        {
            return (SlotType & MapItemTypeToSlotType(item.Type)) != 0;
        }
        
        public void Clear()
        {
            item = Item.None;
            ContentChanged();
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

            int remaining = item.Count - amount;
            Item toTake = new(item, amount);        /// Return a copy of the item with amount taken
            item = new(item, remaining);            /// Re-assign left items
            ContentChanged();
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
                || value < 1;           /// Value is 0 or -1 (take entire stack)
        }

        /// <summary>
        /// Tries to combine the Item in the Slot to given Item.
        /// Returns false if not the same item.
        /// </summary>
        public bool TryCombine(Item toAdd, out Item excess)
        {
            if (!item.CompareTo(toAdd))
            {
                excess = toAdd;
                return false;
            }
        
            /// Stack overflow when stack size is 0, needs fix
            int newCount = item.Count + toAdd.Count;
            int excessCount = newCount - item.StackSize;

            if (excessCount > 0)
            {
                item = new(item, item.StackSize);
                excess = new(item, excessCount);
            } else
            {
                item = new(item, newCount);
                excess = Item.None;
            }
            ContentChanged();
            return true;
        }
        
        SlotType MapItemTypeToSlotType(ItemType itemType)
        {
            return itemType switch
            {
                ItemType.Item => SlotType.Item,
                ItemType.Weapon => SlotType.Weapon,
                ItemType.Tool => SlotType.Tool,
                ItemType.Equipment => SlotType.Equipment,
                ItemType.Accessory => SlotType.Accessory,
                _ => SlotType.All
            };
        }
    }
}