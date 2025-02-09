using System;
using System.Collections.Generic;

using UZSG.Entities;
using UZSG.Interactions;
using UZSG.Data;
using UZSG.UI.Objects;
using UZSG.Saves;
using UZSG.Items;

namespace UZSG.Objects
{
    public class Storage : BaseObject, IInteractable, IPlaceable, IPickupable, ISaveDataReadWrite<ObjectSaveData>
    {
        public StorageData StorageData => objectData as StorageData;
        public string ActionText => Game.Locale.Translatable("action.open");
        public string DisplayName => objectData.DisplayNameTranslatable;
        public bool AllowInteractions { get; set; } = true;
        public override bool CanBePickedUp
        {
            get
            {
                return container.HasAny && objectData.CanBePickedUp;
            }
        }

        Player player;
        Container container = new();
        public Container Container => container;
        StorageGUI gui;
        public StorageGUI GUI => gui;
                
        protected override void OnPlace()
        {
            container = new(StorageData.Size);
            
            LoadGUIAsset(StorageData.GUI, onLoadCompleted: (gui) =>
            {
                this.gui = (StorageGUI) gui;
                this.gui.LinkStorage(this);
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

        public List<InteractAction> GetInteractActions()
        {
            var actions = new List<InteractAction>();

            actions.Add(new()
            {
                Type = InteractType.Open,
                Interactable = this,
                InputAction = Game.Input.InteractPrimary,
            });

            return actions;
        }
        
        public virtual void Interact(InteractionContext context)
        {
            if (context.Actor is not Player player) return;

            this.player = player;
            
            player.InfoHUD.Hide();
            player.Actions.Disable();
            player.Controls.Disable();
            player.FPP.ToggleControls(false);
            
            animator.CrossFade("open", 0.5f);
            gui.SetPlayer(player);

            player.UseObjectGUI(gui);
            player.InventoryWindow.OnClosed += OnCloseInventory;
            player.InventoryWindow.Show();

            // gui.Show();
            Game.UI.SetCursorVisible(true);
        }

        void OnCloseInventory()
        {
            player.InventoryWindow.OnClosed -= OnCloseInventory;
            player.RemoveObjectGUI(gui);
            player.InventoryWindow.Hide();

            animator.CrossFade("close", 0.5f);
            Game.UI.SetCursorVisible(false);
            /// encapsulate
            player.InfoHUD.Show();
            player.Actions.Enable();
            player.Controls.Enable();
            player.FPP.ToggleControls(true);
            player = null;
        }

        public void ReadSaveData(StorageObjectSaveData saveData)
        {
            this.container = new(StorageData.Size);
            this.container.ReadSaveData(saveData.Slots);
        }

        public override ObjectSaveData WriteSaveData()
        {
            var sd = base.WriteSaveData();
            var saveData = new StorageObjectSaveData()
            {
                InstanceId = sd.InstanceId,
                Id = sd.Id,
                Transform = sd.Transform,
                Slots = container.WriteSaveData()
            };

            return saveData;
        }
    }
}