using System;
using System.Collections.Generic;
using UnityEngine;
using UZSG.Items.Weapons;

namespace UZSG.Inventory
{
    public class Hotbar : Container
    {
        public struct ChangeEquippedArgs
        {
            public int Index { get; set; }
            public ItemSlot ItemSlot { get; set; }
        }

        public const int HandsIndex = 0;
        public const int MainhandIndex = 1;
        public const int OffhandIndex = 2;

        public override int SlotsCount { get; set; }
        ItemSlot _hands;
        public ItemSlot Hands { get => _hands; }
        [SerializeField] ItemSlot _mainhand;
        public ItemSlot Mainhand { get => _mainhand; }
        [SerializeField] ItemSlot _offhand;
        public ItemSlot Offhand { get => _offhand; }
        [SerializeField] List<ItemSlot> _slots = new();
        public override List<ItemSlot> Slots => _slots;

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
            _hands = new(HandsIndex, SlotType.Weapon);
            // _hands.OnContentChanged += SlotContentChanged;

            _mainhand = new(MainhandIndex, SlotType.Weapon);
            _mainhand.OnContentChanged += SlotContentChanged;

            _offhand = new(OffhandIndex, SlotType.Weapon);
            _offhand.OnContentChanged += SlotContentChanged;

            _slots = new()
            {
                _hands,
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

            _hands.Put(new("arms"));
        }
    }
}