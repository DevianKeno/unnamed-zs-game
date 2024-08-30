using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UZSG.FPP;
using UZSG.Systems;

namespace UZSG.Entities.Vehicles
{
    public class VehicleInputHandler : MonoBehaviour
    {
        VehicleEntity _vehicle;

        [Header("Vehicle Input")]
        InputAction _moveInput;
        InputAction _backInput;
        InputAction _handbrakeInput;
        InputAction _switchSeatInput;
        InputAction _switchViewInput;
        FPPCameraInput _cameraInput;

        private void Awake()
        {
            _vehicle = gameObject.GetComponent<VehicleEntity>();
        }

        private void Start()
        {
            _moveInput = Game.Main.GetInputAction("Vehicle Move", "Player Move");
            _backInput = Game.Main.GetInputAction("Back", "Global");
            _handbrakeInput = Game.Main.GetInputAction("Handbrake", "Player Move");
            _switchSeatInput = Game.Main.GetInputAction("Change Seat", "Player Actions");
            _switchViewInput = Game.Main.GetInputAction("Change Vehicle View", "Player Actions");
        }

        public void ToggleGeneralControls(Player player, bool isEnabled)
        {
            TogglePlayerMovement(player, !isEnabled);

            if (isEnabled)
            {
                _switchSeatInput.performed += OnSwitchSeatInputPerform;
                _switchViewInput.performed += OnSwitchViewInputPerform;
                _backInput.performed += OnBackInputPerform;
            }
            else
            {
                _switchSeatInput.performed -= OnSwitchSeatInputPerform;
                _switchViewInput.performed -= OnSwitchViewInputPerform;
                _backInput.performed -= OnBackInputPerform;
            }
        }

        public void TogglePlayerMovement(Player player, bool isEnabled)
        {
            player.Controls.SetControl("Move", isEnabled);
            player.Controls.SetControl("Jump", isEnabled);
            player.Controls.SetControl("Crouch", isEnabled);
            player.Controls.SetControl("Toggle Walk", isEnabled);
        }

        public void ToggleVehicleControls(bool isEnabled)
        {
            if (isEnabled)
            {
                _moveInput.performed += OnMoveInput;
                _moveInput.started += OnMoveInput;
                _moveInput.canceled += OnMoveInput;

                _handbrakeInput.started += OnHandbrakeInput;
                _handbrakeInput.canceled += OnHandbrakeInput;
            }
            else
            {
                _moveInput.performed -= OnMoveInput;
                _moveInput.started -= OnMoveInput;
                _moveInput.canceled -= OnMoveInput;

                _handbrakeInput.started -= OnHandbrakeInput;
                _handbrakeInput.canceled -= OnHandbrakeInput;
            }
        }

        public void ToggleFPPCameraInput(Player player, bool isEnabled)
        {
            // Enable Player Camera Look
            _cameraInput = player.transform.Find("FPP Camera Controller").GetComponent<FPPCameraInput>();
            _cameraInput.ResetCameraPosition = true;
            _cameraInput.ToggleControls(isEnabled);
        }

        #region Input Action Callbacks
        private void OnMoveInput(InputAction.CallbackContext context)
        {
            _vehicle.Controller.DriverInput = context.ReadValue<Vector2>();
        }

        private void OnHandbrakeInput(InputAction.CallbackContext context)
        {
            _vehicle.Controller.IsHandbraked = context.started ? true : false;
        }

        private void OnBackInputPerform(InputAction.CallbackContext context)
        {
            GameObject playerUI = GetPlayerGameObjectFromContext(context);
            // Testing Only
            Player player = playerUI.GetComponent<PlayerReference>().PlayerEntity;
            _vehicle.SeatManager.ExitVehicle(player);
        }

        private void OnSwitchSeatInputPerform(InputAction.CallbackContext context)
        {
            GameObject playerUI = GetPlayerGameObjectFromContext(context);
            // Testing Only
            Player player = playerUI.GetComponent<PlayerReference>().PlayerEntity;
            _vehicle.SeatManager.ChangeSeat(player);
        }

        private void OnSwitchViewInputPerform(InputAction.CallbackContext context)
        {
            GameObject playerUI = GetPlayerGameObjectFromContext(context);
            // Testing Only
            Player player = playerUI.GetComponent<PlayerReference>().PlayerEntity;
            _vehicle.CameraManager.ChangeVehicleView(player);
        }

        private GameObject GetPlayerGameObjectFromContext(InputAction.CallbackContext context)
        {
            // Retrieve the input device from the action context
            var control = context.action.controls.FirstOrDefault();
            if (control != null)
            {
                var device = control.device;
                // Find all PlayerInput components and look for the one associated with this device
                var playerInputs = FindObjectsOfType<PlayerInput>();
                foreach (var playerInput in playerInputs)
                {
                    if (playerInput.devices.Contains(device))
                    {
                        return playerInput.gameObject;
                    }
                }
            }
            return null;
        }

        #endregion
    }
}


