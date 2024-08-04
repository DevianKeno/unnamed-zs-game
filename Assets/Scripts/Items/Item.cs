using System;

using UnityEngine;

using UZSG.Systems;
using UZSG.Data;

namespace UZSG.Items
{
    public interface IComparable<T>
    {
        public bool CompareTo(T other);
    }
    
    /// <summary>
    /// Represents an Item with count.
    /// </summary>
    [Serializable]
    public class Item : IComparable<Item>
    {
        public static Item None => new(data: null);
        
        [SerializeField] protected ItemData _itemData;
        public ItemData Data => _itemData;
        [SerializeField] int _count;
        public int Count => _count;
        public bool IsNone => _itemData == null;


        #region Item constructors

        /// <summary>
        /// Create 'None' item.
        /// </summary>
        public Item(ItemData data = null)
        {
            _itemData = data;
            _count = 0;
        }

        /// <summary>
        /// Create an Item object from ItemData with count.
        /// </summary>
        public Item(ItemData data, int count = 1)
        {
            _itemData = data;
            _count = Math.Clamp(count, 0, EnsureStackSizeNotZero());
        }
        
        /// <summary>
        /// Create an item by id.
        /// </summary>
        public Item(string id, int count = 1)
        {
            _itemData = Game.Items.GetData(id);
            _count = Math.Clamp(count, 0, EnsureStackSizeNotZero());
        }
                
        /// <summary>
        /// Create a copy of the Item with a new count.
        /// </summary>
        public Item(Item other, int count = 1)
        {
            _itemData = other.Data;
            _count = Math.Clamp(count, 0, EnsureStackSizeNotZero());
        }

        #endregion
        
        int EnsureStackSizeNotZero()
        {
            return Data.StackSize == 0 ? 1 : Data.StackSize;
        }
        
        public void SetCount(int value)
        {
            _count = value;
        }

        /// <summary>
        /// Take an amount from this item.
        /// </summary>
        public Item Take(int amount)
        {
            _count -= amount;
            return new(_itemData, amount);
        }

        public Item Combine(Item other)
        {
            if (!CanBeCombinedWith(other)) return None;

            _count += other.Count;
            return this;
        }
        
        public bool TryCombine(Item other, out Item excess)
        {
            excess = None;
            if (!CanBeCombinedWith(other)) return false;

            _count += other.Count;
            if (_count > Data.StackSize)
            {
                excess = new(this, _count - Data.StackSize);
            }
            return true;
        }

        public bool CanBeCombinedWith(Item other)
        {
            if (other == null || other.IsNone) return false; /// Invalid item
            if (!CompareTo(other)) return false; /// Not the same
            if (!_itemData.IsStackable) return false; /// Not stackable
            if (_count + other.Count > Data.StackSize) return false; /// Exceeds stack size

            return true;
        }

        /// <summary>
        /// Returns true if the items are the same.
        /// </summary>
        public bool CompareTo(Item other)
        {
            if (other != null) return _itemData == other.Data;
            return false;
        }
    }
}