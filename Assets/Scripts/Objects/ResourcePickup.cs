using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Interactions;
using UZSG.Items;
using UZSG.Players;
using UZSG.Systems;

using static UZSG.Players.PlayerActions.PickupEventStatus;

namespace UZSG.Objects
{
    public class ResourcePickup : BaseObject, IInteractable
    {
        public ResourceData ResourceData => objectData as ResourceData;
        public string Action => "Pick up";
        public string Name => ResourceData.Name;
        public bool AllowInteractions { get; set; } = true;
        public event EventHandler<IInteractArgs> OnInteract;

        Player _actor;

        protected override void Start()
        {
            base.Start();
        }

        public void Interact(IInteractActor actor, IInteractArgs args)
        {
            if (actor is Player player)
            {
                if (player.Inventory.Bag.IsFull) return;
                
                _actor = player;
                player.Actions.StartPickupRoutine(this, OnPickupEvent);
            }
        }

        void OnPickupEvent(PlayerActions.PickupEventStatus status)
        {
            if (status == Finished)
            {
                if (_actor.Actions.PickUpItem(new Item(ResourceData.Yield)))
                {
                    Game.Audio.PlayInWorld("pick", Position);
                    Destroy();
                }
                else
                {
                    Game.Entity.Spawn<ItemEntity>("item_entity", Position, callback: (info) =>
                    {
                        info.Entity.Item = ResourceData.Yield;
                    });
                }
            }
        }
    }
}