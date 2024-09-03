using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.Items.Weapons;
using System;
using UZSG.Players;
using System.Collections;
using MEC;

namespace UZSG.FPP
{
    public class FPPCameraInput : MonoBehaviour
    {
        public Player Player;

        [Header("Camera Settings")]
        public float Sensitivity;
        public bool EnableControls = true;
        public bool SmoothMouse;
        public float Smoothness = 1f;
        public bool ResetCameraPosition = false;
        
        [Header("Crouch Camera Settings")]
        public float CrouchDuration = 0.666f;
        public Vector3 CrouchOffset = new(0f, -0.6f, 0f);
        /// <summary>
        /// Maintain forward look even while crouching.
        /// </summary>
        public bool CrouchLookForward = true;
        public float CrouchLookForwardDistance = 16f;
        public float CrouchLookForwardDamping = 10f;
        public LeanTweenType CrouchEase = LeanTweenType.linear;

        bool _hasRecoil;
        /// <summary>
        /// True if the Player is looking around with the mouse.
        /// </summary>
        public bool IsLooking { get; private set; }

        float _verticalRotation = 0f;
        float _horizontalRotation = 0f;
        float _addedVerticalRecoil;
        float _addedHorizontalRecoil;
        float _recoilRecoverySpeedCached = 0f;
        Vector3 _originalLocalPosition;
        
        Dictionary<string, InputAction> inputs;
        InputActionMap actionMap;
        InputAction look;

        [Header("Components")]
        public Transform Holder;
        [SerializeField] Camera FPPCamera;
        public Camera Camera => FPPCamera;
        
        internal void Initialize()
        {
            Camera.main.GetUniversalAdditionalCameraData().cameraStack.Add(FPPCamera);
            InitializeInputs();
            Player.Controls.OnCrouch += OnCrouch;
            _originalLocalPosition = transform.localPosition;
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

        CoroutineHandle crouchTransitionCoroutine;

        void OnCrouch(bool crouched)
        {
            Vector3 targetPosition = _originalLocalPosition;
            
            if (crouched)
            {
                targetPosition += CrouchOffset;
            }
            else
            {
                targetPosition = _originalLocalPosition;
            }

            LeanTween.cancel(gameObject);
            LeanTween.moveLocal(gameObject, targetPosition, CrouchDuration)
            .setEase(CrouchEase);

            if (CrouchLookForward)
            {
                Timing.KillCoroutines(crouchTransitionCoroutine);
                crouchTransitionCoroutine = Timing.RunCoroutine(CrouchLookForwardCoroutine());
            }
        }

        IEnumerator<float> CrouchLookForwardCoroutine()
        {
            const int MaxIter = 50;
            for (int i = 0; i <= (CrouchDuration / 0.02f) || i <= MaxIter; i++) /// 0.02f is fixedUpdate interval in seconds
            {
                var targetRotation = Quaternion.Inverse(transform.localRotation) * Quaternion.LookRotation(Player.Forward * CrouchLookForwardDistance);
                transform.localRotation = Quaternion.Slerp(
                    transform.localRotation,
                    targetRotation,
                    CrouchLookForwardDamping * Time.fixedDeltaTime
                );

                yield return Timing.WaitForOneFrame;
            }
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
            IsLooking = lookInput.x != 0 || lookInput.y != 0;
            float mouseX = lookInput.x * Sensitivity;
            float mouseY = lookInput.y * Sensitivity;
            
            _verticalRotation -= mouseY;
            _verticalRotation = Mathf.Clamp(_verticalRotation, -80f, 80f);
            _horizontalRotation += mouseX;

            var targetRotation = new Vector3(
                _verticalRotation + _addedVerticalRecoil,
                _horizontalRotation + _addedHorizontalRecoil,
                0f
            );

            if (SmoothMouse)
            {
                targetRotation = Vector3.Lerp(transform.localEulerAngles, targetRotation, Smoothness * Time.deltaTime);
            }
            
            transform.localEulerAngles = targetRotation;
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
