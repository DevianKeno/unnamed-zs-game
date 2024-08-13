using System;
using System.Collections.Generic;

using UZSG.Inventory;
using UZSG.Items;

namespace UZSG
{
    public partial class Container
    {
        public event EventHandler<Item> OnExcessItem;

        /// <summary>
        /// Puts the Item to the nearest slot.
        /// Tries to combine with the same existing Items in the Container.
        /// </summary>
        public virtual bool TryPutNearest(Item item)
        {
            /// Check first cached Item Id slots, for combining
            if (CanPutItem(item, out var cachedSlots))
            {
                if (cachedSlots == null) /// Item does not exists yet in Container
                {
                    return TryPutNearestEmpty(item);
                }

                /// Put along cached slots
                Item remaining = item;
                foreach (var slot in cachedSlots)
                {
                    if (slot.TryCombine(remaining, out var excess))
                    {
                        remaining = excess;
                        if (remaining.IsNone) break;
                    }
                    /// If for some reason, suddenly cannot put anymore ¯\_(ツ)_/¯
                    /// but it shouldn't go here, because of the first gate clause
                    else 
                    {
                        OnExcessItem?.Invoke(this, excess);
                        return false;
                    }
                }

                return true;
            }

            return false; 
        }

        /// <summary>
        /// Tries to put the Item in the specified slot index.
        /// </summary>
        public virtual bool TryPutIn(int slotIndex, Item item)
        {
            if (item.IsNone) return true;
            if (!Slots.IsValidIndex(slotIndex)) return false;

            ItemSlot slot = Slots[slotIndex];
            if (slot.IsEmpty)
            {
                return slot.TryPut(item);
            }
            else
            {
                if (slot.TryCombine(item, out Item excess))
                {
                    if (!excess.IsNone)
                    {
                        OnExcessItem?.Invoke(this, excess);
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
        /// Tries to put the item to the nearest empty slot.
        /// Returns true if put succesfully, otherwise false.
        /// </summary>
        public virtual bool TryPutNearestEmpty(Item item)
        {
            if (item.IsNone) return true;
            if (IsFull) return false;

            foreach (ItemSlot slot in Slots)
            {
                if (slot.IsEmpty) /// just put
                {
                    slot.Put(item);
                    return true;
                }
                
                /// HmmHmmHmm, I think I can remove this 77% sure
                if (slot.TryCombine(item, out Item excess))
                {
                    if (TryPutNearestEmpty(excess))
                    {
                        return true;
                    }
                    else
                    {
                        if (!excess.IsNone)
                        {
                            OnExcessItem?.Invoke(this, excess);
                        }
                    }

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
                // UncacheItem(itemsToTake, slot);
                // OnSlotContentChanged?.Invoke(this, new()
                // {
                //     Slot = slot
                // });
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
            // UncacheItem(itemsToTake, slot);
            // OnSlotContentChanged?.Invoke(this, new()
            // {
            //     Slot = slot
            // });
            return itemsToTake;
        }

        public virtual void ClearItem(ItemSlot slot)
        {
            if (slot == null || slot.IsEmpty) return;

            // UncacheItem(slot.Item, slot);
            slot.Clear();
            // OnSlotContentChanged?.Invoke(this, new()
            // {
            //     Slot = slot
            // });
        }

        public virtual void ClearItem(int slotIndex)
        {
            if (!Slots.IsValidIndex(slotIndex)) return;

            ClearItem(Slots[slotIndex]);
        }
    }
}
