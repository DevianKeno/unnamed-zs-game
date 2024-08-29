using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.Items.Weapons;
using System;

namespace UZSG.FPP
{
    public class FPPCameraInput : MonoBehaviour
    {
        public Player Player;

        [Header("Camera Settings")]
        public float Sensitivity;
        public bool EnableControls = true;
        public bool ResetCameraPosition = false;

        bool _hasRecoil;
        bool _isLooking;
        /// <summary>
        /// True if the Player is looking around with the mouse.
        /// </summary>
        public bool IsLooking => _isLooking;

        float _verticalRotation = 0f;
        float _horizontalRotation = 0f;
        float _addedVerticalRecoil;
        float _addedHorizontalRecoil;
        float _recoilRecoverySpeedCached = 0f;

        Dictionary<string, InputAction> inputs;
        InputActionMap actionMap;
        InputAction look;

        [Header("Components")]
        public Transform Holder;
        [SerializeField] Camera FPPCamera;
        public Camera Camera => FPPCamera;
        [SerializeField] Animator animator;
        public Animator Animator => animator;
        
        internal void Initialize()
        {
            Camera.main.GetUniversalAdditionalCameraData().cameraStack.Add(FPPCamera);
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
            HandleRecoilRecovery();
        }

        public void ToggleControls(bool enabled)
        {
            EnableControls = enabled;

            if (enabled)
            {
                look.Enable();
            }
            else
            {
                look.Disable();
            }
        }

        void HandleLook()
        {
            if (!EnableControls) return;

            if (ResetCameraPosition) // Camera Reset (Temporary Only)
            {
                _verticalRotation = 0f;
                _horizontalRotation = 0f;
                transform.localEulerAngles = Vector3.zero;
                ResetCameraPosition = false;
                return; 
            }

            var lookInput = look.ReadValue<Vector2>();
            _isLooking = lookInput.x != 0 || lookInput.y != 0;
            // print($"Look: {lookInput}");
            // print(_isLooking);
            float mouseX = lookInput.x * Sensitivity;// * Time.deltaTime;
            float mouseY = lookInput.y * Sensitivity;// * Time.deltaTime;

            _verticalRotation -= mouseY;
            _verticalRotation = Mathf.Clamp(_verticalRotation, -80f, 80f);
            _horizontalRotation += mouseX;

            transform.localEulerAngles = new(
                _verticalRotation + _addedVerticalRecoil,
                _horizontalRotation + _addedHorizontalRecoil,
                0f
            );
        }

        void HandleRecoilRecovery()
        {
            if (_hasRecoil)
            {
                var time = Time.deltaTime;
                // var time = Game.Tick.SecondsPerTick * Time.deltaTime;
                _addedVerticalRecoil = Mathf.Lerp(_addedVerticalRecoil, 0f, time / _recoilRecoverySpeedCached);
                _addedHorizontalRecoil = Mathf.Lerp(_addedHorizontalRecoil, 0f, time / _recoilRecoverySpeedCached);

                if (Mathf.Abs(_addedVerticalRecoil) < 0.01f && Mathf.Abs(_addedHorizontalRecoil) < 0.01f)
                {
                    _recoilRecoverySpeedCached = 0f;
                    _hasRecoil = false;
                }
            }
        }

        public void AddRecoilMotion(RecoilAttributes recoilInfo)
        {
            _addedVerticalRecoil += -recoilInfo.VerticalRecoilAmount;
            _addedHorizontalRecoil += recoilInfo.HorizontalRecoilAmount * recoilInfo.HorizontalRecoilDirection;
            _recoilRecoverySpeedCached = recoilInfo.Speed;
            _hasRecoil = true;
        }

        void CursorToggledCallback(bool isVisible)
        {
            ToggleControls(!isVisible);
        }
    }
}
