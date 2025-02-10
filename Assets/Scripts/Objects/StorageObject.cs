using System;
using System.Collections.Generic;

using UZSG.Entities;
using UZSG.Interactions;
using UZSG.Data;
using UZSG.UI;
using UZSG.Saves;
using UZSG.Inventory;

namespace UZSG.Objects
{
    /// <summary>
    /// Represent objects that have item storage (e.g., chests, crates, etc.).
    /// </summary>
    public class StorageObject : BaseObject, IPlayerInteractable, ISaveDataReadWrite<StorageObjectSaveData>
    {
        public StorageData StorageData => objectData as StorageData;

        public Player Player { get; protected set;}
        public bool AllowInteractions { get; set; } = true;
        public override bool CanBePickedUp
        {
            get
            {
                return Container.HasAnyItem && objectData.CanBePickedUp;
            }
        }

        public Container Container { get; protected set; }   
        public StorageGUI GUI { get; protected set; }
        
        protected override void OnPlaceEvent()
        {
            Container = new Container(StorageData.Size);
            Container.OnSlotItemChanged += OnContainerItemChanged;
            
            AllowInteractions = true;
        }


        #region Public

        public List<InteractAction> GetInteractActions()
        {
            var actions = new List<InteractAction>();

            actions.Add(new()
            {
                Type = InteractType.Open,
                Interactable = this,
                InputAction = Game.Input.InteractPrimary,
            });

            if (!Container.HasAnyItem)
            {
                actions.Add(new()
                {
                    Type = InteractType.PickUp,
                    Interactable = this,
                    InputAction = Game.Input.InteractSecondary,
                    IsHold = true,
                    
                });
            }

            return actions;
        }
        
        public virtual void Interact(InteractionContext context)
        {
            if (context.Actor is not Player player) return;

            switch (context.Type)
            {
                case InteractType.Open:
                {
                    this.Open(player);
                    break;
                }

                case InteractType.PickUp:
                {
                    this.Pickup(player);
                    break;
                }
            }
        }
        
        protected virtual void Open(Player player)
        {
            if (!StorageData.GUIAsset.IsSet())
            {
                Game.Console.LogWarn($"StorageObject '{StorageData.Id}' has no GUI asset set.");
                return;
            }

            this.Player = player;
            Game.UI.CreateFromAddressableAsync<StorageGUI>(StorageData.GUIAsset, callback: (element) =>
            {
                GUI = element;
                GUI.ReadStorage(this);
                player.UseObjectGUI(GUI);
                
                player.InfoHUD.Hide();
                player.Actions.Disable();
                player.Controls.Disable();
                player.FPP.ToggleControls(false);

                player.InventoryWindow.OnClosed += OnCloseInventory;
                player.InventoryWindow.Show();

                Game.UI.SetCursorVisible(true);
            });
        }
        
        public override void Pickup(IInteractActor actor)
        {
            if (actor is Player player)
            {
                if (this.CanBePickedUp && player.Actions.PickUpItem(this.AsItem()))
                {
                    Destruct();
                }
            }
        }

        public virtual void ReadSaveData(StorageObjectSaveData saveData)
        {
            base.ReadSaveData(saveData);
            this.Container = new(StorageData.Size);
            this.Container.ReadSaveData(saveData.Slots);
        }

        public new StorageObjectSaveData WriteSaveData()
        {
            var objectSave = base.WriteSaveData();
            return new StorageObjectSaveData()
            {
                Id = objectSave.Id,
                Transform = objectSave.Transform,
                Slots = Container.WriteSaveData(),
            };
        }

        #endregion


        void OnContainerItemChanged(object sender, ItemSlot.ItemChangedContext e)
        {
            MarkDirty();
            OnContainerItemChangedEvent(e);
        }

        /// <summary>
        /// Raised once whenever an item in this container is changed.
        /// </summary>
        protected virtual void OnContainerItemChangedEvent(ItemSlot.ItemChangedContext context) { }
        
        protected virtual void OnCloseInventory()
        {
            Player.InventoryWindow.OnClosed -= OnCloseInventory;
            Player.RemoveObjectGUI(GUI);
            Player.InventoryWindow.Hide();

            Game.UI.SetCursorVisible(false);
            /// encapsulate
            Player.InfoHUD.Show();
            Player.Actions.Enable();
            Player.Controls.Enable();
            Player.FPP.ToggleControls(true);
            Player = null;
        }
    }
}