using System;
using UnityEditor;
using UnityEngine;
using URMG.Items;
using URMG.UI;

namespace URMG.Inventory
{
    public enum ContainerType { Hotbar, Bag, Container }
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
        public Hotbar Hotbar { get => _hotbar; }
        Bag _bag;
        public Bag Bag { get => _bag; }

        void Awake()
        {
            _hotbar = GetComponent<Hotbar>();
            _bag = GetComponent<Bag>();
        }
    }
}
