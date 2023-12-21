using System;
using UnityEngine;
using URMG.Items;

namespace URMG.Inventory
{
    public enum SlotType { All, Item, Weapon, Tool, Equipment, Accessory }

    public class ItemSlot
    {
        public struct ContentChangedArgs
        {
            public Item Content;
        }

        [SerializeField] int _index;
        public int Index { get => _index; }

        [SerializeField] Item _item;
        public Item Item { get => _item; }

        [SerializeField] bool _isEmpty;
        public bool IsEmpty
        {
            get 
            {
                if (_item == Item.None) return true;
                return false;
            }
        }
        public SlotType Type;

        /// <summary>
        /// Called whenever the content of this slot is changed.
        /// </summary>
        public event EventHandler<ContentChangedArgs> OnContentChanged;

        public ItemSlot(int index)
        {
            _index = index;
            _item = Item.None;
            Type = SlotType.All;
        }
        
        public ItemSlot(int index, SlotType slotType)
        {
            _index = index;
            _item = Item.None;
            Type = slotType;
        }

        void ContentChanged()
        {        
            OnContentChanged?.Invoke(this, new()
            {
                Content = _item
            });
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
            ContentChanged();
        }
        public void Clear()
        {
            _item = Item.None;
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
            _item = Item.None;
            ContentChanged();
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