using System;
using UnityEngine;
using UZSG.Data;
using UZSG.Items;

namespace UZSG.Inventory
{
    public enum HotbarIndex {
        Hands, Mainhand, Offhand, Three, Four, Five, Six, Seven, Eight, Nine, //Ten,
    }

    public struct SlotContentChangedArgs
    {
        /// <summary>
        /// The Slot that has changed.
        /// </summary>
        public ItemSlot Slot { get; set; }
    }

    public class InventoryHandler : MonoBehaviour
    {
        Hotbar _hotbar;
        public Hotbar Hotbar => _hotbar;
        Bag _bag;
        public Bag Bag => _bag;

        public ItemSlot Equipped => Hotbar.Equipped;
        public ItemSlot Mainhand => Hotbar.Mainhand;
        public ItemSlot Offhand => Hotbar.Offhand;

        public bool IsFull
        {
            get
            {
                foreach (var slot in _bag.Slots)
                {
                    if (slot.IsEmpty) return false;
                    continue;
                }
                return true;
            }
        }

        [SerializeField] HotbarUIHandler hotbarUI;

        void Awake()
        {
            _bag = GetComponent<Bag>();
            _hotbar = GetComponent<Hotbar>();
        }

        public static int SlotToIndex(HotbarIndex value)
        {
            return value switch
            {
                HotbarIndex.Mainhand => 1,
                HotbarIndex.Offhand => 2,
                HotbarIndex.Three => 3,
                HotbarIndex.Four => 4,
                HotbarIndex.Five => 5,
                HotbarIndex.Six => 6,
                HotbarIndex.Seven => 7,
                HotbarIndex.Eight => 8,
                HotbarIndex.Nine => 9,
                // HotbarIndex.Ten => 0,
                _ => -1,
            };
        }

        public static HotbarIndex IndexToSlot(int value)
        {
            return value switch
            {
                1 => HotbarIndex.Mainhand,
                2 => HotbarIndex.Offhand,
                3 => HotbarIndex.Three,
                4 => HotbarIndex.Four,
                5 => HotbarIndex.Five,
                6 => HotbarIndex.Six,
                7 => HotbarIndex.Seven,
                8 => HotbarIndex.Eight,
                9 => HotbarIndex.Nine,
                // 0 => HotbarIndex.Hands,
                _ => throw new ArgumentException("Invalid value"),
            };
        }
        
        public void Initialize()
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

        /// <summary>
        /// Tries to put a Weapon item in either the Mainhand or Offhand.
        /// </summary>
        public bool TryPutWeapon(Item item, out HotbarIndex putOnIndex)
        {
            if (Mainhand.TryPutItem(item))
            {
                putOnIndex = HotbarIndex.Mainhand;
                return true;
            }

            if (Offhand.TryPutItem(item))
            {
                putOnIndex = HotbarIndex.Offhand;
                return true;
            }

            putOnIndex = default;
            return false;
        }
    }
}
