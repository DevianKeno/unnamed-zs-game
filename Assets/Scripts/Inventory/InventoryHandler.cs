using System;
using UnityEngine;
using UnityEngine.Serialization;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Items;
using UZSG.Saves;
using UZSG.Systems;
using UZSG.UI;

namespace UZSG.Inventory
{
    public enum HotbarIndex {
        Three, Four, Five, Six, Seven, Eight, Nine, //Ten,
    }

    public enum EquipmentIndex {
        UNDEFINED, Mainhand, Offhand
    }

    public class InventoryHandler : MonoBehaviour, ISaveDataReadWrite<InventorySaveData>
    {
        public Player Player { get; private set; }
        [Space]
        [SerializeField, FormerlySerializedAs("ThrowForce")] float throwForce;

        Container _bag;
        /// <summary>
        /// Container for all other item stuff.
        /// </summary>
        public Container Bag => _bag;
        Hotbar _hotbar;
        /// <summary>
        /// Container for tools and items.
        /// </summary>
        public Hotbar Hotbar => _hotbar;
        Equipment _equipment;
        /// <summary>
        /// Container for weapons and equipment.
        /// </summary>
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

        internal void Initialize(Player player)
        {
            this.Player = player;
        }

        public void ReadSaveData(InventorySaveData saveData)
        {
            var bagSlotCount = Mathf.FloorToInt(Player.Attributes.Get("bag_slots_count").Value);
            _bag = new(bagSlotCount);
            _bag.ReadSaveData(saveData.Bag);

            var hotbarSlotCount = Mathf.FloorToInt(Player.Attributes.Get("hotbar_slots_count").Value);
            _hotbar = new(hotbarSlotCount);
            _hotbar.ReadSaveData(saveData.Hotbar);
            
            /// special treatment
            _equipment = new();
            _equipment.ReadSaveData(saveData.Equipment);
        }

        public InventorySaveData WriteSaveData()
        {
            var saveData = new InventorySaveData()
            {
                Bag = Bag.WriteSaveData(),
                Hotbar = Hotbar.WriteSaveData(),
                Equipment = Equipment.WriteSaveData(),
            };

            return saveData;
        }


        #region Public methods

        /// <summary>
        /// Drops item on the ground.
        /// </summary>
        public void DropItem(Item item)
        {
            if (item.IsNone) return;
            
            var position = Player.EyeLevel + Player.Forward;
            Game.Entity.Spawn<ItemEntity>("item", position, onCompleted: (info) =>
            {
                info.Entity.Item = item;
                var throwDirection = Player.Forward + Vector3.up;
                info.Entity.Throw(throwDirection, throwForce);
            });
        }

        /// <summary>
        /// Drops item on the ground.
        /// </summary>
        public void DropItem(int slotIndex)
        {
            if (Bag.TryGetSlot(slotIndex, out var slot))
            {
                if (slot.IsEmpty) return;

                var position = Player.EyeLevel + Player.Forward;
                Game.Entity.Spawn<ItemEntity>("item", position, onCompleted: (info) =>
                {
                    info.Entity.Item = slot.TakeAll();
                    var throwForce = (Player.Forward + Vector3.up) * this.throwForce;
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
        
        #endregion
        

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
