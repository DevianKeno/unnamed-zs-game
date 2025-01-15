using System;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.Interactions;

using UZSG.Data;
using UZSG.UI.Objects;
using UZSG.Saves;
using System.Collections.Generic;

namespace UZSG.Objects
{
    public class Storage : BaseObject, IInteractable, IPlaceable, IPickupable, ISaveDataReadWrite<ObjectSaveData>
    {
        public StorageData StorageData => objectData as StorageData;
        public string ActionText => "Open";
        public string DisplayName => objectData.DisplayName;
        public bool AllowInteractions { get; set; } = true;
        
        Player player;
        Container container = new();
        public Container Container => container;
        StorageGUI gui;
        public StorageGUI GUI => gui;
                
        public override void Place()
        {
            if (IsPlaced) return;
            IsPlaced = true;
            
            container = new(StorageData.Size);
            
            LoadGUIAsset(StorageData.GUI, onLoadCompleted: (gui) =>
            {
                this.gui = (StorageGUI) gui;
                this.gui.LinkStorage(this);
            });
        }
        
        public List<InteractAction> GetInteractActions()
        {
            var actions = new List<InteractAction>();

            actions.Add(new()
            {
                Interactable = this,
                ActionText = "Open",
                InteractableText = this.objectData.DisplayName,
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