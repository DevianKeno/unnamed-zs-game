using System;

using UnityEngine;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.FPP;
using UZSG.Items;
using UZSG.Items.Weapons;
using UZSG.Interactions;

namespace UZSG.UI.HUD
{
    public class CrosshairHandler : MonoBehaviour
    {
        public Player player;
        [Space]

        public bool ShowDot = true;

        [SerializeField] GameObject dotCrosshair;
        [SerializeField] DynamicCrosshair dynamicCrosshair;
        
        internal void Initialize(Player player)
        {
            this.player = player;
            dynamicCrosshair.Initialize(player);

            InitializeEvents();

        }

        void InitializeEvents()
        {
            player.Actions.OnInteract += OnPlayerInteract;
            player.Actions.OnInteractVehicle += OnPlayerInteractVehicle;
            player.FPP.OnFPPControllerChanged += OnPlayerChangeHeldItem;
            player.BuildManager.OnEnteredBuildMode += OnEnteredBuildMode;
            player.BuildManager.OnExitedBuildMode += OnExitedBuildMode;
            
            
        }

        void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                dotCrosshair.SetActive(ShowDot);
            }
        }

        void OnDestroy()
        {
            Game.UI.OnWindowOpened -= OnWindowOpened;
            Game.UI.OnWindowClosed -= OnWindowClosed;
        }

        
        #region Event callbacks

        void OnPlayerInteract(InteractionContext context)
        {
            if (context.Phase == InteractPhase.Started)
            {
                Hide();
            }
            else if (context.Phase == InteractPhase.Finished || context.Phase == InteractPhase.Canceled)
            {
                Show();
            }
        }

        void OnPlayerInteractVehicle(VehicleInteractContext context)
        {
            if (context.Entered)
            {
                Hide();
            }
            else if (context.Exited)
            {
                Show();
            }
        }

        void OnPlayerChangeHeldItem(FPPItemController controller)
        {
            /// Switch crosshairs
            if (player.FPP.FPPItemController is GunWeaponController gunWeapon)
            {
                dotCrosshair.gameObject.SetActive(false);
                dynamicCrosshair.Show();
            }
            else
            {
                dynamicCrosshair.Hide();
                dotCrosshair.gameObject.SetActive(true);
            }
        }

        void OnEnteredBuildMode()
        {
            Hide();
        }

        void OnExitedBuildMode()
        {
            Show();
        }

        void OnUnpause()
        {
            Show();
        }

        void OnWindowOpened(Window window)
        {
            Hide();
        }

        void OnWindowClosed(Window window)
        {
            Show();
        }

        #endregion

        /// <summary>
        /// Crosshair will not display if meets the ff. criteria:
        ///  - there is a window open
        ///  - player is busy (interacting with something)
        ///  - player is in build mode (holding a placeable item)
        /// However, <c>force</c> displays it regardless.
        /// </summary>
        public void Show(bool force = false)
        {
            if (!force)
            {
                if (Game.UI.HasActiveWindow) return;
                if (player != null)
                if (player.Actions.IsBusy || player.BuildManager.InBuildMode)
                {
                    return;
                }
            }

            dotCrosshair.gameObject.SetActive(true);
            dynamicCrosshair.Show();
        }

        public void Hide()
        {
            dotCrosshair.gameObject.SetActive(false);
            dynamicCrosshair.Hide();
        }
    }
}