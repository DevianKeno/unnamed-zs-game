using System;
using UnityEngine;

namespace URMG.Inventory
{
    public class Bag : Container
    {
        public static int MaxBagSlots = 16;
        [SerializeField] ItemSlot[] _slots;

        public override ItemSlot[] Slots => _slots;

        public ItemSlot this[int i]
        {
            get
            {
                if (i < 0 || i > MaxBagSlots) return null;
                return _slots[i];
            }
        }

        void Start()
        {
            _slots = new ItemSlot[MaxBagSlots];

            for (int i = 0; i < _slots.Length; i++)
            {
                ItemSlot newSlot = new(i, SlotType.Item);
                newSlot.OnContentChanged += SlotContentChanged;
                _slots[i] = newSlot;
            }
        }
    }
}