using System;
using System.Collections.Generic;

using UZSG.Inventory;
using UZSG.Items;

namespace UZSG
{
    public partial class Container
    {
        /// <summary>
        /// Tries to put the Item in the specified slot index.
        /// </summary>
        public virtual bool TryPut(int slotIndex, Item item)
        {
            if (!Slots.IsValidIndex(slotIndex)) return false;
            if (item == null || item.IsNone) return true;

            ItemSlot slot = Slots[slotIndex];
            if (slot == null) return false; /// wdym slot ns null?
            
            if (slot.IsEmpty)
            {
                if (slot.TryPut(item))
                {
                    CacheItem(item, slot);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (slot.TryCombine(item, out Item excess))
                {
                    CacheItem(item, slot);
                    return true;
                }
                else
                {
                    return false;
                }
            }
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
                    CacheItem(item, s);
                    return true;
                }
                
                if (s.TryCombine(item, out Item excess))
                {
                    if (TryPutNearest(excess))
                    {
                        CacheItem(item, s);
                        return true;
                    }
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
        /// Take entire stack.
        /// </summary>
        public virtual Item Take(int slotIndex)
        {
            if (!Slots.IsValidIndex(slotIndex)) return Item.None;

            ItemSlot slot = Slots[slotIndex];
            if (slot.IsEmpty)
            {
                return Item.None;
            }
            else
            {
                var itemsToTake = slot.TakeAll();
                UncacheItem(itemsToTake, slot);
                return itemsToTake;
            }
        }

        /// <summary>
        /// Take some amount of Item from Slot.
        /// </summary>
        public virtual Item TakeItems(int slotIndex, int amount)
        {           
            if (!Slots.IsValidIndex(slotIndex)) return Item.None;

            ItemSlot slot = Slots[slotIndex];
            if (slot.IsEmpty) return Item.None;

            
            var itemsToTake = slot.TakeItems(amount);
            UncacheItem(itemsToTake, slot);
            return itemsToTake;
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
