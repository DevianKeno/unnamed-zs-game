using System;
using Unity.VisualScripting;
using UnityEngine;
using URMG.Items;
using URMG.UI;

namespace URMG.Inventory
{
    public struct SlotContentChangedArgs
    {
        /// <summary>
        /// The Slot that has changed.
        /// </summary>
        public ItemSlot Slot;
    }

    /// <summary>
    /// Manages the Bag screen.
    /// </summary>
    public class BagHandler : MonoBehaviour
    {
        public int MaxBagSlots = 18;
        BagData data;
        [SerializeField] ItemSlot[] _slots;
        int[] _emptySlots;
        int[] _occupiedSlots;

        /// <summary>
        /// Called whenever the content of a Slot is changed.
        /// </summary>
        public event EventHandler<SlotContentChangedArgs> OnSlotContentChanged;

        void Awake()
        {
            _slots = new ItemSlot[MaxBagSlots];
            _emptySlots = new int[MaxBagSlots];
            _occupiedSlots = new int[MaxBagSlots];
        }

        void Start()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                ItemSlot newSlot = new(i);
                newSlot.OnContentChanged += SlotContentChanged;
                _slots[i] = newSlot;
            }
        }

        void SlotContentChanged(object sender, ItemSlot.ContentChangedArgs e)
        {
            OnSlotContentChanged?.Invoke(this, new()
            {
                Slot = (ItemSlot) sender
            });
        }

        public ItemSlot GetSlot(int index)
        {
            if (index < 0 || index > MaxBagSlots) throw new ArgumentOutOfRangeException();
            return _slots[index];
        }

        /// <summary>
        /// Tries to puts the item to the lowest indexed empty slot.
        /// </summary>
        public bool TryPutNearest(Item item)
        {
            if (item == Item.None) return true;

            foreach (ItemSlot slot in _slots)
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
        /// Tries to put the item in the given slot index.
        /// </summary>
        public bool TryPut(int slotIndex, Item item)
        {
            ItemSlot slot = GetSlot(slotIndex);

            if (slot.IsEmpty)
            {
                slot.PutItem(item);
                return true;
            }            
            
            if (slot.TryCombine(item, out Item excess))
            {
                if (TryPutNearest(excess)) return true;
                return false;
            }
            return false;
        }

        /// <summary>
        /// Take entire stack.
        /// </summary>
        public Item Take(int slotIndex)
        {
            ItemSlot slot = GetSlot(slotIndex);
            if (slot.IsEmpty) return Item.None;

            return slot.TakeItems(-1);
        }

        /// <summary>
        /// Take some amount of Item from Slot.
        /// </summary>
        public Item TakeItems(int slotIndex, int amount)
        {           
            ItemSlot slot = GetSlot(slotIndex);
            if (slot.IsEmpty) return Item.None;

            return slot.TakeItems(amount);
        }

        public Item Swap(int slotIndex)
        {
            return Item.None;
        }

        public void ThrowItem(int index)
        {
            
        }

        public void ClearItem(ItemSlot slot)
        {
            if (slot.IsEmpty) return;
            slot.Clear();
        }

        public void ClearItem(int slotIndex)
        {
            ClearItem(_slots[slotIndex]);
        }
    }
}
