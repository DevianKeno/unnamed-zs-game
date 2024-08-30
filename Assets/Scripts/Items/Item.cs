using System;

using UnityEngine;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Saves;
using UZSG.Attributes;
using UnityEngine.Serialization;
using System.Collections.Generic;

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
    public class Item : IAttributable, IComparable<Item>, ISaveDataReadWrite<ItemSaveData>
    {
        public const int MaxStackSize = 99999;
        public static Item None => new(data: null);
        
        [FormerlySerializedAs("_itemData")]
        [SerializeField] ItemData itemData;
        public ItemData Data
        {
            get
            {
                return itemData;
            }
        }
        [FormerlySerializedAs("_count")]
        [SerializeField] int count;
        public int Count
        {
            get
            {
                return count;
            }
            set
            {
                count = value;
            }
        }
        [SerializeField] AttributeCollection attributes;
        public AttributeCollection Attributes
        {
            get
            {
                return attributes;
            }
            set
            {
                attributes = value;
            }
        }

        public bool IsNone => itemData == null;
        public string Id => itemData.Id;
        /// <summary>
        /// Called whenever this Item is changed/updated.
        /// </summary>
        public event Action OnChanged;


        #region Item constructors

        /// <summary>
        /// Create 'None' item.
        /// </summary>
        public Item(ItemData data = null)
        {
            itemData = data;
            count = 0;
        }

        /// <summary>
        /// Create an Item object from ItemData with count.
        /// </summary>
        public Item(ItemData data, int count = 1)
        {
            itemData = data;
            this.count = count;
        }
        
        /// <summary>
        /// Create an item by id.
        /// </summary>
        public Item(string id, int count = 1)
        {
            itemData = Game.Items.GetData(id);
            this.count = count;
        }
                        
        /// <summary>
        /// Create a copy of the Item.
        /// </summary>
        public Item(Item other)
        {
            itemData = other.Data;
            count = other.Count;
        }
                        
        /// <summary>
        /// Create a copy of the Item with a new count.
        /// </summary>
        public Item(Item other, int count = 1)
        {
            itemData = other.Data;
            this.count = count;
        }

        #endregion
        

        #region Save read/write
                
        public void ReadSaveData(ItemSaveData saveData)
        {
            
        }

        public ItemSaveData WriteSaveData()
        {
            var saveData = new ItemSaveData()
            {
                Id = IsNone ? "none" : itemData.Id,
                Count = count,
            };

            if (attributes != null)
            {
                saveData.HasAttributes = true;
                saveData.Attributes = Attributes.WriteSaveData();
            }

            return saveData;
        }

        #endregion


        #region Public methods

        /// <summary>
        /// Clamp this Item's count to either its StackSize or MaxStack.
        /// </summary>
        public void ClampStack(bool max = false)
        {
            int maxStack = max ? MaxStackSize : itemData.StackSize;
            count = Math.Clamp(count, 1, maxStack);
        }

        /// <summary>
        /// Take an amount from this item.
        /// </summary>
        public Item Take(int amount)
        {
            count -= amount;
            OnChanged?.Invoke();
            return new(itemData, amount);
        }
        
        /// <summary>
        /// Try to combine this Item with the other item.
        /// Exceeding stack size.
        /// </summary>
        public void Combine(Item other)
        {
            if (IsNone)
            {
                itemData = other.itemData;
            }
            count += other.Count;
            OnChanged?.Invoke();
        }

        /// <summary>
        /// Try to combine this Item with the other item.
        /// </summary>
        public bool TryCombine(Item other, out Item excess, bool max = false)
        {
            excess = None;
            if (!CanCombineWith(other)) return false;

            if (IsNone)
            {
                itemData = other.itemData;
            }
            int maxStack = max ? MaxStackSize : itemData.StackSize;
            count += other.Count;
            if (count > maxStack)
            {
                excess = new(this, count - maxStack);
            }
            OnChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Try to stack this Item until max stack size.
        /// </summary>
        public bool TryCombineMax(Item other, out Item excess)
        {
            excess = None;
            if (!CanCombineWith(other)) return false;

            count += other.Count;
            if (count > MaxStackSize)
            {
                excess = new(this, count - MaxStackSize);
            }
            OnChanged?.Invoke();
            return true;
        }
        
        /// <summary>
        /// Try to stack this Item with the other item.
        /// </summary>
        public bool TryStack(Item other)
        {
            if (!CanStackWith(other)) return false;

            count += other.Count;
            OnChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Checks if this Item can be combined with the other Item.
        /// </summary>
        public bool CanCombineWith(Item other)
        {
            if (IsNone) return true;                    /// This is an empty Item (lol)
            if (!itemData.IsStackable) return false;    /// Not stackable
            if (!CompareTo(other)) return false;        /// Not the same

            return true;
        }

        /// <summary>
        /// Checks if this Item can be stacked with the other Item.
        /// Returns false if the total of the combined Items exceeds the Item's stack size, unless maxed.
        /// </summary>
        public bool CanStackWith(Item other, bool max = false)
        {
            if (!CanCombineWith(other)) return false;

            int maxStack = IsNone ? other.Data.StackSize : Data.StackSize;
            maxStack = max ? MaxStackSize : maxStack;

            return count + other.Count <= maxStack;
        }

        /// <summary>
        /// Returns true if the Items are of the same Id.
        /// </summary>
        public bool CompareTo(Item other)
        {
            return itemData == other.Data;
        }

        /// <summary>
        /// Multiply the count of this Item by the value.
        /// </summary>
        public static Item operator *(Item item, int value)
        {
            return new Item(item, item.Count * value);
        }

        public override bool Equals(object obj)
        {
            if (obj is Item item)
            {
                return itemData == item.Data;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return itemData?.GetHashCode() ?? 0;
        }

        public static bool operator ==(Item left, Item right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Item left, Item right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}