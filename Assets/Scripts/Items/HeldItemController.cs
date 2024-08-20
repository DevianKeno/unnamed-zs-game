using UnityEngine;
using UnityEngine.InputSystem;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Players;
using UZSG.Attributes;
using UZSG.Saves;
using System;

namespace UZSG.Items
{
    /// <summary>
    /// Controller for "Held Items".
    /// </summary>
    public abstract class HeldItemController : MonoBehaviour, IAttributable, ISaveDataReadWrite<ItemSaveData>
    {
        protected ItemData itemData;
        public ItemData ItemData
        {
            get { return itemData; }
            set { itemData = value; }
        }
        protected Entity owner;
        public Entity Owner
        {
            get { return owner; }
            set { owner = value; }
        }
        [SerializeField] protected AudioSourceController audioSourceController;
        public AudioSourceController AudioSource => audioSourceController;
        [SerializeField] protected AttributeCollection attributes;
        public AttributeCollection Attributes => attributes;

        protected InputActionMap actionMap;

        public abstract void Initialize();
        public abstract void SetStateFromAction(ActionStates state);

        /// Save and Write for this "Held Item" as an "Item".
        public virtual void ReadSaveData(ItemSaveData saveData)
        {
            attributes.ReadSaveData(saveData.Attributes);
        }

        public virtual ItemSaveData WriteSaveData()
        {
            var saveData = new ItemSaveData()
            {
                Id = itemData.Id,
                Count = 1, /// useless for Held Item
                Attributes = attributes.WriteSaveData(),
            };

            return saveData;
        }

        protected Item AsItem()
        {
            var item = new Item(ItemData.Id) /// 1 count of course
            {
                Attributes = attributes
            };
            
            return item;
        }
    }
}
