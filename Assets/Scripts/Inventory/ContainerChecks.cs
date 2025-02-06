using System;
using System.Collections.Generic;


using UZSG.Inventory;
using UZSG.Items;
using System.Linq;

namespace UZSG
{
    public partial class Container
    {
        int _freeSlotsCounter;

        /// <summary>
        /// Check if the Item exists within the Container.
        /// Takes into account the Count of the given Item.
        /// Passing a None Item returns false.
        /// </summary>
        public bool Contains(Item item)
        {
            if (item.IsNone) return false;

            if (_cachedIdItemCount.TryGetValue(item.Id, out var count))
            {
                return count >= item.Count;
            }

            return false;
        }

        /// <summary>
        /// Checks if ALL Items in the list exists within the container.
        /// Takes into account the Count of the given Items.
        /// </summary>
        public bool ContainsAll(List<Item> items)
        {
            foreach (var item in items)
            {
                if (item.IsNone) continue;
                if (!Contains(item)) return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Finds the Item of Id in this Container.
        /// Returns the list of Slots the Item is put in, which is empty if it does not exist.
        /// </summary>
        public List<ItemSlot> FindItem(string id)
        {
            if (_cachedIdSlots.TryGetValue(id, out var slots))
            {
                return slots.ToList(); 
            }

            return new();
        }

        /// <summary>
        /// Finds the Item in this Container.
        /// Returns the list of Slots the Item is put in, which is empty if it does not exist.
        /// </summary>
        public List<ItemSlot> FindItem(Item item)
        {
            return FindItem(item.Data.Id);
        }

        /// <summary>
        /// Checks if the given Item can be put in the Container.
        /// </summary>
        /// <param name="max">Whether to allow max stacking, up to 99999.</param>
        public bool CanPutItem(Item item, bool max = false)
        {
            if (item.IsNone) return true;

            /// Check first the cached ItemSlots containing the same Item
            if (_cachedIdSlots.TryGetValue(item.Data.Id, out var slots))
            {
                int nextCount = item.Count;
                foreach (ItemSlot slot in slots)
                {
                    var nextItem = new Item(item, nextCount);
                    if (slot.Item.CanStackWith(nextItem, max)) return true;
                    
                    nextCount -= slot.Item.Data.StackSize - slot.Item.Count;
                    if (nextCount <= 0) break;
                }
            }
            
            return CanPutNearestEmpty(item);
        }

        /// <summary>
        /// Checks if the given Items can be put in the Container.
        /// Returns false if at least one Item cannot be put in.
        /// </summary>
        /// <param name="max">Whether to allow max stacking, up to 99999.</param>
        public bool CanPutItems(List<Item> items, bool max = false)
        {
            if (IsFull) return false;
            
            _freeSlotsCounter = FreeSlotsCount;
            foreach (var item in items)
            {
                if (_freeSlotsCounter <= 0) return false;
                if (!CanPutItem(item, max)) return false;
            }

            return true;
        }

        /// <summary>
        /// Check if the given Item can be put to a nearest empty Slot.
        /// </summary>
        public bool CanPutNearestEmpty(Item item)
        {
            if (item.IsNone) return true;
            if (IsFull) return false;

            foreach (ItemSlot slot in Slots)
            {
                if (slot.IsEmpty)
                {
                    _freeSlotsCounter--;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Counts the Item present in this container.
        /// </summary>
        public int CountId(string itemId)
        {
            if (_cachedIdItemCount.TryGetValue(itemId, out var count))
            {
                return count;
            }
            
            return 0;
        }

        /// <summary>
        /// Counts the Item present in this container.
        /// </summary>
        public int CountItem(Item item)
        {
            if (_cachedIdItemCount.TryGetValue(item.Id, out var count))
            {
                return count;
            }
            
            return 0;
        }
    }
}
