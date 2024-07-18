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
        public SlotType Type;
        [SerializeField] public bool IsEmpty
        {
            get
            {
                return item == Item.None;
            }
        }

        /// <summary>
        /// Called whenever the content of this Slot is changed.
        /// </summary>
        public event EventHandler<ContentChangedArgs> OnContentChanged;

        public ItemSlot(int index)
        {
            this.index = index;
            item = Item.None;
            Type = SlotType.All;
        }
        
        public ItemSlot(int index, SlotType slotType)
        {
            this.index = index;
            item = Item.None;
            Type = slotType;
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

        public bool TryPut(Item item)
        {
            if ((int) Type + 1 != (int) item.Type) return false;
            PutItem(item);
            return true;
        }

        public void PutItem(Item item)
        {
            this.item = item;
            ContentChanged();
        }

        public bool TryPutItem(Item item)
        {
            if (!IsEmpty)
            {
                return false;
            }
            this.item = item;
            ContentChanged();
            return true;
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
            if (amount > item.Count || item.Count <= 1 || amount < 1)
            {
                return TakeAll();
            }

            int remaining = item.Count - amount;
            Item toTake = new(item, amount);
            item = new(item, remaining);
            ContentChanged();
            return toTake;
        }

        public Item TakeAll()
        {
            Item toTake = new(item, item.Count);
            Clear();
            return toTake;
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
        
            // Stack overflow when stack size is 0, needs fix
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
    }
}