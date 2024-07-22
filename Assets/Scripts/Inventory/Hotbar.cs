using System;
using System.Collections.Generic;
using UnityEngine;

namespace UZSG.Inventory
{
    public class Hotbar : Container
    {
        public struct ChangeEquippedArgs
        {
            public int Index { get; set; }
            public ItemSlot ItemSlot { get; set; }
        }

        public const int MainhandIndex = 1;
        public const int OffhandIndex = 2;

        public override int SlotsCount { get; set; }
        [SerializeField] ItemSlot _mainhand;
        public ItemSlot Mainhand { get => _mainhand; }
        [SerializeField] ItemSlot _offhand;
        public ItemSlot Offhand { get => _offhand; }
        [SerializeField] List<ItemSlot> _slots = new();
        public override List<ItemSlot> Slots => _slots;
        ItemSlot _equipped;
        public ItemSlot Equipped { get => _equipped; }
        public int _equippedIndex = -1;

        public event EventHandler<ChangeEquippedArgs> OnChangeEquipped;

        public ItemSlot this[int i]
        {
            get
            {
                if (i < 0 || i > SlotsCount) return null;
                if (i == MainhandIndex) return _mainhand;
                if (i == OffhandIndex) return _offhand;
                return _slots[i];
            }
        }
        
        internal void Initialize()
        {
            _mainhand = new(MainhandIndex, SlotType.Weapon);
            _mainhand.OnContentChanged += SlotContentChanged;

            _offhand = new(OffhandIndex, SlotType.Weapon);
            _offhand.OnContentChanged += SlotContentChanged;

            _slots = new()
            {
                new(0), /// Empty hand slot
                _mainhand,
                _offhand
            };

            for (int i = 0; i < SlotsCount; i++)
            {
                int index = 3 + i;
                ItemSlot newSlot = new(index, SlotType.All);
                newSlot.OnContentChanged += SlotContentChanged;
                _slots.Add(newSlot);
            }
        }

        public void SelectSlot(int index)
        {
            if (_equippedIndex == index)
            {
                _equippedIndex = -1;
                _equipped = null;
            }
            else
            {
                _equippedIndex = index;
                _equipped = _slots[index];
            }

            OnChangeEquipped?.Invoke(this, new()
            {
                Index = index,
                ItemSlot = _equipped
            });
        }
    }
}