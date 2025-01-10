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
        public static Item None => new(null);
        
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
        public bool HasAttributes { get; private set; }
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
                HasAttributes = attributes != null;
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
        /// Create an Item object from ItemData with count.
        /// </summary>
        public Item(ItemData data, int count)
        {
            this.itemData = data;
            this.count = count;
        }
        
        /// <summary>
        /// Create an Item by Id.
        /// </summary>
        public Item(string id, int count)
        {
            if (Game.Items.TryGetData(id, out var itemData))
            {
                this.itemData = itemData;
                this.count = count;
            }
            else
            {
                this.itemData = null;
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
                this.itemData = null;
                this.count = 0;
            }
            else
            {
                this.itemData = other.Data;
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

        public Item Copy()
        {
            return new(this);
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

            if (this.IsNone) /// this ItemSlot has no Item in it
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
            return itemData != null
                && other != null
                && other.Data != null
                && itemData.Id == other.Data.Id;
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