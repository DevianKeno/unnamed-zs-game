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
                if (player.Inventory.Bag.IsFull)
                {
                    /// Maybe can still pick up but immediately drops the item
                    return;
                }

                player.Actions.StartPickupRoutine(this);
                player.Actions.OnInteract -= OnPlayerInteract;
                player.Actions.OnInteract += OnPlayerInteract;
            }
        }

        void OnPlayerInteract(InteractContext context)
        {
            if (context.Phase == InteractPhase.Finished)
            {
                if (context.Actor is Player player)
                {
                    player.Actions.OnInteract -= OnPlayerInteract;

                    if (player.Actions.PickUpItem(new Item(ResourceData.Yield)))
                    {
                        Game.Audio.PlayInWorld("pick", Position);
                        Destroy();
                    }
                    else /// incapable of picking up, drop on ground
                    {
                        Game.Entity.Spawn<ItemEntity>("item_entity", Position, callback: (info) =>
                        {
                            info.Entity.Item = ResourceData.Yield;
                        });
                    }
                }
            }
            else if (context.Phase == InteractPhase.Canceled)
            {
                if (context.Actor is Player player)
                {
                    player.Actions.OnInteract -= OnPlayerInteract;
                }
            }
        }
    }
}