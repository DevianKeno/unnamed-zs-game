using UnityEngine;

namespace UZSG.Inventory
{
    public class Bag : Container
    {
        public override int SlotsCount { get; set; }
        [SerializeField] ItemSlot[] _slots;
        bool _isFull;
        public bool IsFull => _isFull;
        public override ItemSlot[] Slots => _slots;

        public ItemSlot this[int i]
        {
            get
            {
                if (i < 0 || i > SlotsCount) return null;
                return _slots[i];
            }
        }
        
        ~Bag()
        {
            foreach (var slot in Slots)
            {
                slot.OnContentChanged -= SlotContentChanged;
            }
        }

        internal void Init()
        {
            _slots = new ItemSlot[SlotsCount];

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