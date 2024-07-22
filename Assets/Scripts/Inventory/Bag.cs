using System.Collections.Generic;
using UnityEngine;

namespace UZSG.Inventory
{
    public class Bag : Container
    {
        public override int SlotsCount { get; set; }
        [SerializeField] List<ItemSlot> _slots = new();
        bool _isFull;
        public bool IsFull => _isFull;
        public override List<ItemSlot> Slots => _slots;

        public ItemSlot this[int i]
        {
            get
            {
                if (i < 0 || i > SlotsCount) return null;
                return _slots[i];
            }
        }
        
        internal void Initialize()
        {
            _slots = new();

            for (int i = 0; i < SlotsCount; i++)
            {
                ItemSlot newSlot = new(i, SlotType.All);
                newSlot.OnContentChanged += SlotContentChanged;
                _slots.Add(newSlot);
            }   
        }
    }
}