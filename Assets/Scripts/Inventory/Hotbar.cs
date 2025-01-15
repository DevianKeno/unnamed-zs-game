using System;
using System.Collections.Generic;
using UnityEngine;

using UZSG.Items;
using UZSG.Items.Weapons;

namespace UZSG.Inventory
{
    public class Hotbar : Container
    {
        public Hotbar(int slotCount) : base(slotCount)
        {
        }
        
        /// <summary>
        /// Tries to put a Weapon item in any of the Hotbar slots.
        /// </summary>
        /// <param name="putOnIndex">Where the item has been put.</param>
        public bool TryEquip(Item item, out int putOnIndex)
        {
            putOnIndex = -1;
            if (this.IsFull) return false;

            foreach (ItemSlot slot in this.Slots)
            {
                putOnIndex = slot.Index;
                if (slot.IsEmpty)
                {
                    slot.Put(item);
                    return true;
                }
                else if (slot.Item.CanStackWith(item))
                {
                    slot.Item.Stack(item);
                    return true;
                }

                continue;
            }
            
            return false;
        }
    }
}