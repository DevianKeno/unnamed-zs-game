using System;
using System.Threading;
using UnityEngine;
using URMG.Items;

namespace URMG.InventoryS
{
    public class Slot
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

        /// <summary>
        /// Called whenever the content of this slot is changed.
        /// </summary>
        public event EventHandler<ContentChangedArgs> OnContentChanged;

        public Slot(int index)
        {
            _index = index;
            _item = Item.None;
        }

        void ContentChanged()
        {        
            OnContentChanged?.Invoke(this, new()
            {
                Content = _item
            });
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
            if (!_item.CompareTo(toAdd) || !toAdd.Data.IsStackable)
            {
                excess = toAdd;
                return false;
            }
        
            int newCount = _item.Count + toAdd.Count;
            int excessCount = newCount - _item.Data.MaxStackSize;

            if (excessCount > 0)
            {
                _item = new(_item.Data, _item.Data.MaxStackSize);
                excess = new(_item.Data, excessCount);
            } else
            {
                _item = new(_item.Data, newCount);
                excess = Item.None;
            }
            ContentChanged();
            return true;
        }
    }
}