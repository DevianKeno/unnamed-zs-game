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

        internal void Initialize()
        {
            for (int i = 0; i < SlotsCount; i++)
            {
                ItemSlot newSlot = new(i, ItemSlotType.All);
                newSlot.OnContentChanged += SlotContentChanged;
                _slots.Add(newSlot);
            }
        }
    }
}