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

    [Serializable]
    public class ItemSlot
    {
        public struct ContentChangedArgs
        {
            public Item Item;
        }
        
        public static ItemSlot Empty { get => null; }

        [SerializeField] int _index;
        public int Index => _index;

        [SerializeField] Item _item;
        public Item Item => _item;

        [SerializeField] bool _isEmpty;
        public bool IsEmpty => _isEmpty;
        public SlotType Type;

        /// <summary>
        /// Called whenever the content of this Slot is changed.
        /// </summary>
        public event EventHandler<ContentChangedArgs> OnContentChanged;

        public ItemSlot(int index)
        {
            _index = index;
            _item = Item.None;
            _isEmpty = true;
            Type = SlotType.All;
        }
        
        public ItemSlot(int index, SlotType slotType)
        {
            _index = index;
            _item = Item.None;
            _isEmpty = true;
            Type = slotType;
        }

        void ContentChanged()
        {        
            OnContentChanged?.Invoke(this, new()
            {
                Item = _item
            });
        }

        public Item View()
        {
            return new(_item);
        }

        public bool TryPut(Item item)
        {
            if ((int) Type + 1 != (int) item.Type) return false;
            PutItem(item);
            return true;
        }

        public void PutItem(Item item)
        {
            _item = item;
            _isEmpty = false;
            ContentChanged();
        }
        public void Clear()
        {
            _item = Item.None;
            _isEmpty = true;
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
            if (amount > _item.Count || _item.Count <= 1 || amount < 1)
            {
                return TakeAll();
            }

            int remaining = _item.Count - amount;
            Item toTake = new(_item, amount);
            _item = new(_item, remaining);
            ContentChanged();
            return toTake;
        }

        public Item TakeAll()
        {
            Item toTake = new(_item, _item.Count);
            Clear();
            return toTake;
        }

        /// <summary>
        /// Tries to combine the Item in the Slot to given Item.
        /// Returns false if not the same item.
        /// </summary>
        public bool TryCombine(Item toAdd, out Item excess)
        {
            if (!_item.CompareTo(toAdd))
            {
                excess = toAdd;
                return false;
            }
        
            // Stack overflow when stack size is 0, needs fix
            int newCount = _item.Count + toAdd.Count;
            int excessCount = newCount - _item.StackSize;

            if (excessCount > 0)
            {
                _item = new(_item, _item.StackSize);
                excess = new(_item, excessCount);
            } else
            {
                _item = new(_item, newCount);
                excess = Item.None;
            }
            ContentChanged();
            return true;
        }
    }
}