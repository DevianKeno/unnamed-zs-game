using System;
using UnityEngine;

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
        
        [SerializeField] ItemData _itemData;
        public ItemData Data => _itemData;
        public string Id { get => _itemData.Id; }
        public string Name { get => _itemData.Name; }
        public int StackSize { get => _itemData.StackSize; }
        public ItemType Type { get => _itemData.Type; }
        [SerializeField] int _count;
        public int Count { get => _count; }

        /// <summary>
        /// Create an Item object from ItemData.
        /// </summary>
        public Item(ItemData itemData, int count)
        {
            _itemData = itemData;
            
            if (count < 0)
            {
                count = StackSize;
            }
            _count = count;
        }

        /// <summary>
        /// Create a copy of the Item.
        /// </summary>
        public Item(Item item)
        {
            _itemData = item.Data;
            _count = item.Count;
        }
        
        /// <summary>
        /// Create a copy of the Item with a new count.
        /// </summary>
        public Item(Item item, int count)
        {
            _itemData = item.Data;

            if (count < 0)
            {
                count = StackSize;
            }
            _count = count;
        }
        
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
        /// Returns true if the items have the same id.
        /// </summary>
        public bool CompareTo(Item other)
        {
            if (_itemData.Id == other.Id) return true;
            return false;
        }
    }
}