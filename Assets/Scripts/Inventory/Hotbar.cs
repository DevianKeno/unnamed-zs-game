using UnityEngine;

namespace URMG.Inventory
{
    public class Hotbar : Container
    {
        public static int MainhandIndex = 1;
        public static int OffhandIndex = 2;
        public static int[] SlotsIndex = { 3, 4, 5, 6, 7, 8, 9, 0 };
        [SerializeField] ItemSlot _mainhand = new (MainhandIndex, SlotType.Weapon);
        public ItemSlot Mainhand { get => _mainhand; }
        [SerializeField] ItemSlot _offhand = new (OffhandIndex, SlotType.Tool);
        public ItemSlot Offhand { get => _offhand; }
        [SerializeField] ItemSlot[] _slots;
        public override ItemSlot[] Slots => _slots;

        public ItemSlot this[int i]
        {
            get
            {
                if (i < 0 || i > 9) return null;
                if (i == 1) return _mainhand;
                if (i == 2) return _offhand;

                return _slots[i];
            }
        }

        void Start()
        {
            _slots = new ItemSlot[8];
            for (int i = 3; i < SlotsIndex.Length; i++)
            {
                ItemSlot newSlot = new(i, SlotType.Tool);
                newSlot.OnContentChanged += SlotContentChanged;
                _slots[i] = newSlot;
            }
        }
    }
}