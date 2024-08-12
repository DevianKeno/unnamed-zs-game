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
        [SerializeField] Crafter crafter;
        public Crafter Crafter => crafter;

        bool _hasGUILoaded;
        WorkstationGUI _GUI;
        [SerializeField] Canvas canvas;

        public event EventHandler<InteractArgs> OnInteract;

        InputAction backAction;
        
        protected override void Start()
        {
            /// TESTING ONLY
            /// Place() should execute when the object is placed on the world :)
            Place(); 
            ///
        }

        void InitializeCrafter(Player player)
        {
            // crafter.BindUI(_GUI as CraftingGUI);
            // crafter.AddRecipes(WorkstationData.IncludedRecipes);s
            player.ExternalCrafter = crafter;
        }

        void RequestCrafterInformation(Player player)
        {
            // crafter.AddContainer(player.Inventory.Bag);
            // crafter.AddContainer(player.Inventory.Hotbar);
            // crafter.AddRecipes(player.PlayerEntityData.KnownRecipes);
        }

        public virtual void Place()
        {
            LoadGUIAsset(WorkstationData.GUI, onLoadCompleted: (gui) =>
            {
                InitializeGUI(gui);
            });
        }

        public virtual void Interact(IInteractActor actor, InteractArgs args)
        {
            if (actor is not Player player) return;
            if (!_hasGUILoaded)
            {
                Game.Console.LogAndUnityLog($"Workstation '{WorkstationData.Id}' has no GUI loaded.");
                return;
            }

            player.InfoHUD.Hide();
            player.Actions.Disable();
            player.Controls.Disable();
            player.FPP.ToggleControls(false);
            Game.UI.ToggleCursor(true);

            _GUI.OnClose += () => 
            {
                player.InfoHUD.Show();
                player.Actions.Enable();
                player.Controls.Enable();
                player.FPP.ToggleControls(true);
                Game.UI.ToggleCursor(false);

                player.ExternalCrafter = null;
            };

            RequestCrafterInformation(player);
            InitializeCrafter(player);
        }

        public void OpenGUI()
        {
            _GUI.SetActive(true);
        }

        public void CloseGUI()
        {
            _GUI.SetActive(false);
        }

        protected virtual void LoadGUIAsset(AssetReference asset, Action<WorkstationGUI> onLoadCompleted = null)
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
                    var go = Instantiate(a.Result, canvas.transform);
                    
                    if (go.TryGetComponent<WorkstationGUI>(out var gui))
                    {
                        gui.SetActive(false);
                        onLoadCompleted?.Invoke(gui);
                        return;
                    }
                }
            };
        }

        void InitializeGUI(WorkstationGUI gui)
        {
            _GUI = gui;
            _GUI.Title = WorkstationData.WorkstationName;
            _hasGUILoaded = true;

            backAction = Game.Main.GetInputAction("Back", "Global");
            backAction.performed += (ctx) =>
            {
                CloseGUI();
            };
        }
    }
}