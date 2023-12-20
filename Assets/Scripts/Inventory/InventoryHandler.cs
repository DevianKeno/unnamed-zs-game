using System;
using Unity.VisualScripting;
using UnityEngine;
using URMG.Items;
using URMG.UI;

namespace URMG.InventoryS
{
    public struct SlotContentChangedArgs
    {
        public Slot Slot;
    }

    /// <summary>
    /// Inventory manager.
    /// </summary>
    public class InventoryHandler : MonoBehaviour
    {
        public int MaxBagSlots = 18;
        InventoryData data;
        [SerializeField] Slot[] _slots;
        Slot[] _emptySlots;
        Slot[] _occupiedSlots;

        /// <summary>
        /// Called whenever the content of a Slot is changed.
        /// </summary>
        public event EventHandler<SlotContentChangedArgs> OnSlotContentChanged;

        void Awake()
        {
            _slots = new Slot[MaxBagSlots];
            _emptySlots = new Slot[MaxBagSlots];
            _occupiedSlots = new Slot[MaxBagSlots];
        }

        void Start()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                Slot newSlot = new(i);
                newSlot.OnContentChanged += SlotContentChanged;
                _slots[i] = newSlot;
            }
        }

        void SlotContentChanged(object sender, Slot.ContentChangedArgs e)
        {
            OnSlotContentChanged?.Invoke(this, new()
            {
                Slot = (Slot) sender
            });
        }

        public Slot GetSlot(int index)
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

            foreach (Slot slot in _slots)
            {
                if (slot.IsEmpty)
                {
                    slot.PutItem(item);
                    return true;
                }
                
                if (slot.TryCombine(item, out Item excess))
                {
                    if (TryPutNearest(excess)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Tries to put the item in the given slot index.
        /// </summary>
        public bool TryPut(int slotIndex, Item item)
        {
            Slot slot = GetSlot(slotIndex);

            if (slot.IsEmpty)
            {
                slot.PutItem(item);
                return true;
            }            
            
            if (slot.TryCombine(item, out Item excess))
            {
                if (TryPutNearest(excess)) return true;
            }         
            return false;
        }

        /// <summary>
        /// Take entire stack.
        /// </summary>
        public Item Take(int slotIndex)
        {
            Slot slot = GetSlot(slotIndex);
            if (slot.IsEmpty) return Item.None;

            return slot.TakeItems(-1);
        }

        /// <summary>
        /// Take some amount of Item from Slot.
        /// </summary>
        public Item TakeItems(int slotIndex, int amount)
        {           
            Slot slot = GetSlot(slotIndex);
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

        public void ClearItem(Slot slot)
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
