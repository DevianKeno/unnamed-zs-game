using System.Collections.Generic;
using UnityEngine;

namespace UZSG.Inventory
{
    public class Bag : Container
    {
        internal void Initialize()
        {
            _slots = new();

            for (int i = 0; i < SlotsCount; i++)
            {
                ItemSlot newSlot = new(i, ItemSlotType.All);
                newSlot.OnContentChanged += SlotContentChanged;
                _slots.Add(newSlot);
            }   
        }
    }
}