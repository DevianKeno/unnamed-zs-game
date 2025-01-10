using System;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.Interactions;

using UZSG.Data;
using UZSG.UI.Objects;
using UZSG.Saves;

namespace UZSG.Objects
{
    public class Storage : BaseObject, IInteractable, IPlaceable, IPickupable, ISaveDataReadWrite<ObjectSaveData>
    {
        public StorageData StorageData => objectData as StorageData;
        public string Action => "Open";
        public string Name => objectData.Name;
        public bool AllowInteractions { get; set; } = true;
        
        Player player;
        Container container = new();
        public Container Container => container;
        StorageGUI gui;
        public StorageGUI GUI => gui;
        
        public event EventHandler<IInteractArgs> OnInteract;
        
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
        
        public virtual void Interact(IInteractActor actor, IInteractArgs args)
        {
            if (actor is not Player player) return;

            this.player = player;
            
            player.InfoHUD.Hide();
            player.Actions.Disable();
            player.Controls.Disable();
            player.FPP.ToggleControls(false);
            
            animator.CrossFade("open", 0.5f);
            gui.SetPlayer(player);

            player.UseObjectGUI(gui);
            player.InventoryGUI.OnClosed += OnCloseInventory;
            player.InventoryGUI.Show();

            // gui.Show();
            Game.UI.SetCursorVisible(true);
        }

        void OnCloseInventory()
        {
            player.InventoryGUI.OnClosed -= OnCloseInventory;
            player.RemoveObjectGUI(gui);
            player.InventoryGUI.Hide();

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