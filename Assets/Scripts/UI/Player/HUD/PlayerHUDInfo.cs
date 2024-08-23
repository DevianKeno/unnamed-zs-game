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
        public DynamicCrosshair crosshair;
        public SwitchCrosshair allCrosshair;
        public Compass compass;
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
            crosshair.Initialize(player);
            allCrosshair.Initialize(player);
            compass.Initialize(player);
            
            resourceHealthRingUI = Game.UI.Create<ResourceHealthRingUI>("Resource Health Ring UI", show: false);

            InitializeEvents();
        }

        void InitializeEvents()
        {
            Player.Actions.OnLookAtSomething += OnPlayerLookAtSomething;
            Player.Actions.OnPickupItem += OnPlayerPickupedItem;
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
            }
            else if (lookable is UZSG.Objects.Resource resource && !resourceHealthRingUI.IsVisible)
            {
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
