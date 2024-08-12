using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

using UZSG.Entities;
using UZSG.Inventory;
using UZSG.Systems;
using UZSG.Items;
using UZSG.UI.HUD;
using UZSG.Items.Weapons;
using UZSG.Players;

namespace UZSG.UI.HUD
{
    public class PlayerInfoHUD : Window
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
        public GameObject pickupsIndicatorContainer;

        internal void Initialize(Player player)
        {
            if (player == null)
            {
                Game.Console.LogAndUnityLog($"Invalid player.");
                return;
            }

            Player = player;
            InitializeEvents();
            crosshair.Initialize(player);
            allCrosshair.Initialize(player);
            compass.Initialize(player);
        }

        void InitializeEvents()
        {            
            Player.Actions.OnPickupItem += (item) =>
            {
                var indicator = Game.UI.Create<PickupsIndicator>("Pickups Indicator", show: false);
                indicator.transform.SetParent(pickupsIndicatorContainer.transform); 
                indicator.SetDisplayedItem(item);
                indicator.PlayAnimation();
            };
        }

        public void FadeVignette(float alpha)
        {
            LeanTween.cancel(vignette.rectTransform);
            LeanTween.alpha(vignette.rectTransform, alpha, VignetteFadeDuration)
            .setEase(VignetteEase);
        }
    }
}
