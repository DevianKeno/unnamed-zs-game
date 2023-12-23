using UnityEngine;

namespace UZSG.Inventory
{
    public class Hotbar : Container
    {
        public static int MainhandIndex = 1;
        public static int OffhandIndex = 2;
        public static int[] SlotsIndex = { 3, 4, 5, 6, 7, 8, 9, 0 };
        [SerializeField] ItemSlot _mainhand;
        public ItemSlot Mainhand { get => _mainhand; }
        [SerializeField] ItemSlot _offhand;
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

        void Awake() 
        {
            _slots = new ItemSlot[11];
            _mainhand = new (MainhandIndex, SlotType.Weapon);
            _mainhand.OnContentChanged += SlotContentChanged;
            _offhand = new (OffhandIndex, SlotType.Tool);
            _offhand.OnContentChanged += SlotContentChanged;
            _slots[1] = _mainhand;
            _slots[2] = _offhand;
            for (int i = 3; i < 11; i++)
            {
                ItemSlot newSlot = new(i)
                {
                    Type = SlotType.Tool,
                };
                newSlot.OnContentChanged += SlotContentChanged;
                _slots[i] = newSlot;
            }
            _slots[0] = _slots[10];
        }
    }
}