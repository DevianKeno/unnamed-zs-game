using System;
using System.Collections.Generic;
using UnityEngine;
using UZSG.Items;
using UZSG.Items.Weapons;

namespace UZSG.Inventory
{
    public class Equipment : Container
    {
        public struct ChangeEquippedArgs
        {
            public int Index { get; set; }
            public ItemSlot ItemSlot { get; set; }
        }

        public Equipment() : base(0)
        {
            Initialize();
        }
        
        public const int HandsIndex = 0;
        public const int MainhandIndex = 1;
        public const int OffhandIndex = 2;
        
        ItemSlot _hands;
        public ItemSlot Hand => _hands;
        ItemSlot _mainhand;
        public ItemSlot Mainhand => _mainhand;
        ItemSlot _offhand;
        public ItemSlot Offhand => _offhand;

        public void Initialize()
        {
            _hands = new(HandsIndex, ItemSlotType.Weapon);
            _hands.OnContentChanged += SlotContentChanged;
            _hands.Put(new("arms"));

            _mainhand = new(MainhandIndex, ItemSlotType.Weapon);
            _mainhand.OnContentChanged += SlotContentChanged;

            _offhand = new(OffhandIndex, ItemSlotType.Weapon);
            _offhand.OnContentChanged += SlotContentChanged;

            _slots = new()
            {
                _hands,
                _mainhand,
                _offhand,
            };
        }
        
        /// <summary>
        /// Tries to put a Weapon item in either the Mainhand or Offhand.
        /// </summary>
        public bool TryEquipWeapon(Item item, out EquipmentIndex putOnIndex)
        {
            if (_mainhand.TryPut(item))
            {
                putOnIndex = EquipmentIndex.Mainhand;
                return true;
            }

            if (_offhand.TryPut(item))
            {
                putOnIndex = EquipmentIndex.Offhand;
                return true;
            }

            putOnIndex = default;
            return false;
        }
    }
}