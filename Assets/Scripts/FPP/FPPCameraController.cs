using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UZSG.Systems;
using UZSG.Entities;
using UZSG.Players;
using UZSG.Items.Weapons;

namespace UZSG.FPP
{
    public class FPPCameraController : MonoBehaviour
    {
        public Player Player;

        [Header("Camera Settings")]
        public float Sensitivity;
        public bool EnableControls = true;
        public bool EnableBobbing = true;
        float _verticalRotation = 0f;
        float _horizontalRotation = 0f;

        bool _isRecoiling;
        float _currentVerticalRecoil;
        float _currentHorizontalRecoil;
        float _recoilRecoverySpeedCached = 0f;

        [Header("Components")]
        [SerializeField] Animator animator;
        public Animator Animator => animator;
        
        InputActionMap actionMap;
        InputAction look;
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

            var lookInput = look.ReadValue<Vector2>();
            float mouseX = lookInput.x * Sensitivity * Time.deltaTime;
            float mouseY = lookInput.y * Sensitivity * Time.deltaTime;

            _verticalRotation -= mouseY;
            _verticalRotation = Mathf.Clamp(_verticalRotation, -89f, 89f);
            _horizontalRotation += mouseX;

            transform.localEulerAngles = new Vector3(
                _verticalRotation + _currentVerticalRecoil,
                _horizontalRotation + _currentHorizontalRecoil,
                0f
            );
        }

        void HandleRecoilRecovery()
        {
            if (_isRecoiling)
            {
                var time = Time.deltaTime;
                // var time = Game.Tick.SecondsPerTick * Time.deltaTime;
                _currentVerticalRecoil = Mathf.Lerp(_currentVerticalRecoil, 0f, time / _recoilRecoverySpeedCached);
                _currentHorizontalRecoil = Mathf.Lerp(_currentHorizontalRecoil, 0f, time / _recoilRecoverySpeedCached);

                if (Mathf.Abs(_currentVerticalRecoil) < 0.01f && Mathf.Abs(_currentHorizontalRecoil) < 0.01f)
                {
                    _recoilRecoverySpeedCached = 0f;
                    _isRecoiling = false;
                }
            }
        }

        public void AddRecoilMotion(RecoilAttributes recoilInfo)
        {
            _currentVerticalRecoil += -recoilInfo.VerticalRecoilAmount;
            _currentHorizontalRecoil += recoilInfo.HorizontalRecoilAmount * recoilInfo.HorizontalRecoilDirection;
            _recoilRecoverySpeedCached = recoilInfo.RecoilRecoverySpeed;
            _isRecoiling = true;
        }

        void CursorToggledCallback(bool isVisible)
        {
            ToggleControls(!isVisible);
        }
    }
}
