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

        /// Special slots aside from the Hotbar slots
        ItemSlot _hands;
        public ItemSlot Hand => _hands;
        [SerializeField] ItemSlot _mainhand;
        public ItemSlot Mainhand => _mainhand;
        [SerializeField] ItemSlot _offhand;
        public ItemSlot Offhand => _offhand;
        
        internal void Initialize()
        {
            _hands = new(HandsIndex, ItemSlotType.Weapon);
            // _hands.OnContentChanged += SlotContentChanged;

            _mainhand = new(MainhandIndex, ItemSlotType.Weapon);
            _mainhand.OnContentChanged += SlotContentChanged;

            _offhand = new(OffhandIndex, ItemSlotType.Weapon);
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
                ItemSlot newSlot = new(index, ItemSlotType.All);
                newSlot.OnContentChanged += SlotContentChanged;
                _slots.Add(newSlot);
            }

            _hands.Put(new("arms"));
        }
    }
}