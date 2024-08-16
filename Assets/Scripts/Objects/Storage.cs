using System;
using System.Linq;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.InputSystem;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.Interactions;
using UZSG.UI;
using UZSG.Crafting;
using UZSG.Items;
using UZSG.Inventory;
using UZSG.Data;

namespace UZSG.Objects
{
    public class Storage : BaseObject, IInteractable, IPlaceable
    {
        public string ActionText => "Open";
        public string Name => objectData.Name;
        
        Player player;
        Window gui;
        public Window GUI => gui;
        
        public event EventHandler<InteractArgs> OnInteract;

        InputAction backAction;
        
        protected override void Start()
        {
            base.Start();

            /// TESTING ONLY
            /// Place() should execute when the object is placed on the world :)
            Place(); 
        }

        void Place()
        {
            // LoadGUIAsset(WorkstationData.GUI, onLoadCompleted: (gui) =>
            // {
            //     this.gui = gui;
            //     this.gui.LinkWorkstation(this);
            // });
        }

        public virtual void Interact(IInteractActor actor, InteractArgs args)
        {
            if (actor is not Player player) return;

            this.player = player;
            
            player.InfoHUD.Hide();
            player.Actions.Disable();
            player.Controls.Disable();
            player.FPP.ToggleControls(false);

            // InitializeCrafter();
            // ReinitializeGUI();
            // InitializeEvents();

            // player.UseWorkstation(this);
            // player.InventoryGUI.OnClose += OnCloseInventory;
            gui.Show();
            Game.UI.ToggleCursor(true);
        }
    }
}