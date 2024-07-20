using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.Players;

namespace UZSG.FPP
{
    public class FPPCameraController : MonoBehaviour
    {
        [Header("Camera Settings")]
        public float Sensitivity;
        public bool EnableControls = true;
        public bool EnableBobbing = true;
        float verticalRotation = 0f;
        
        [Header("Components")]
        [SerializeField] Player player;
        [SerializeField] Animator animator;
        public Animator Animator => animator;

        InputAction look;
        InputActionMap actionMap;
        Dictionary<string, InputAction> inputs;

        internal void Initialize()
        {
            InitializeInputs();
        }

        void InitializeInputs()
        {
            actionMap = Game.Main.GetActionMap("Player");
            inputs = Game.Main.GetActionsFromMap(actionMap);
        }

        void Start()
        {
            look = inputs["Look"];
            look.Enable();
        }

        void Update()
        {
            HandleLook();
        }
        
        public void ToggleControls(bool enabled)
        {
            EnableControls = enabled;

            if (enabled)
            {
                look.Enable();
            } else
            {
                look.Disable();
            }
        }

        void HandleLook()
        {
            if (!EnableControls) return;

            var lookInput = look.ReadValue<Vector2>();
            float mouseX = lookInput.x * Sensitivity * Time.deltaTime;
            float mouseY = lookInput.y * Sensitivity * Time.deltaTime;

            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -89f, 89f); 
            transform.localEulerAngles = new Vector3(verticalRotation, transform.localEulerAngles.y + mouseX, 0f);
        }

        void CursorToggledCallback(bool isVisible)
        {
            ToggleControls(!isVisible);
        }

        // public void SetBob(BobSetting b)
        // {
        //     Amplitude = b.Amplitude;
        //     Frequency = b.Frequency;
        // }

        // void ApplyForwardBob()
        // {
        //     if (!EnableBobbing) return;
        //     if (!Player.Controls.IsGrounded) return;
        //     if (Player.Controls.HorizontalSpeed < MinMoveSpeed) return;

        //     Vector3 camBob = _origin;
        //     Vector3 weaponBob = WeaponOrigin;
        //     float frequency = Time.time * Frequency;
        //     float sine = Mathf.Sin(frequency);
        //     float cosine = Mathf.Cos(frequency / 2);

        //     camBob += mainCamera.transform.right * (cosine * Amplitude * 2f);
        //     camBob += mainCamera.transform.up * (sine * Amplitude);
        //     // weaponBob += mainCamera.transform.right * (cosine * Amplitude * 0.5f);
        //     // weaponBob += -mainCamera.transform.up * (sine * Amplitude * 0.5f);
            
        //     // _framePosition += camBob;
        //     LocMotion(camBob);

        //     /// This makes the bobbing speed per the TPS
        //     // if (bobLerpTimer < Game.Tick.SecondsPerTick)
        //     // {
        //     //     bobLerpTimer += Time.time;                    
        //     //     virtualCamera.transform.localPosition = Vector3.Lerp(_camOrigin, camBob, bobLerpTimer / Game.Tick.SecondsPerTick);
        //     //     // WeaponHolder.transform.localPosition = Vector3.Lerp(WeaponOrigin, weaponBob, bobLerpTimer / Game.Tick.SecondsPerTick);
        //     // } else
        //     // {
        //     //     bobLerpTimer = 0f;
        //     // }
        // }
    }
}