using System;

using UZSG.Data;
using UZSG.Entities;
using UZSG.Interactions;
using UZSG.Items;

namespace UZSG.Objects
{
    public class ResourcePickup : BaseObject, IInteractable
    {
        public ResourceData ResourceData => objectData as ResourceData;
        public Item Item;
        public string ActionText => "Pick up";
        public string Name => ResourceData.Name;

        public event EventHandler<InteractArgs> OnInteract;

        public void Interact(IInteractActor actor, InteractArgs args)
        {
            if (actor is Player player)
            {
                if (player.Inventory.Bag.TryPutNearest(new Item(Item)))
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}