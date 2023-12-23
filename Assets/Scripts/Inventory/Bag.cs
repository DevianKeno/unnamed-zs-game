using System;
using UnityEngine;

namespace UZSG.Inventory
{
    public class Bag : Container
    {
        public static int MaxBagSlots = 16;
        [SerializeField] ItemSlot[] _slots;
        bool _isFull;
        public bool IsFull => _isFull;
        public override ItemSlot[] Slots => _slots;

        public ItemSlot this[int i]
        {
            get
            {
                if (i < 0 || i > MaxBagSlots) return null;
                return _slots[i];
            }
        }

        void Awake()
        {
            _slots = new ItemSlot[MaxBagSlots];

            for (int i = 0; i < _slots.Length; i++)
            {
                ItemSlot newSlot = new(i)
                {
                    Type = SlotType.Item | SlotType.Equipment | SlotType.Accessory
                };
                newSlot.OnContentChanged += SlotContentChanged;
                _slots[i] = newSlot;
            }            
        }
    }
}