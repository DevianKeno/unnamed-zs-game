using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

using UZSG.Attributes;
using UZSG.Entities;
using UZSG.Items;
using UZSG.Interactions;

namespace UZSG.UI.HUD
{
    /// <summary>
    /// This hides when the Player is in their Inventory or another GUI.
    /// </summary>
    public class PlayerHUDInfoUI : UIElement
    {
        public Player Player { get; private set; }
        [Space]

        [SerializeField] LeanTweenType VignetteEase;
        [SerializeField] float VignetteFadeDuration;
        [SerializeField] bool displayClockSeconds;

        string _previousDateTimeText;
        List<UIElement> _previouslyVisibleElements = new();

        [Header("UI Elements")]
        [SerializeField] TextMeshProUGUI dateTmp;
        [SerializeField] TextMeshProUGUI timeTmp;
        [SerializeField] InteractionIndicator interactionIndicator;
        [SerializeField] CrosshairHandler Crosshair;
        [SerializeField] Compass Compass;
        [SerializeField] Image vignette;
        [SerializeField] PickupsIndicator pickupsIndicator;
        [SerializeField] RadialProgressUI pickupTimer;

        ResourceHealthRingUI resourceHealthRingUI;

        internal void Initialize(Player player)
        {
            if (player == null)
            {
                Game.Console.LogWithUnity($"Invalid player.");
                return;
            }

            Player = player;
            
            Crosshair.Initialize(player);
            Compass.Initialize(player);
            interactionIndicator = Game.UI.Create<InteractionIndicator>("Interaction Indicator", show: false);
            resourceHealthRingUI = Game.UI.Create<ResourceHealthRingUI>("Resource Health Ring UI", show: false);
            InitializeEvents();
        }

        void InitializeEvents()
        {
            Player.Controls.OnCrouch += OnCrouch;
            Player.Actions.OnLookAtSomething += OnPlayerLookAtSomething;
            Player.Actions.OnPickupItem += OnPlayerPickupedItem;
            
            Game.World.CurrentWorld.Time.OnDayPassed += OnDayPassed;
            Game.World.CurrentWorld.Time.OnHourPassed += OnHourPassed;
            Game.World.CurrentWorld.Time.OnMinutePassed += OnMinutePassed;
            Game.World.CurrentWorld.Time.OnSecondPassed += OnSecondPassed;
            Game.UI.OnAnyWindowOpened += OnWindowOpened;
            Game.UI.OnAnyWindowClosed += OnWindowClosed;
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
            Player.Controls.OnCrouch -= OnCrouch;
            Player.Actions.OnLookAtSomething -= OnPlayerLookAtSomething;
            Player.Actions.OnPickupItem -= OnPlayerPickupedItem;
            
            Game.UI.OnAnyWindowOpened -= OnWindowOpened;
            Game.UI.OnAnyWindowClosed -= OnWindowClosed;

            interactionIndicator.Destruct();
            resourceHealthRingUI.Destruct();
        }


        #region Event callbacks

        UZSG.Attributes.Attribute listeningToAttribute;

        void OnPlayerLookAtSomething(IInteractable interactable, List<InteractAction> actions)
        {
            if (interactable == null)
            {
                interactionIndicator.Hide();
                resourceHealthRingUI.Hide();
                return;
            }

            interactionIndicator.Indicate(interactable, actions);

            if (interactable is UZSG.Objects.Resource resource && !resourceHealthRingUI.IsVisible)
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

        void OnPlayerInteract(InteractionContext context)
        {
            if (context.Phase == InteractPhase.Started)
            {
                interactionIndicator.Hide();
                resourceHealthRingUI.Hide();
            }
        }

        void OnPlayerPickupedItem(Item item)
        {
            if (!item.IsNone)
            {
                pickupsIndicator.AddEntry(item);
            }
        }

        void OnDayPassed(int day)
        {
            dateTmp.text = $"Day {day}";
        }

        void OnHourPassed(int hour)
        {
            if (displayClockSeconds)
            {
                timeTmp.text = $"{hour}:{Game.World.CurrentWorld.Time.Minute:D2}:{Game.World.CurrentWorld.Time.Second:D2}";
            }
            else
            {
                timeTmp.text = $"{hour}:{Game.World.CurrentWorld.Time.Minute:D2}";
            }
        }

        void OnMinutePassed(int minute)
        {
            if (displayClockSeconds)
            {
                timeTmp.text = $"{Game.World.CurrentWorld.Time.Hour}:{minute:D2}:{Game.World.CurrentWorld.Time.Second:D2}";
            }
            else
            {
                timeTmp.text = $"{Game.World.CurrentWorld.Time.Hour}:{minute:D2}";
            }
        }

        void OnSecondPassed(int second)
        {
            if (displayClockSeconds)
            {
                timeTmp.text = $"{Game.World.CurrentWorld.Time.Hour}:{Game.World.CurrentWorld.Time.Minute:D2}:{second:D2}";
            }
        }

        void OnWindowOpened(Window window)
        {
            HideEverything();
        }

        void OnWindowClosed(Window window)
        {
            ShowEverythingThatWasOnceVisible();
            _previouslyVisibleElements.Clear();
        }

        #endregion


        #region Public methods

        public void FadeVignette(float alpha)
        {
            LeanTween.cancel(vignette.rectTransform);
            LeanTween.alpha(vignette.rectTransform, alpha, VignetteFadeDuration)
            .setEase(VignetteEase);
        }

        void ShowEverythingThatWasOnceVisible()
        {
            foreach (var element in _previouslyVisibleElements)
            {
                element?.Show();
            }
        }

        public void HideEverything()
        {
            StoreIfVisibleThenHide(interactionIndicator);
            StoreIfVisibleThenHide(resourceHealthRingUI);
            
            void StoreIfVisibleThenHide(UIElement element)
            {
                if (element.IsVisible)
                {
                    _previouslyVisibleElements.Add(element);
                }
                element.Hide();
            }
        }

        #endregion
    }
}
