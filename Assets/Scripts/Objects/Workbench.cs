using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

using UZSG.Crafting;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Interactions;
using UZSG.Inventory;
using UZSG.Items;
using UZSG.Saves;
using UZSG.UI;
using static UZSG.Crafting.CraftingRoutineStatus;

namespace UZSG.Objects
{
    public class Workbench : CraftingStation, ISaveDataReadWrite<WorkbenchSaveData>
    {
        protected override void OnPlaceEvent()
        {
            base.OnPlaceEvent();
            Addressables.LoadAssetAsync<GameObject>(WorkstationData.GUIAsset);
            Crafter.OnRoutineNotify += OnCraftingRoutineNotify;
            Crafter.OnRoutineUpdate += OnCraftingRoutineUpdate;
            OutputContainer.OnSlotItemChanged += OnOutputSlotItemChanged;

            AllowInteractions = true;
        }

        protected virtual void Use(Player player)
        {
            if (!WorkstationData.GUIAsset.IsSet())
            {
                Game.Console.LogWarn($"Workstation '{WorkstationData.Id}' has no GUI asset set.");
                return;
            }

            this.Player = player;
            Game.UI.CreateFromAddressableAsync<CraftingGUI>(WorkstationData.GUIAsset, callback: (element) =>
            {
                GUI = element;
                GUI.ReadWorkstation(this);
                player.UseObjectGUI(GUI);
                
                player.InfoHUD.Hide();
                player.Actions.Disable();
                player.Controls.Disable();
                player.FPP.ToggleControls(false);

                AddInputContainer(player.Inventory.Bag);

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

        public override void HitBy(HitboxCollisionInfo info)
        {
            base.HitBy(info);

            Game.Audio.PlayInWorld("bullet_hit_wood", info.ContactPoint);
        }


        #region Public methods

        public override bool TryCraft(ref CraftItemOptions options)
        {
            return base.TryCraft(ref options);
        }

        public void ReadSaveData(WorkbenchSaveData saveData)
        {
            throw new NotImplementedException();
        }

        public new WorkbenchSaveData WriteSaveData()
        {
            throw new NotImplementedException();
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


        #region Event callbacks

        protected virtual void OnCraftingRoutineNotify(CraftingRoutine routine)
        {
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
                    break;
                }
                
                case Canceled:
                {
                    OnCraftEvent(routine);
                    break;
                }
            }
        }

        protected virtual void OnCraftingRoutineUpdate(CraftingRoutine routine)
        {
            
        }

        protected void OnRoutineSecond(CraftingRoutine routine, float timeElapsed)
        {
            
        }

        protected void OnCloseInventory()
        {
            if (Player is Player player)
            {
                player.InventoryWindow.OnClosed -= OnCloseInventory;
                player.RemoveObjectGUI(GUI);
                player.InventoryWindow.Hide();
                
                ClearInputContainers();
                Game.UI.SetCursorVisible(false);
                /// encapsulate
                player.InfoHUD.Show();
                player.Actions.Enable();
                player.Controls.Enable();
                player.FPP.ToggleControls(true);
                player = null;

                GUI.Destruct();
            }
        }

        #endregion
    }
}