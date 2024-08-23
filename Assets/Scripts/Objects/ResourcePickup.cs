using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
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
                player.Actions.StartPickupRoutine(ResourceData.PickupDuration, onTimerNotify: (status) =>
                {
                    if (status == Players.PlayerActions.PickupStatus.Finished)
                    {
                        if (player.Inventory.Bag.TryPutNearest(new Item(Item)))
                        {
                            Game.Audio.PlayInWorld("pick", Position);
                            Destroy(gameObject);
                        }
                        else
                        {
                            Game.Entity.Spawn<ItemEntity>("item_entity", Position, callback: (info) =>
                            {
                                info.Entity.Item = Item;
                            });
                        }
                    }
                });
            }
        }
    }
}