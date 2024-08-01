using System;
using UnityEngine;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Items;
using UZSG.Systems;

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
        public Player Player;
        [Space]

        public float ThrowForce;

        Hotbar _hotbar;
        public Hotbar Hotbar => _hotbar;
        public ItemSlot Mainhand => Hotbar.Mainhand;
        public ItemSlot Offhand => Hotbar.Offhand;
        Bag _bag;
        public Bag Bag => _bag;
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
        public bool HasFreeWeaponSlot
        {
            get
            {
                if (Mainhand.IsEmpty || Offhand.IsEmpty) return true;
                return false;
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
                0 => HotbarIndex.Hands,
                _ => throw new ArgumentException("Invalid value"),
            };
        }
        
        public void Initialize()
        {
            _bag.Initialize();
            _hotbar.Initialize();
        }

        public void LoadData(InventoryData data)
        {
        }

        public void SelectHotbarSlot(int index)
        {            
            if (index < 0 || index > Hotbar.SlotsCount) return;
            
            /// update ui if any 
        }

        /// <summary>
        /// Tries to put a Weapon item in either the Mainhand or Offhand.
        /// </summary>
        public bool TryEquipWeapon(Item item, out HotbarIndex putOnIndex)
        {
            if (Mainhand.TryPut(item))
            {
                putOnIndex = HotbarIndex.Mainhand;
                return true;
            }

            if (Offhand.TryPut(item))
            {
                putOnIndex = HotbarIndex.Offhand;
                return true;
            }

            putOnIndex = default;
            return false;
        }

        /// <summary>
        /// Drops item on the ground.
        /// </summary>
        public void DropItem(int slotIndex)
        {
            if (Bag.TryGetSlot(slotIndex, out var slot))
            {
                var position = Player.Position + Vector3.forward;
                Game.Entity.Spawn<ItemEntity>("item", position, callback: (info) =>
                {
                    info.Entity.Item = slot.TakeAll();
                    var throwForce = (Player.Forward + Vector3.up) * ThrowForce;
                    info.Entity.Rigidbody.AddForce(throwForce, ForceMode.Impulse);
                });
            }
        }
    }
}
