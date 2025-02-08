using System;
using System.Collections.Generic;

using UnityEngine.UIElements;

using UZSG.Data;
using UZSG.Entities;
using UZSG.Interactions;
using UZSG.Items;
using UZSG.Players;

namespace UZSG.Objects
{
    public class ResourcePickup : BaseObject, IInteractable
    {
        public ResourceData ResourceData => objectData as ResourceData;
        public string ActionText => "Pick Up";
        public string DisplayName => ResourceData.DisplayName;
        public bool AllowInteractions { get; set; } = true;

        protected override void Start()
        {
            base.Start();
        }

        public List<InteractAction> GetInteractActions()
        {
            var actions = new List<InteractAction>();

            actions.Add(new()
            {
                Interactable = this,
                ActionText = this.ActionText,
                InteractableText = this.DisplayName,
                InputAction = Game.Input.InteractPrimary,
            });

            return actions;
        }

        public void Interact(InteractionContext context)
        {
            if (context.Phase == InteractPhase.Started && context.Actor is Player player)
            {
                if (player.Inventory.Bag.IsFull)
                {
                    /// Maybe can still pick up but immediately drops the item
                    return;
                }

                player.Actions.StartPickupRoutine(this, allowMovement: true);
                player.Actions.OnInteract -= OnPlayerInteract;
                player.Actions.OnInteract += OnPlayerInteract;
            }
        }

        void OnPlayerInteract(InteractionContext context)
        {
            if (context.Phase == InteractPhase.Finished)
            {
                if (context.Actor is Player player)
                {
                    player.Actions.OnInteract -= OnPlayerInteract;

                    if (player.Actions.PickUpItem(new Item(ResourceData.Yield)))
                    {
                        Game.Audio.PlayInWorld("pick", Position);
                        Destruct();
                    }
                    else /// incapable of picking up, drop on ground
                    {
                        Game.Entity.Spawn<ItemEntity>("item_entity", Position, onCompleted: (info) =>
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