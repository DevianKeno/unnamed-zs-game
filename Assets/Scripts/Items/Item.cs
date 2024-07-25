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
        public static Item None => null;
        
        [SerializeField] protected ItemData _itemData;
        public ItemData Data => _itemData;
        public string Id { get => _itemData.Id; }
        public string Name { get => _itemData.Name; }
        public int StackSize { get => _itemData.StackSize; }
        public ItemType Type { get => _itemData.Type; }
        [SerializeField] int _count;
        public int Count { get => _count; }
        public bool IsNone => _itemData == null;


        #region Item constructors

        /// <summary>
        /// Create an Item object from ItemData.
        /// </summary>
        public Item(ItemData data)
        {
            _itemData = data;
            _count = 1;
        }

        /// <summary>
        /// Create an Item object from ItemData with count.
        /// </summary>
        public Item(ItemData data, int count)
        {
            _itemData = data;
            _count = Math.Clamp(count, 1, Data.StackSize);
        }
        
        /// <summary>
        /// Create an item by id.
        /// </summary>
        public Item(string id)
        {
            _itemData = Game.Items.GetItemData(id);
            _count = 1;
        }
        
        /// <summary>
        /// Create an item by id with count.
        /// </summary>
        public Item(string id, int count)
        {
            _itemData = Game.Items.GetItemData(id);
            _count = Math.Clamp(count, 1, Data.StackSize);
        }

        /// <summary>
        /// Create a copy of the Item.
        /// </summary>
        public Item(Item other)
        {
            _itemData = other.Data;
            _count = other.Count;
        }
        
        /// <summary>
        /// Create a copy of the Item with a new count.
        /// </summary>
        public Item(Item other, int count)
        {
            _itemData = other.Data;
            _count = Math.Clamp(count, 1, Data.StackSize);
        }

        #endregion
        
        
        public void SetCount(int value)
        {
            _count = value;
        }

        public Item Take(int amount)
        {
            if (amount > _count) return Take();
            return new(_itemData, amount);
        }

        public Item Take()
        {
            Item toTake = new(_itemData, _count);
            return toTake;
        }
        
        public void Combine(Item item)
        {
            if (!CompareTo(item)) return;
            _count += item.Count;
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