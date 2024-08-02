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
        public abstract int SlotsCount { get; set; }
        public abstract List<ItemSlot> Slots { get; }
        /// <summary>
        /// Called whenever the content of a Slot is changed.
        /// </summary>
        public event EventHandler<SlotContentChangedArgs> OnSlotContentChanged;

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
            if (index < 0 || index > Slots.Count) return null;
            return Slots[index];
        }
        
        public bool TryGetSlot(int index, out ItemSlot slot)
        {
            if (index < 0 || index > Slots.Count)
            {
                slot = null;
                return false;
            }
            slot = Slots[index];
            return true;
        }

        public virtual Item ViewItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex > Slots.Count) return Item.None;
            return Slots[slotIndex].Item;
        }

        /// <summary>
        /// Tries to put the item to the nearest empty slot.
        /// Returns true if put succesfully, otherwise false.
        /// </summary>
        public virtual bool TryPutNearest(Item item)
        {
            if (item == Item.None) return true;

            foreach (ItemSlot slot in Slots)
            {
                if (slot.IsEmpty) /// just put
                {
                    slot.Put(item);
                    return true;
                }
                
                if (slot.TryCombine(item, out Item excess))
                {
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
            ItemSlot slot = Slots[slotIndex];
            if (slot.IsEmpty) return Item.None;

            return slot.TakeItems(-1);
        }

        /// <summary>
        /// Take some amount of Item from Slot.
        /// </summary>
        public virtual Item TakeItems(int slotIndex, int amount)
        {           
            ItemSlot slot = Slots[slotIndex];
            if (slot.IsEmpty) return Item.None;

            return slot.TakeItems(amount);
        }
        
        /// <summary>
        /// Check if the item exists within the container, regardless of amount.
        /// </summary>
        public bool Contains(Item item, out ItemSlot slot)
        {
            slot = null;
            foreach (ItemSlot s in Slots)
            {
                if (s.IsEmpty) continue;

                if (s.Item.CompareTo(item))
                {
                    slot = s;
                    return true;
                }
            }
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
        /// Prints the items inside the container
        /// </summary>
        public virtual void PrintItems()
        {
            foreach(ItemSlot slot in Slots){

                if(slot.Item.CompareTo(Item.None)){
                    continue;
                }
                print($"Slot {slot.Index}: {slot.Item.Name} ({slot.Item.Count})");
            }
        }


        public virtual void RemoveItems(List<Item> items)
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
    }
}
