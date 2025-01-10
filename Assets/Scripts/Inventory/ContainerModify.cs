using System;
using System.Collections.Generic;
using System.Linq;
using UZSG.Inventory;
using UZSG.Items;

namespace UZSG
{
    public partial class Container
    {
        /// <summary>
        /// Called when there's an unhandled excess item.
        /// </summary>
        public event Action<Item> OnExcessItem;

        /// <summary>
        /// Puts the Item to the nearest possible Slot.
        /// Tries to combine it with the same existing Items in the Container, otherwise to an empty Slot.
        /// </summary>
        public bool TryPutNearest(Item item)
        {
            if (item.IsNone) return true;

            /// Check first the cached ItemSlots containing the same Item
            if (_cachedIdSlots.TryGetValue(item.Data.Id, out var slots))
            {
                var nextItem = item;
                foreach (ItemSlot slot in slots)
                {
                    if (slot.TryCombine(nextItem, out Item excess))
                    {
                        if (excess.IsNone) return true;
                        nextItem = excess;
                    }
                }
            }
            /// new item
            return TryPutNearestEmpty(item);
        }

        /// <summary>
        /// Tries to put the item to the nearest empty slot.
        /// Returns true if put succesfully, otherwise false.
        /// </summary>
        public bool TryPutNearestEmpty(Item item)
        {
            if (item.IsNone) return true;
            if (IsFull) return false;

            foreach (ItemSlot slot in Slots)
            {
                if (slot.IsEmpty) /// just put
                {
                    Slots[slot.Index].Put(item);
                    return true;
                }
            }

            return false;
        }
        
        /// <summary>
        /// Puts the Item to the nearest possible Slot.
        /// Tries to combine it with the same existing Items in the Container, otherwise to an empty Slot.
        /// </summary>
        public bool TryPutNearest(Item item, out Item excess)
        {
            excess = Item.None;
            if (item.IsNone) return true;

            /// Check first the cached ItemSlots containing the same Item
            if (_cachedIdSlots.TryGetValue(item.Data.Id, out var slots))
            {
                var nextItem = item;
                foreach (ItemSlot slot in slots)
                {
                    if (slot.TryCombine(nextItem, out excess))
                    {
                        if (excess.IsNone) return true;
                        nextItem = excess;
                    }
                }
            }
            /// new item
            return TryPutNearestEmpty(item);
        } 

        /// <summary>
        /// Tries to put the Item in the specified slot index.
        /// </summary>
        public bool TryPutAt(int slotIndex, Item item)
        {
            if (item.IsNone) return true;
            if (!Slots.IsValidIndex(slotIndex)) return false;

            ItemSlot slot = Slots[slotIndex];
            if (slot.IsEmpty)
            {
                slot.Put(item);
                return true;
            }
            else
            {
                if (slot.TryCombine(item, out Item excess))
                {
                    if (!excess.IsNone)
                    {
                        OnExcessItem?.Invoke(excess);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Take Items from this container given the Item.
        /// If the given Item is more than what's stored in this container, it simply takes all.
        /// </summary>
        public virtual Item TakeItem(Item item)
        {
            Item toReturn = Item.None;
            if (item == null || item.Count == 0) return toReturn;
            
            var remaining = item.Count;

            if (IdSlots.TryGetValue(item.Id, out var hashset))
            {
                foreach (var slot in hashset.ToList())
                {
                    var tookItem = slot.TakeItems(remaining);
                    toReturn.Combine(tookItem);
                    remaining -= tookItem.Count;

                    if (remaining <= 0) break;
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Take a list of Items from this container given the list of Items.
        /// This takes all Items, even when other Items in the given list are not present in this Container.
        /// </summary>
        public virtual List<Item> TakeItems(List<Item> items)
        {
            List<Item> toReturn = new();

            foreach (var item in items)
            {
                toReturn.Add(TakeItem(item));
            }
            
            return toReturn;
        }

        /// <summary>
        /// Take the entire stack from a specific slot index.
        /// </summary>
        public virtual Item TakeFrom(int slotIndex)
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
                return itemsToTake;
            }
        }

        /// <summary>
        /// Take some amount of Item from Slot.
        /// </summary>
        public virtual Item TakeFrom(int slotIndex, int amount)
        {           
            if (!Slots.IsValidIndex(slotIndex)) return Item.None;

            ItemSlot slot = Slots[slotIndex];
            if (slot.IsEmpty) return Item.None;
            
            var itemsToTake = slot.TakeItems(amount);
            return itemsToTake;
        }

        public virtual void ClearItem(ItemSlot slot)
        {
            if (slot == null || slot.IsEmpty) return;

            slot.Clear();
        }

        public virtual void ClearItem(int slotIndex)
        {
            if (!Slots.IsValidIndex(slotIndex)) return;

            ClearItem(Slots[slotIndex]);
        }
    }
}
