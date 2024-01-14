using System.Collections.Generic;
using UnityEngine;
using UZSG.Data;

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

        void Awake()
        {
            _bag = GetComponent<Bag>();
            _hotbar = GetComponent<Hotbar>();
        }

        public void Init()
        {
            _bag.Init();
            _hotbar.Init();
        }

        public void LoadData(InventoryData data)
        {
        }

        public void SelectHotbarSlot(int index)
        {            
            if (index < 0 || index > 9) return; // Should be index > Hotbar.MaxSlots
            Hotbar.SelectSlot(index);
        }
    }
}
