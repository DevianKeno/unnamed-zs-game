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
using UZSG.Attributes;

namespace UZSG.UI.HUD
{
    /// <summary>
    /// This hides when the Player is in their Inventory or another GUI.
    /// </summary>
    public class PlayerHUDInfoUI : UIElement
    {
        public Player Player;
        [Space]

        public LeanTweenType VignetteEase;
        public float VignetteFadeDuration;

        bool _enableTimeText;
        
        [Header("Elements")]
        [SerializeField] TextMeshProUGUI dateTmp;
        [SerializeField] TextMeshProUGUI timeTmp;
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
            _enableTimeText = true;
            dateTmp.text = $"Day {Game.World.CurrentWorld.Time.CurrentDay.ToString()} | Monday";

            Game.Tick.OnSecond += OnSecond;
        }

        void OnSecond(SecondInfo info)
        {
            if (_enableTimeText)
            {
                var time = Game.World.CurrentWorld.Time;
                timeTmp.text = $"{time.Hour}:{time.Minute:D2}";
            }
        }

        void InitializeEvents()
        {
            Player.Controls.OnCrouch += OnCrouch;
            Player.Actions.OnLookAtSomething += OnPlayerLookAtSomething;
            Player.Actions.OnPickupItem += OnPlayerPickupedItem;
            Player.Actions.OnInteractVehicle += OnInteractVehicle;
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

        UZSG.Attributes.Attribute listeningToAttribute;

        void OnPlayerLookAtSomething(ILookable lookable)
        {
            if (lookable == null)
            {
                resourceHealthRingUI.Hide();
                return;
            }

            if (lookable is UZSG.Objects.Resource resource && !resourceHealthRingUI.IsVisible)
            {
                resourceHealthRingUI.DisplayResource(resource);
                resourceHealthRingUI.SetHealthRingVisible(resource.IsDamaged);
                resourceHealthRingUI.Show();

                if (resource.Attributes.TryGet("health", out var health))
                {
                    listeningToAttribute = health;
                    listeningToAttribute.OnValueChanged += UpdateUI;
                }
            }

            void UpdateUI(object sender, AttributeValueChangedContext e)
            {
                listeningToAttribute.OnValueChanged -= UpdateUI;
                resourceHealthRingUI.SetHealthRingVisible(true);
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
