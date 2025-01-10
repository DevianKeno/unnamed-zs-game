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

            player.Actions.OnInteract += OnInteract;
            player.FPP.OnFPPControllerChanged += OnChangeHeldItem;
            player.BuildManager.OnEnteredBuildMode += OnEnteredBuildMode;
            player.BuildManager.OnExitedBuildMode += OnExitedBuildMode;
            Game.World.CurrentWorld.OnPause += Hide;
            Game.World.CurrentWorld.OnUnpause += OnUnpause;
            Game.UI.OnWindowOpened += OnWindowOpened;
            Game.UI.OnWindowClosed += OnWindowClosed;
        }

        void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                dotCrosshair.SetActive(ShowDot);
            }
        }

        
        #region Event callbacks

        void OnInteract(InteractContext context)
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

        void OnChangeHeldItem(FPPItemController controller)
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