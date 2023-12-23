using System;
using UnityEngine;
using URMG.Interactions;
using URMG.Player;

namespace URMG.Items
{
    public class ItemEntity : MonoBehaviour, IInteractable
    {
        public ItemData ItemData;
        public int ItemCount;
        public string Name => ItemData.Name;
        public string Action => "Pick Up";
        public event EventHandler<InteractArgs> OnInteract;

        /// <summary>
        /// Gets an Item object from the entity.
        /// </summary>
        /// <returns></returns>
        public Item AsItem()
        {
            return new Item(ItemData, ItemCount);
        }

        public void Interact(PlayerActions actor, InteractArgs args)
        {
            actor.PickUpItem(this);
        }
    }
}