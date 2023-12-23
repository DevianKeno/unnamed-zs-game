using UZSG.Player;
using UZSG.Interactions;
using UZSG.Items;
using System;

namespace UZSG.Entities
{
    /// <summary>
    /// Items that appear in the world (e.g. Interactables, Pickupables, etc.)
    /// </summary>
    public class ItemEntity : Entity, IInteractable
    {
        public ItemData ItemData;
        public int ItemCount;
        public string Name => ItemData.Name;
        public string Action
        {
            get
            {
                if (ItemData.Type == ItemType.Item ||
                    ItemData.Type == ItemType.Tool || 
                    ItemData.Type == ItemType.Equipment ||
                    ItemData.Type == ItemType.Accessory) return "Pick Up";
                if (ItemData.Type == ItemType.Weapon) return "Equip";
                return "Interact with";
            }
        }

        public event EventHandler<InteractArgs> OnInteract;

        /// <summary>
        /// Gets an Item object from the entity.
        /// </summary>
        public Item AsItem()
        {
            return new Item(ItemData, ItemCount);
        }

        /// <summary>
        /// Gets a Weapon object from the entity.
        /// </summary>
        // public Weapon AsWeapon()
        // {
        //     // return new Weapon();
        // }

        public void Interact(PlayerActions actor, InteractArgs args)
        {
            actor.PickUpItem(this);
        }
    }
}