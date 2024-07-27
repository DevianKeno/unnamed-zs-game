using UnityEngine;
using UZSG.Inventory;

namespace UZSG
{
    public enum EquipmentStates {
        Idle, Move,
        Primary, PrimaryHold, PrimaryRelease,
        Secondary, SecondaryHold, SecondaryRelease,
        ADSDown, ADSUp,
        Reload, ReloadClip, Melee, Equip, Dequip,
    }
    
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
            headSlot = new(0, ItemSlotType.Equipment);
            chestSlot = new(1, ItemSlotType.Equipment);
            handSlot = new(2, ItemSlotType.Equipment);
            legsSlot = new(3, ItemSlotType.Equipment);
            feetSlot = new(4, ItemSlotType.Equipment);

            for (int i = 5; i < accessorySlots.Length; i++)
            {
                ItemSlot newSlot = new(i);
                newSlot.OnContentChanged += SlotContentChanged;
                newSlot.SlotType = ItemSlotType.Accessory;
                accessorySlots[i] = newSlot;
            }
        }

        void SlotContentChanged(object sender, ItemSlot.ContentChangedArgs e)
        {
            //
        }
    }
}