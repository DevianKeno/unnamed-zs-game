using System;
using UnityEngine;
using UZSG.Systems;

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
        public string Id => _itemData.Id;
        public string Name => _itemData.Name;
        public int StackSize => _itemData.StackSize;
        public ItemType Type => _itemData.Type;
        public ItemSubtype Subtype => _itemData.Subtype;
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
            _count = Math.Clamp(count, 1, Data.StackSize);
        }
        
        /// <summary>
        /// Create an item by id.
        /// </summary>
        public Item(string id, int count = 1)
        {
            _itemData = Game.Items.GetItemData(id);
            _count = Math.Clamp(count, 1, Data.StackSize);
        }
                
        /// <summary>
        /// Create a copy of the Item with a new count.
        /// </summary>
        public Item(Item other, int count = 1)
        {
            _itemData = other.Data;
            _count = Math.Clamp(count, 1, Data.StackSize);
        }

        #endregion
        
        
        public void SetCount(int value)
        {
            _count = value;
        }

        /// <summary>
        /// Take an amount from this item.
        /// </summary>
        public Item Take(int amount)
        {
            if (amount >= _count)
            {
                return this;
            }

            _count -= amount;
            return new(_itemData, amount);
        }
        
        public bool Combine(Item item, out Item excess)
        {
            excess = None;
            if (!CompareTo(item))
            {
                return false;
            }

            _count += item.Count;
            if (_count > Data.StackSize)
            {
                excess = new(this, _count - Data.StackSize);
            }
            return true;
        }

        /// <summary>
        /// Returns true if the items are the same.
        /// </summary>
        public bool CompareTo(Item other)
        {
            return _itemData == other.Data;
        }
    }
}