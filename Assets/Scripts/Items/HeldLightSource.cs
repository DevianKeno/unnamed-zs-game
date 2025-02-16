using UnityEngine;
using UnityEngine.InputSystem;

using UZSG.Entities;
using UZSG.FPP;
using UZSG.Items.Tools;

namespace UZSG.Items
{
    /// <summary>
    /// Controller for held light sources, which can be spot (flashlights) or point (lamps).
    /// </summary>
    public class HeldLightSource : HeldToolController, IDamageSource
    {
        /// <summary>
        /// Is the light source enabled?
        /// </summary>
        public bool IsOn
        {
            get => lightComponent.enabled;
            set => lightComponent.enabled = value;
        }
        /// <summary>
        /// The color of the light source.
        /// </summary>
        public Color Color
        {
            get => lightComponent.color;
            set => lightComponent.color = value;
        }
        [SerializeField] Light lightComponent;
        [SerializeField] ViewmodelLightSource lightSource;

        protected override void Awake()
        {
            base.Awake();
            
            lightComponent = GetComponentInChildren<ViewmodelLightSource>().Light;
        }

        public override void Initialize()
        {
            base.Initialize();
            InitializeEventsFromOwnerInput();   
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Player.Actions.ActionMap["Toggle Flashlight"].performed -= OnInputToggleFlashlight;
        }

        void InitializeEventsFromOwnerInput()
        {
            if (owner is not Player player) return;

            Player.Actions.ActionMap["Toggle Flashlight"].performed += OnInputToggleFlashlight;
        }

        void OnInputToggleFlashlight(InputAction.CallbackContext context)
        {
            IsOn = !IsOn;
            Game.Audio.Play("click");
        }
    }
}