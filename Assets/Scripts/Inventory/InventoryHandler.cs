using System;
using UnityEngine;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Items;
using UZSG.Systems;

namespace UZSG.Inventory
{
    public enum HotbarIndex {
        Three, Four, Five, Six, Seven, Eight, Nine, //Ten,
    }

    public enum EquipmentIndex {
        Hands, Mainhand, Offhand
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

        Bag _bag;
        public Bag Bag => _bag;
        Hotbar _hotbar;
        public Hotbar Hotbar => _hotbar;
        Equipment _equipment;
        public Equipment Equipment => _equipment;

        public bool IsFull
        {
            get
            {
                foreach (var slot in _bag.Slots)
                {
                    if (slot.IsEmpty) return false;
                }
                return true;
            }
        }
        public bool HasFreeWeaponSlot
        {
            get
            {
                if (_equipment.Mainhand.IsEmpty || _equipment.Offhand.IsEmpty) return true;
                return false;
            }
        }

        void Awake()
        {
            _bag = GetComponent<Bag>();
            _hotbar = GetComponent<Hotbar>();
            _equipment = GetComponent<Equipment>();
        }
        
        public void Initialize()
        {
            _bag.Initialize();
            _hotbar.Initialize();
            _equipment.Initialize();
        }

        public void LoadData(InventoryData data)
        {
        }

        public void SelectHotbarSlot(int index)
        {            
            if (index < 0 || index > Hotbar.SlotsCount) return;
            
            /// update ui if any 
        }

        public bool TryPutHotbar(Item item, out HotbarIndex putOnIndex)
        {
            if (Hotbar.TryPutNearest(item, out var slot))
            {
                putOnIndex = (HotbarIndex) slot.Index;
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

        public ItemSlot GetEquipmentOrHotbarSlot(int index)
        {
            if (index >= 0 && index < 3)
            {
                return Equipment[index];
            }
            if (index >= 3)
            {
                return Hotbar[index - 3];
            }
            
            return null;
        }
    }
}
