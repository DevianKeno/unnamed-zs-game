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
    public struct Item : IComparable<Item>
    {
        public static Item None => new(data: null);
        
        [SerializeField] ItemData _itemData;
        public ItemData Data => _itemData;
        [SerializeField] int _count;
        public readonly int Count => _count;
        public readonly bool IsNone => _itemData == null;
        public readonly string Id => _itemData.Id;


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
            _count = Math.Clamp(count, 0, EnsureStackSizeNotZero(_itemData.StackSize));
        }
        
        /// <summary>
        /// Create an item by id.
        /// </summary>
        public Item(string id, int count = 1)
        {
            _itemData = Game.Items.GetData(id);
            _count = Math.Clamp(count, 0, EnsureStackSizeNotZero(_itemData.StackSize));
        }
                
        /// <summary>
        /// Create a copy of the Item with a new count.
        /// </summary>
        public Item(Item other, int count = 1)
        {
            _itemData = other.Data;
            _count = Math.Clamp(count, 0, EnsureStackSizeNotZero(_itemData.StackSize));
        }

        #endregion
        
        
        /// <summary>
        /// Take an amount from this item.
        /// </summary>
        public Item Take(int amount)
        {
            _count -= amount;
            return new(_itemData, amount);
        }
        
        /// <summary>
        /// Try to combine this Item with the other item.
        /// </summary>
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
        
        /// <summary>
        /// Try to stack this Item with the other item.
        /// </summary>
        public bool TryCombineStack(Item other)
        {
            if (!CanBeStackedWith(other)) return false;

            _count += other.Count;
            return true;
        }

        /// <summary>
        /// Checks if this Item can be combined with the other Item.
        /// </summary>
        public readonly bool CanBeCombinedWith(Item other)
        {
            if (!_itemData.IsStackable) return false;   /// Not stackable
            if (!CompareTo(other)) return false;        /// Not the same

            return true;
        }

        /// <summary>
        /// Checks if this Item can be stacked with the other Item.
        /// Returns false if the total of the combined Items exceeds the Item's stack size.
        /// </summary>
        public readonly bool CanBeStackedWith(Item other)
        {
            if (!CanBeCombinedWith(other)) return false;
            
            return _count + other.Count <= _itemData.StackSize; /// Exceeds stack size
        }

        /// <summary>
        /// Returns true if the items are the same.
        /// </summary>
        public readonly bool CompareTo(Item other)
        {
            return _itemData == other.Data;
        }
        
        static int EnsureStackSizeNotZero(int stackSize)
        {
            return stackSize == 0 ? 1 : stackSize;
        }
    }
}