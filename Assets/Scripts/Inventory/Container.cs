using System;
using UnityEngine;
using UZSG.Inventory;
using UZSG.Items;

namespace UZSG
{
    /// <summary>
    /// Base class for all Containers that have ItemSlots.
    /// </summary>
    public abstract class Container : MonoBehaviour
    {
        public abstract ItemSlot[] Slots { get; }
        /// <summary>
        /// Called whenever the content of a Slot is changed.
        /// </summary>
        public event EventHandler<SlotContentChangedArgs> OnSlotContentChanged;

        protected virtual void SlotContentChanged(object slot, ItemSlot.ContentChangedArgs e)
        {
            OnSlotContentChanged?.Invoke(this, new((ItemSlot) slot));
        }

        public ItemSlot GetSlot(int index)
        {
            if (index < 0 || index > Slots.Length) return null;
            return Slots[index];
        }

        public virtual Item ViewItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex > Slots.Length) return Item.None;
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
                if (slot.IsEmpty) // just put
                {
                    slot.PutItem(item);
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
            if (slotIndex < 0 || slotIndex > Slots.Length) return false;
            if (item == Item.None) return true;

            ItemSlot slot = Slots[slotIndex];

            if (slot.IsEmpty)
            {
                slot.PutItem(item);
                return true;
            } else
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

        public virtual Item Swap(int slotIndex)
        {
            return Item.None;
        }

        public virtual Item ThrowItem(int index)
        {
            return Item.None;
        }

        public virtual void ClearItem(ItemSlot slot)
        {
            if (slot.IsEmpty) return;
            slot.Clear();
        }

        public virtual void ClearItem(int slotIndex)
        {
            ClearItem(Slots[slotIndex]);
        }
    }
}
