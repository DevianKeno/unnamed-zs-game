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
        public const int MaxStackSize = 99999;
        public static Item None => new(data: null);
        
        [SerializeField] ItemData _itemData;
        public ItemData Data => _itemData;
        [SerializeField] int _count;
        public int Count
        {
            get => _count;
            set => _count = value;
        }
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
        /// Create a copy of the Item.
        /// </summary>
        public Item(Item other)
        {
            _itemData = other.Data;
            int stack = other.IsNone ? 0 : _itemData.StackSize;
            _count = Math.Clamp(other.Count, 0, EnsureStackSizeNotZero(stack));
        }
                        
        /// <summary>
        /// Create a copy of the Item with a new count.
        /// </summary>
        public Item(Item other, int count = 1)
        {
            _itemData = other.Data;
            int stack = other.IsNone ? 0 : _itemData.StackSize;
            _count = Math.Clamp(count, 0, EnsureStackSizeNotZero(stack));
        }

        #endregion


        /// <summary>
        /// Clamp this Item's count to either its StackSize or MaxStack.
        /// </summary>
        public void ClampStack(bool max = false)
        {
            int maxStack = max ? MaxStackSize : _itemData.StackSize;
            _count = Math.Clamp(_count, 1, maxStack);
        }

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
        /// Exceeding stack size.
        /// </summary>
        public void Combine(Item other)
        {
            if (IsNone)
            {
                _itemData = other._itemData;
            }
            _count += other.Count;
        }

        /// <summary>
        /// Try to combine this Item with the other item.
        /// </summary>
        public bool TryCombine(Item other, out Item excess, bool max = false)
        {
            excess = None;
            if (!CanBeCombinedWith(other)) return false;

            int maxStack = max ? MaxStackSize : Data.StackSize;
            _count += other.Count;
            if (_count > maxStack)
            {
                excess = new(this, _count - maxStack);
            }
            return true;
        }

        /// <summary>
        /// Try to stack this Item until max stack size.
        /// </summary>
        public bool TryCombineMax(Item other, out Item excess)
        {
            excess = None;
            if (!CanBeCombinedWith(other)) return false;

            _count += other.Count;
            if (_count > MaxStackSize)
            {
                excess = new(this, _count - MaxStackSize);
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
        /// Returns false if the total of the combined Items exceeds the Item's stack size, unless maxed.
        /// </summary>
        public readonly bool CanBeStackedWith(Item other, bool max = false)
        {
            if (!CanBeCombinedWith(other)) return false;
            
            int maxStack = max ? MaxStackSize : _itemData.StackSize;
            return _count + other.Count <= maxStack;
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
            return stackSize <= 0 ? 1 : stackSize;
        }

        public static Item operator *(Item item, int value)
        {
            return new Item(item, item.Count * value);
        }
    }
}