using System;
using System.Collections.Generic;
using UnityEngine;
using UZSG.Items;
using UZSG.Items.Weapons;

namespace UZSG.Inventory
{
    public class Equipment : Container
    {
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
        ItemSlot head;
        public ItemSlot Head => head;
        ItemSlot body;
        public ItemSlot Body => body;
        ItemSlot hands;
        public ItemSlot Hands => hands;
        ItemSlot legs;
        public ItemSlot Legs => legs;
        ItemSlot feet;
        public ItemSlot Feet => feet;

        public void Initialize()
        {
            _hands = new(HandsIndex, ItemSlotType.Weapon);
            _hands.OnItemChangedInternal += SlotContentChangedInternal;
            _hands.Put(new("arms", 1));

            _mainhand = new(MainhandIndex, ItemSlotType.Weapon);
            _mainhand.OnItemChangedInternal += SlotContentChangedInternal;

            _offhand = new(OffhandIndex, ItemSlotType.Weapon);
            _offhand.OnItemChangedInternal += SlotContentChangedInternal;
            
            head = new(OffhandIndex, ItemSlotType.Equipment);
            head.OnItemChangedInternal += SlotContentChangedInternal;
            
            body = new(OffhandIndex, ItemSlotType.Equipment);
            body.OnItemChangedInternal += SlotContentChangedInternal;
            
            hands = new(OffhandIndex, ItemSlotType.Equipment);
            hands.OnItemChangedInternal += SlotContentChangedInternal;
            
            legs = new(OffhandIndex, ItemSlotType.Equipment);
            legs.OnItemChangedInternal += SlotContentChangedInternal;
            
            feet = new(OffhandIndex, ItemSlotType.Equipment);
            feet.OnItemChangedInternal += SlotContentChangedInternal;

            slots = new()
            {
                _hands,
                _mainhand,
                _offhand,
                head,
                body,
                hands,
                legs,
                feet,
            };
        }
        
        /// <summary>
        /// Tries to put a Weapon item in either the Mainhand or Offhand.
        /// </summary>
        /// <param name="putOnIndex">Where the item has been put.</param>
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