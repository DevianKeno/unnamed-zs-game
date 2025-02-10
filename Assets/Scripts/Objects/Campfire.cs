using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;

using UZSG.Crafting;
using UZSG.Entities;
using UZSG.Interactions;
using UZSG.Inventory;
using UZSG.Items;
using UZSG.UI;
using static UZSG.Crafting.CraftingRoutineStatus;

namespace UZSG.Objects
{
    public class Campfire : FueledWorkstation
    {
        [SerializeField] ParticleSystem flameParticles;
        [SerializeField] Light spotLight;

        protected override void OnPlaceEvent()
        {
            base.OnPlaceEvent();
            Addressables.LoadAssetAsync<GameObject>(WorkstationData.GUIAsset);
            OutputContainer.OnSlotItemChanged += OnOutputSlotItemChanged;
            AllowInteractions = true;
        }

        void Use(Player player)
        {
            if (!WorkstationData.GUIAsset.IsSet())
            {
                Game.Console.LogWarn($"Workstation '{WorkstationData.Id}' has no GUI asset set.");
                return;
            }

            this.Player = player;
            Game.UI.CreateFromAddressableAsync<FueledCraftingGUI>(WorkstationData.GUIAsset, callback: (element) =>
            {
                GUI = element;
                FueledCraftingGUI.ReadWorkstation(this);

                player.InfoHUD.Hide();
                player.Actions.Disable();
                player.Controls.Disable();
                player.FPP.ToggleControls(false);

                AddInputContainer(player.Inventory.Bag);

                player.UseObjectGUI(GUI);
                player.InventoryWindow.OnClosed += OnCloseInventory;
                player.InventoryWindow.Show();
                
                Game.UI.SetCursorVisible(true);
            });
        }

        protected override void OnDestructEvent()
        {
            if (Player != null)
            {
                Player.InventoryWindow.OnClosed -= OnCloseInventory;
                OnCloseInventory();
            }
        }


        #region Public methods

        public override bool TryCraft(ref CraftItemOptions options)
        {
            return false;
        }

        public override bool TryFueledCraft(ref CraftItemOptions options)
        {
            return base.TryFueledCraft(ref options);
        }

        public void SetParticlesVisible(bool visible)
        {
            if (visible)
            {
                flameParticles.gameObject.SetActive(true);
                spotLight.gameObject.SetActive(true);
            }
            else
            {
                flameParticles.gameObject.SetActive(false);
                spotLight.gameObject.SetActive(false);
            }
        }

        #region IInteractable

        public override List<InteractAction> GetInteractActions()
        {
            var actions = new List<InteractAction>();

            var use = new InteractAction()
            {
                Interactable = this,
                Type = InteractType.Use,
                InputAction = Game.Input.InteractPrimary,
            };
            actions.Add(use);

            if (this.ObjectData.CanBePickedUp)
            {
                actions.Add(new()
                {
                    Interactable = this,
                    Type = InteractType.PickUp,
                    InputAction = Game.Input.InteractSecondary,
                    IsHold = true,
                    HoldDurationSeconds = this.ObjectData.PickupTimeSeconds,
                });
            }

            return actions;
        }

        public override void Interact(InteractionContext context)
        {
            if (context.Actor is not Player player) return;

            switch (context.Type)
            {
                case InteractType.Use:
                {
                    this.Use(player);
                    break;
                }

                case InteractType.PickUp:
                {
                    this.Pickup(player);
                    break;
                }
            }
        }

        #endregion
        #endregion


        #region Overrides

        protected override void OnCraftingRoutineNotify(CraftingRoutine routine)
        {
            base.OnCraftingRoutineNotify(routine);

            switch (routine.Status)
            {
                case Prepared:
                {
                    OnCraftEvent(routine);
                    break;
                }

                case Started:
                {
                    OnCraftEvent(routine);
                    SetParticlesVisible(true);
                    break;
                }
                
                case Ongoing:
                {
                    OnCraftEvent(routine);
                    break;
                }
                
                case Paused:
                {
                    OnCraftEvent(routine);
                    SetParticlesVisible(false);
                    break;
                }
                
                case CraftedSingle:
                {
                    OnCraftSingle(routine);
                    break;
                }
                
                case Completed:
                {
                    routine.Finish();
                    OnCraftEvent(routine);

                    if (!IsFueled)
                    {
                        SetParticlesVisible(false);
                    }
                    break;
                }
                
                case Canceled:
                {
                    OnCraftEvent(routine);
                    break;
                }
            }
        }

        protected override void OnFuelSlotChanged(object sender, ItemSlot.ItemChangedContext e)
        {
            base.OnFuelSlotChanged(sender, e);

            if (e.NewItem.IsNone || !e.NewItem.Data.IsFuel) return;
            if (WorkstationData.UsesFuelWhenIdle)
            {
                SetParticlesVisible(true);
            }
        }

        protected override void OnFuelDepletedEvent()
        {
            base.OnFuelDepletedEvent();
            SetParticlesVisible(false);
        }

        protected virtual void OnOutputSlotItemChanged(object sender, ItemSlot.ItemChangedContext e)
        {
            onOutputSlotItemChanged?.Invoke(e);
        }
        /// <summary>
        /// Listens to all output slots when their Item is changed.
        /// </summary>
        protected event Action<ItemSlot.ItemChangedContext> onOutputSlotItemChanged;
        void OnCraftSingle(CraftingRoutine routine)
        {
            var outputItem = new Item(routine.RecipeData.Output);
            
            if (base.OutputContainer.TryPutNearest(outputItem))
            {
                CraftingUtils.PlayCraftSound(this);
                OnCraftEvent(routine);
                return;
            }

            /// output slot is full wtf?? what do lmao
            onOutputSlotItemChanged += PutItemWhenOutputSlotIsEmpty;
            void PutItemWhenOutputSlotIsEmpty(ItemSlot.ItemChangedContext slotInfo)
            {
                /// look for empty space
                if (!slotInfo.NewItem.Is(Item.None)) return;
                
                onOutputSlotItemChanged -= PutItemWhenOutputSlotIsEmpty;
                slotInfo.ItemSlot.Put(outputItem);
                CraftingUtils.PlayCraftSound(this);
                OnCraftEvent(routine);
            };
        }

        #endregion

        protected void OnCloseInventory()
        {
            if (Player is Player player)
            {
                player.InventoryWindow.OnClosed -= OnCloseInventory;
                player.RemoveObjectGUI(GUI);
                player.InventoryWindow.Hide();
                
                ClearInputContainers();
                Game.UI.SetCursorVisible(false);
                /// TODO: encapsulate
                player.InfoHUD.Show();
                player.Actions.Enable();
                player.Controls.Enable();
                player.FPP.ToggleControls(true);
                player = null;

                GUI.Destruct();
            }
        }
    }
}