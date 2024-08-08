using System;
using System.Collections.Generic;

using UZSG.Systems;
using UZSG.Inventory;
using UZSG.Items;

namespace UZSG
{
    public partial class Container
    {
        int _freeSlotsCounter;

        /// <summary>
        /// Checks if the given item can be put in the Container.
        /// </summary>
        public virtual bool CanPutItem(Item item)
        {
            /// Check first the ItemSlots containing the same Item
            if (_cachedIdSlots.TryGetValue(item.Data.Id, out var slots))
            {
                foreach (ItemSlot slot in slots)
                {
                    if (slot == null) continue;
                    if (slot.IsEmpty) return true;
                    if (slot.Item.CanBeStackedWith(item)) return true;
                }
            }
            return CanPutNearest(item);
        }

        /// <summary>
        /// Checks if the given items can be put in the Container.
        /// Returns false if at least one item cannot be put in.
        /// </summary>
        public virtual bool CanPutItems(List<Item> items)
        {
            if (IsFull) return false;
            
            _freeSlotsCounter = FreeSlotsCount;
            foreach (var item in items)
            {
                if (_freeSlotsCounter <= 0) return false;
                if (!CanPutItem(item))
                {
                    return false;
                }
            }

            return false;
        }

        public virtual bool CanPutNearest(Item item)
        {
            if (item.IsNone) return true;
            if (IsFull) return false;

            foreach (ItemSlot slot in Slots)
            {
                if (slot == null) continue;
                if (slot.IsEmpty)
                {
                    _freeSlotsCounter--;
                    return true;
                }
            }
            return false;
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
    }
}
