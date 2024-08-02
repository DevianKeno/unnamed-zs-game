using System;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.InputSystem;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.Interactions;
using UZSG.UI;

namespace UZSG.Objects
{
    public interface IPlaceable
    {
        public virtual void Place() { }
    }

    public class Station : BaseObject, IInteractable, IPlaceable
    {
        StationData stationData;
        public string ActionText => "Use";
        public string Name => objectData.Name;

        StationGUI _GUI;
        [SerializeField] Canvas canvas;

        public event EventHandler<InteractArgs> OnInteract;

        InputAction backAction;
        
        void Start()
        {
            /// TESTING ONLY
            Place(); 
            ///
        }

        public virtual void Place()
        {
            if (objectData is StationData stationData)
            {
                this.stationData = stationData;
                LoadGUIAsset();
            }

            backAction = Game.Main.GetInputAction("Back", "Global");
            backAction.performed += (ctx) =>
            {
                _GUI.Hide();
            };
        }

        public void Interact(IInteractActor actor, InteractArgs args)
        {
            if (actor is not Player player) return;

            player.HUD.Hide();
            player.Actions.Disable();
            player.Controls.Disable();
            player.FPP.ToggleControls(false);
            _GUI.Show();

            _GUI.OnClose += () =>
            {
                player.HUD.Show();
                player.Actions.Enable();
                player.Controls.Enable();
                player.FPP.ToggleControls(true);
            };
        }

        void LoadGUIAsset()
        {
            if (!stationData.GUI.IsSet())
            {
                Game.Console.LogAndUnityLog($"No GUI set for Station '{stationData.Id}'");
                return;
            }

            Addressables.LoadAssetAsync<GameObject>(stationData.GUI).Completed += (a) =>
            {
                if (a.Status == AsyncOperationStatus.Succeeded)
                {
                    var go = Instantiate(a.Result, canvas.transform);
                    if (go.TryGetComponent<StationGUI>(out var gui))
                    {
                        _GUI = gui;
                        _GUI.Hide();
                    }
                }
            };
        }
    }
}