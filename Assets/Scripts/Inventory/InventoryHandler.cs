using System.Collections.Generic;
using UnityEngine;

namespace UZSG.Inventory
{
    public struct SlotContentChangedArgs
    {
        ItemSlot _slot;
        /// <summary>
        /// The Slot that has changed.
        /// </summary>
        public readonly ItemSlot Slot => _slot;

        public SlotContentChangedArgs(ItemSlot slot)
        {
            _slot = slot;
        }
    }

    public class InventoryHandler : MonoBehaviour
    {
        Hotbar _hotbar;
        public Hotbar Hotbar => _hotbar;
        Bag _bag;
        public Bag Bag => _bag;
        public ItemSlot Equipped => Hotbar.Equipped;

        public void Initialize()
        {

        }

        void Awake()
        {
            _hotbar = GetComponent<Hotbar>();
            _bag = GetComponent<Bag>();
        }

        public void SelectHotbarSlot(int index)
        {            
            if (index < 0 || index > 9) return; // Should be index > Hotbar.MaxSlots
            Hotbar.SelectSlot(index);
        }
    }
}
