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
        public const int MAX_STACK_SIZE = 99999;
        public const int NONE_ITEM_HASH_CODE = -1;
        public static Item None => new(null);
        
        [FormerlySerializedAs("_itemData")]
        [SerializeField] ItemData itemData;
        public ItemData Data
        {
            get
            {
                return itemData;
            }
            set
            {
                itemData = value;
                this._hashCode = GetHashCode();
            }
        }
        [FormerlySerializedAs("_count")]
        [SerializeField] int count;
        public int Count
        {
            get => count;
            set => count = value;
        }
        public bool HasAttributes { get; private set; }
        [SerializeField] AttributeCollection attributes;
        public AttributeCollection Attributes
        {
            get => attributes;
            set
            {
                attributes = value;
                HasAttributes = attributes != null;
            }
        }
        public bool IsNone => _hashCode == -1 || this.itemData == null;
        public string Id => itemData.Id;
        /// <summary>
        /// Called whenever this Item is changed/updated.
        /// </summary>
        public event Action OnChanged;
        int _hashCode = -1;


        #region Item constructors

        /// <summary>
        /// Create an Item object from ItemData with count.
        /// </summary>
        public Item(ItemData data, int count)
        {
            Data = data;
            this.count = count;
        }
        
        /// <summary>
        /// Create an Item by Id.
        /// </summary>
        public Item(string id, int count)
        {
            if (Game.Items.TryGetData(id, out var itemData))
            {
                this.Data = itemData;
                this.count = count;
            }
            else
            {
                this.Data = null;
                this.count = 0;
            }
        }
                        
        /// <summary>
        /// Create a copy of the Item, either with/without a new count.
        /// A count of -1 retains the original count of the copied Item,
        /// while passing a new count will create a copy of the Item with the new count.
        /// </summary>
        public Item(Item other, int count = -1)
        {
            if (other == null || other.IsNone)
            {
                this.Data = null;
                this.count = 0;
            }
            else
            {
                this.Data = other.Data;
                this.count = count < 0 ? other.count : count;

                if (other.HasAttributes)
                {
                    this.attributes = new();
                    this.attributes.AddList(other.Attributes.List);
                    this.HasAttributes = true;
                }
            }
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
        /// Take an amount from this item.
        /// </summary>
        public Item Take(int amount)
        {
            count -= Math.Clamp(amount, 0, count);
            OnChanged?.Invoke();
            return new(itemData, amount);
        }

        public Item Copy()
        {
            return new(this);
        }

        public bool Stack(Item other)
        {
            if (!this.IsNone)
            {
                if (!CompareTo(other)
                || !itemData.IsStackable)
                {
                    return false; 
                }
            }
            
            this.count += other.Count;
            OnChanged?.Invoke();
            return true;
        }
        
        /// <summary>
        /// Try to stack this Item with the other item.
        /// Immediately stacks the Item if possible, does nothing otherwise.
        /// If you just want to just check use <c>CanStackWith()</c> instead.
        /// </summary>
        public bool TryStack(Item other, out Item excess, bool useMaxStackSize = false)
        {
            excess = Item.None;
            if (!CanStackWith(other)) return false;
            if (this.IsNone)
            {
                if (other.IsNone) return true;
                this.Data = this.IsNone ? other.Data : itemData;
            }

            this.count += other.Count;
            if (count > this.Data.StackSize)
            {
                excess = new Item(this.itemData, count - this.Data.StackSize);
                this.count -= excess.Count;
            }

            OnChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Checks if this Item can be stacked with the other Item.
        /// Returns false if the total of the combined Items exceeds the Item's stack size, unless maxed.
        /// </summary>
        public bool CanStackWith(Item other, bool useMaxStackSize = false)
        {
            if (this.IsNone || other.IsNone) return true;
            if (!CompareTo(other)
            || !itemData.IsStackable
            || this.HasAttributes || other.HasAttributes) return false; /// items with attributes cannot be stacked :P
            
            return (count + other.Count) <= (useMaxStackSize ? MAX_STACK_SIZE : this.itemData.StackSize);
        }

        /// <summary>
        /// Returns true if the Items are of the same Id.
        /// </summary>
        public bool Is(Item other)
        {
            return CompareTo(other);
        }

        /// <summary>
        /// Returns true if the Items are of the same Id.
        /// </summary>
        public bool CompareTo(Item other)
        {
            /// compares the hash code of the Id string
            return _hashCode == other._hashCode;
        }

        public void Refresh()
        {
            this._hashCode = GetHashCode();
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
            return itemData?.GetHashCode() ?? NONE_ITEM_HASH_CODE;
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