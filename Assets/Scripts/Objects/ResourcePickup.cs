using System;

using UZSG.Data;
using UZSG.Entities;
using UZSG.Interactions;
using UZSG.Items;
using UZSG.Systems;

namespace UZSG.Objects
{
    public class ResourcePickup : BaseObject, IInteractable
    {
        public ResourceData ResourceData => objectData as ResourceData;
        public Item Item;
        public string Action => "Pick up";
        public string Name => ResourceData.Name;

        public event EventHandler<InteractArgs> OnInteract;

        protected override void Start()
        {
            base.Start();
        }

        public void Interact(IInteractActor actor, InteractArgs args)
        {
            if (actor is Player player)
            {
                if (player.Inventory.Bag.TryPutNearest(new Item(Item)))
                {
                    Game.Audio.Play("pick", transform.position);
                    Destroy(gameObject);
                }
            }
        }
    }
}