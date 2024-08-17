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
using UZSG.Data;

namespace UZSG.Objects
{
    public class Storage : BaseObject, IInteractable, IPlaceable
    {
        public StorageData StorageData => objectData as StorageData;
        public string ActionText => "Open";
        public string Name => objectData.Name;
        
        Player player;
        Container container = new();
        public Container Container => container;
        StorageGUI gui;
        public StorageGUI GUI => gui;
        
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
            LoadGUIAsset(StorageData.GUI, onLoadCompleted: (gui) =>
            {
                this.gui = gui;
                this.gui.LinkStorage(this);
            });
        }
        
        void ReinitializeGUI()
        {
            gui.SetPlayer(player);

            backAction = Game.Main.GetInputAction("Back", "Global");
            backAction.performed += OnInputGlobalBack;
        }


        public virtual void Interact(IInteractActor actor, InteractArgs args)
        {
            if (actor is not Player player) return;

            this.player = player;
            
            player.InfoHUD.Hide();
            player.Actions.Disable();
            player.Controls.Disable();
            player.FPP.ToggleControls(false);
            
            ReinitializeGUI();

            animator.CrossFade("open", 0.5f);

            player.InventoryGUI.Show();
            player.InventoryGUI.OnClose += OnCloseInventory;
            // gui.Show();
            Game.UI.ToggleCursor(true);
        }

        void OnCloseInventory()
        {
            player.InventoryGUI.OnClose -= OnCloseInventory;

            animator.CrossFade("close", 0.5f);
            
            backAction.performed -= OnInputGlobalBack;
            
            Game.UI.ToggleCursor(false);
            // gui.Hide();
            
            /// encapsulate
            player.InfoHUD.Show();
            player.Actions.Enable();
            player.Controls.Enable();
            player.FPP.ToggleControls(true);
            player = null;
        }

        void OnInputGlobalBack(InputAction.CallbackContext context)
        {
            OnCloseInventory();
        }
        
        protected virtual void LoadGUIAsset(AssetReference asset, Action<StorageGUI> onLoadCompleted = null)
        {
            if (!asset.IsSet())
            {
                Game.Console.LogAndUnityLog($"There's no GUI set for Workstation '{StorageData.Id}'. This won't be usable unless otherwise you set its GUI.");
                return;
            }

            Addressables.LoadAssetAsync<GameObject>(asset).Completed += (a) =>
            {
                if (a.Status == AsyncOperationStatus.Succeeded)
                {
                    var go = Instantiate(a.Result);
                    
                    if (go.TryGetComponent<StorageGUI>(out var gui))
                    {
                        onLoadCompleted?.Invoke(gui);
                        return;
                    }
                }
            };
        }
    }
}