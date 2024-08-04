using System;
using System.Collections.Generic;
using UnityEngine;
using UZSG.Inventory;
using UZSG.Items;
using UZSG.Systems;

namespace UZSG
{
    /// <summary>
    /// Base class for all Containers that have ItemSlots.
    /// </summary>
    public abstract class Container : MonoBehaviour
    {
        public int SlotsCount { get; set; }
        [SerializeField] protected List<ItemSlot> _slots = new();
        public List<ItemSlot> Slots => _slots;
        public bool IsFull
        {
            get
            {
                foreach (var slot in Slots)
                {
                    if (slot == null) continue;
                    if (slot.IsEmpty) return false;
                }
                return true;
            }
        }


        #region Events

        /// <summary>
        /// Called whenever the content of a Slot is changed.
        /// </summary>
        public event EventHandler<SlotContentChangedArgs> OnSlotContentChanged;

        #endregion
        

        public ItemSlot this[int i]
        {
            get
            {
                if (!Slots.IsValidIndex(i)) return null;
                return _slots[i];
            }
        }

        protected virtual void SlotContentChanged(object slot, ItemSlot.ContentChangedArgs e)
        {
            OnSlotContentChanged?.Invoke(this, new()
            {
                Slot = (ItemSlot) slot
            });
        }

        [Obsolete("You are advised to use TryGetSlot() instead.")]
        public ItemSlot GetSlot(int index)
        {
            if (!Slots.IsValidIndex(index))
            {
                return null;
            }
            return Slots[index];
        }
        
        public bool TryGetSlot(int index, out ItemSlot slot)
        {
            if (!Slots.IsValidIndex(index))
            {
                slot = null;
                return false;
            }
            slot = Slots[index];
            return true;
        }

        public virtual Item ViewItem(int slotIndex)
        {
            if (!Slots.IsValidIndex(slotIndex)) return Item.None;
            return Slots[slotIndex].Item;
        }

        /// <summary>
        /// Tries to put the item to the nearest empty slot.
        /// Returns true if put succesfully, otherwise false.
        /// </summary>
        public virtual bool TryPutNearest(Item item)
        {
            if (item.IsNone) return true;
            if (IsFull) return false;

            foreach (ItemSlot s in Slots)
            {
                if (s == null) continue;
                if (s.IsEmpty) /// just put
                {
                    s.Put(item);
                    return true;
                }
                
                if (s.TryCombine(item, out Item excess))
                {
                    if (TryPutNearest(excess)) return true;
                    continue;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Tries to put the item to the nearest empty slot.
        /// Returns true if put succesfully, otherwise false.
        /// </summary>
        public virtual bool TryPutNearest(Item item, out ItemSlot slot)
        {
            slot = null;
            if (item.IsNone) return true;
            if (IsFull) return false;

            foreach (ItemSlot s in Slots)
            {
                if (s == null) continue;
                if (s.IsEmpty) /// just put
                {
                    slot = s;
                    s.Put(item);
                    return true;
                }
                
                if (s.TryCombine(item, out Item excess))
                {
                    slot = s; /// IDK ABOUT THIS
                    if (TryPutNearest(excess)) return true;
                    continue;
                }
            }
            return false;
        }
    
        /// <summary>
        /// Tries to put the Item in the specified slot index.
        /// </summary>
        public virtual bool TryPut(int slotIndex, Item item)
        {
            if (!Slots.IsValidIndex(slotIndex)) return false;
            if (item.IsNone) return false;

            ItemSlot slot = Slots[slotIndex];
            if (slot == null) return false;
            
            if (slot.IsEmpty)
            {
                return slot.TryPut(item);
            }
            else
            {
                return slot.TryCombine(item, out Item excess);
            }
        }       

        /// <summary>
        /// Take entire stack.
        /// </summary>
        public virtual Item Take(int slotIndex)
        {
            if (!Slots.IsValidIndex(slotIndex)) return Item.None;

            ItemSlot slot = Slots[slotIndex];
            if (slot.IsEmpty) return Item.None;

            return slot.TakeAll();
        }

        /// <summary>
        /// Take some amount of Item from Slot.
        /// </summary>
        public virtual Item TakeItems(int slotIndex, int amount)
        {           
            if (!Slots.IsValidIndex(slotIndex)) return Item.None;

            ItemSlot slot = Slots[slotIndex];
            if (slot.IsEmpty) return Item.None;

            return slot.TakeItems(amount);
        }
        
        /// <summary>
        /// Check if the item exists within the container, regardless of amount.
        /// </summary>
        public bool Contains(Item item, out ItemSlot slot)
        {
            foreach (ItemSlot s in Slots)
            {
                if (s.IsEmpty) continue;

                if (s.Item.CompareTo(item))
                {
                    slot = s;
                    return true;
                }
            }
            
            slot = null;
            return false;
        }

        /// <summary>
        /// Check if a specified amount of item exists within the container.
        /// </summary>
        public bool ContainsCount(Item item, int amount, out List<ItemSlot> slots)
        {
            slots = new();
            int remaining = item.Count;
            int count = 0;

            foreach (ItemSlot slot in Slots)
            {
                if (slot.IsEmpty) continue;

                if (slot.Item.CompareTo(item))
                {
                    count += slot.Item.Count;
                    remaining -= slot.Item.Count;
                    slots.Add(slot);

                    if (count >= amount)
                    {
                        return true;
                    }
                }
            }

            return count >= item.Count;
        }

        /// <summary>
        /// Counts the number of items inside the container and outputs the slots
        /// </summary>
        public int CountItem(Item item, out List<ItemSlot> slots)
        {
            slots = new();
            int remaining = item.Count;
            int count = 0;

            foreach (ItemSlot slot in Slots)
            {
                if (slot.IsEmpty) continue;

                if (slot.Item.CompareTo(item))
                {
                    count += slot.Item.Count;
                    remaining -= slot.Item.Count;
                    slots.Add(slot);
                }
            }

            return count;
        }

        /// <summary>
        /// Combines this container to another container. <Read Only>
        /// Returns a combined COPY of the slots of both containers.
        /// </summary>
        public List<ItemSlot> Combine(Container other)
        {
            List<ItemSlot> slots = new();
            slots.AddRange(Slots);
            slots.AddRange(other.Slots);
            return slots;
        }
        
        /// <summary>
        /// Removes item with count from the container.
        /// </summary>
        /// <param name="item"></param>
        public virtual void Remove(Item item)
        {
        }

        /// <summary>
        /// Removes ALL items from the container.
        /// </summary>
        public virtual void RemoveAll(Item item)
        {
        }

        public virtual void ClearItem(ItemSlot slot)
        {
            if (slot.IsEmpty) return;

            slot.Clear();
        }

        public virtual void ClearItem(int slotIndex)
        {
            if (!Slots.IsValidIndex(slotIndex)) return;

            ClearItem(Slots[slotIndex]);
        }

        /// <summary>
        /// Prints the items inside the container.
        /// </summary>
        public virtual void PrintItems()
        {
            foreach (ItemSlot slot in Slots)
            {
                if (slot.IsEmpty) continue;
                print($"Slot {slot.Index}: {slot.Item.Data.Name} ({slot.Item.Count})");
            }
        }
    }
}
