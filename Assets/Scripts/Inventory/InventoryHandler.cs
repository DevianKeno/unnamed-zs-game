using System;
using UnityEngine;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Items;
using UZSG.Systems;
using UZSG.UI;

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

        Container _bag;
        public Container Bag => _bag;
        Container _hotbar;
        public Container Hotbar => _hotbar;
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

        public void Initialize()
        {
            var bagSlotCount = Mathf.FloorToInt(Player.Generic.Get("bag_slots_count").Value);
            _bag = new(bagSlotCount);

            var hotbarSlotCount = Mathf.FloorToInt(Player.Generic.Get("hotbar_slots_count").Value);
            _hotbar = new(hotbarSlotCount);
            
            /// special treatment
            _equipment = new();
        }

        void InitializeGUI()
        {

        }

        public void ReadSaveJSON(InventorySaveData data)
        {
        }

        public void SelectHotbarSlot(int index)
        {            
            if (index < 0 || index > Hotbar.SlotCount) return;
            
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
                var position = Player.EyeLevel + Player.Forward;
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


        #region Debugging

        public string FindItemId; /// debugging
        
        [ContextMenu("Find Item Slots")]
        void FindThis()
        {
            foreach (var slot in Bag.FindItem(FindItemId))
            {
                print(slot.Index);
            }
        }

        #endregion
    }
}
