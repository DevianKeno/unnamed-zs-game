using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.Objects;
using UZSG.Inventory;
using UZSG.Items;
using UZSG.Items.Weapons;
using UZSG.Interactions;
using UZSG.Data;
using UZSG.Players;

namespace UZSG.UI.HUD
{
    /// <summary>
    /// This hides when the Player is in their Inventory or another GUI.
    /// </summary>
    public class PlayerHUDInfo : Window
    {
        public Player Player;
        [Space]

        public LeanTweenType VignetteEase;
        public float VignetteFadeDuration;
        
        [Header("Elements")]
        public CrosshairHandler Crosshair;
        public Compass Compass;
        public Image vignette;
        public PickupsIndicator pickupsIndicator;
        public RadialProgressUI pickupTimer;

        ResourceHealthRingUI resourceHealthRingUI;

        internal void Initialize(Player player)
        {
            if (player == null)
            {
                Game.Console.LogAndUnityLog($"Invalid player.");
                return;
            }

            Player = player;
            Crosshair.Initialize(player);
            Compass.Initialize(player);
            
            resourceHealthRingUI = Game.UI.Create<ResourceHealthRingUI>("Resource Health Ring UI", show: false);

            InitializeEvents();
        }

        void InitializeEvents()
        {
            Player.Controls.OnCrouch += OnCrouch;
            Player.Actions.OnLookAtSomething += OnPlayerLookAtSomething;
            Player.Actions.OnPickupItem += OnPlayerPickupedItem;
            Player.Actions.OnInteractVehicle += OnInteractVehicle;
            Player.FPP.OnChangeHeldItem += OnChangeHeldItem;
        }

        void OnInteractVehicle(VehicleInteractContext context)
        {
            if (context.Entered)
            {
                Crosshair.Hide();
            }
            else if (context.Exited)
            {
                Crosshair.Show();
            }
        }

        void OnCrouch(bool crouched)
        {
            if (crouched)
            {
                FadeVignette(alpha: 1f);
            }
            else
            {
                FadeVignette(alpha: 0f);
            }
        }

        void OnDestroy()
        {
            Player.Actions.OnLookAtSomething -= OnPlayerLookAtSomething;
            Player.Actions.OnPickupItem -= OnPlayerPickupedItem;
        }

        #region Event callbacks

        void OnPlayerLookAtSomething(ILookable lookable)
        {
            if (lookable == null)
            {
                resourceHealthRingUI.Hide();
                return;
            }

            if (lookable is UZSG.Objects.Resource resource && !resourceHealthRingUI.IsVisible)
            {
                /// The player must be holding a Tool to display the resource's health
                if (!Player.FPP.IsHoldingTool) return;

                resourceHealthRingUI.DisplayResource(resource);
                resourceHealthRingUI.Show();
            }
        }

        void OnPlayerPickupedItem(Item item)
        {
            if (!item.IsNone)
            {
                pickupsIndicator.AddEntry(item);
            }
        }

        void OnChangeHeldItem(HeldItemController heldItem)
        {
            /// Hide the health ring if swapped to a non-tool Held Item
            if (resourceHealthRingUI.IsVisible && heldItem.ItemData.Type != ItemType.Tool)
            {
                resourceHealthRingUI.Hide();
            }
        }

        #endregion


        #region Public methods

        public void FadeVignette(float alpha)
        {
            LeanTween.cancel(vignette.rectTransform);
            LeanTween.alpha(vignette.rectTransform, alpha, VignetteFadeDuration)
            .setEase(VignetteEase);
        }

        #endregion
    }
}
