using System;
using UnityEngine;
using UZSG.Inventory;

namespace UZSG
{
    public class EquipmentHandler : MonoBehaviour
    {
        public int MaxAccessorySlots = 5;
        ItemSlot headSlot;
        ItemSlot chestSlot;
        ItemSlot handSlot;
        ItemSlot legsSlot;
        ItemSlot feetSlot;
        ItemSlot[] accessorySlots;

        void Awake()
        {
            accessorySlots = new ItemSlot[MaxAccessorySlots];
        }

        void Start()
        {
            headSlot = new(0, SlotType.Equipment);
            chestSlot = new(1, SlotType.Equipment);
            handSlot = new(2, SlotType.Equipment);
            legsSlot = new(3, SlotType.Equipment);
            feetSlot = new(4, SlotType.Equipment);

            for (int i = 5; i < accessorySlots.Length; i++)
            {
                ItemSlot newSlot = new(i);
                newSlot.OnContentChanged += SlotContentChanged;
                newSlot.Type = SlotType.Accessory;
                accessorySlots[i] = newSlot;
            }
        }

        void SlotContentChanged(object sender, ItemSlot.ContentChangedArgs e)
        {
            //
        }
    }
}