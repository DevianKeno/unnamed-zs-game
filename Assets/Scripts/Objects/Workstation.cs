using System;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.InputSystem;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.Interactions;
using UZSG.UI;
using UZSG.Crafting;
using UZSG.Data;

namespace UZSG.Objects
{
    public interface IPlaceable
    {
        public virtual void Place() { }
    }

    public interface ICrafter
    {
        public Crafter Crafter { get; }
    }

    public class Workstation : BaseObject, IInteractable, IPlaceable, ICrafter
    {
        public WorkstationData WorkstationData => objectData as WorkstationData;
        public string ActionText => "Use";
        public string Name => objectData.Name;

        Player player;
        CraftingGUI _GUI;
        public CraftingGUI GUI => _GUI;
        [SerializeField] Crafter crafter;
        public Crafter Crafter => crafter;

        public event EventHandler<InteractArgs> OnInteract;

        InputAction backAction;
        
        protected override void Start()
        {
            /// TESTING ONLY
            /// Place() should execute when the object is placed on the world :)
            Place(); 
            ///
        }

        void InitializeGUI(CraftingGUI gui)
        {
            _GUI = gui;
            _GUI.ResetDisplayed();
            _GUI.SetPlayer(player);
            _GUI.AddRecipes(WorkstationData.IncludedRecipes); /// by raw RecipeData
            /// Situational by workstation
            /// e.g., By Player recipes can also be crafted in Workbench (ofc if the Player knows it)
            /// but not in smelting, etc. as said, situational
            if (WorkstationData.IncludePlayerRecipes)
            {
                _GUI.AddRecipesById(player.SaveData.KnownRecipes); /// by raw RecipeData
            }
            _GUI.InitializeCrafter(crafter); /// this should not be null tho

            backAction = Game.Main.GetInputAction("Back", "Global");
            backAction.performed += OnInputGlobalBack;
        }

        public virtual void Place()
        {
        }

        public virtual void Interact(IInteractActor actor, InteractArgs args)
        {
            if (actor is not Player player) return;

            this.player = player;
            LoadGUIAsset(WorkstationData.GUI, onLoadCompleted: (gui) =>
            {
                InitializeGUI(gui);
            
                /// encapsulate
                player.InfoHUD.Hide();
                player.Actions.Disable();
                player.Controls.Disable();
                player.FPP.ToggleControls(false);

                player.UseWorkstation(this);
                Game.UI.ToggleCursor(true);

                player.InventoryGUI.OnClose += OnCloseInventory;
            });
        }

        void OnCloseInventory()
        {
            player.InventoryGUI.OnClose -= OnCloseInventory;
            backAction.performed -= OnInputGlobalBack;

            player.ResetToPlayerCraftingGUI();
            Game.UI.ToggleCursor(false);
            _GUI.Destroy();
            
            /// encapsulate
            player.InfoHUD.Show();
            player.Actions.Enable();
            player.Controls.Enable();
            player.FPP.ToggleControls(true);
            player = null;
        }


        protected virtual void LoadGUIAsset(AssetReference asset, Action<CraftingGUI> onLoadCompleted = null)
        {
            if (!asset.IsSet())
            {
                Game.Console.LogAndUnityLog($"No GUI set for Workstation '{WorkstationData.Id}'");
                return;
            }

            Addressables.LoadAssetAsync<GameObject>(asset).Completed += (a) =>
            {
                if (a.Status == AsyncOperationStatus.Succeeded)
                {
                    var go = Instantiate(a.Result);
                    
                    if (go.TryGetComponent<CraftingGUI>(out var gui))
                    {
                        onLoadCompleted?.Invoke(gui);
                        return;
                    }
                }
            };
        }

        void OnInputGlobalBack(InputAction.CallbackContext context)
        {
            OnCloseInventory();
        }
    }
}