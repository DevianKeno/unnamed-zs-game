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
        public VehicleEntity Vehicle { get; set; }

        [Header("Vehicle Input")]
        InputAction _moveInput;
        InputAction _backInput;
        InputAction _handbrakeInput;
        InputAction _switchSeatInput;
        InputAction _switchViewInput;
        FPPCameraInput _cameraInput;

        void Awake()
        {
            Vehicle = GetComponent<VehicleEntity>();
        }

        void Start()
        {
            _moveInput = Game.Main.GetInputAction("Vehicle Move", "Player Move");
            _handbrakeInput = Game.Main.GetInputAction("Handbrake", "Player Move");

            _switchSeatInput = Game.Main.GetInputAction("Change Seat", "Player Actions");
            _switchViewInput = Game.Main.GetInputAction("Change Vehicle View", "Player Actions");

            _backInput = Game.Main.GetInputAction("Back", "Global");
        }

        public void ToggleGeneralControls(Player player, bool isEnabled)
        {
            TogglePlayerMovement(player, !isEnabled);

            /// hello I moved this to the PlayerActions.cs because it's not working for the other scenes ;)
            if (isEnabled)
            {
                // _switchSeatInput.performed += OnSwitchSeatInputPerform;
                // _switchViewInput.performed += OnSwitchViewInputPerform;
                // _backInput.performed += OnBackInputPerform;
            }
            else
            {
                // _switchSeatInput.performed -= OnSwitchSeatInputPerform;
                // _switchViewInput.performed -= OnSwitchViewInputPerform;
                // _backInput.performed -= OnBackInputPerform;
            }
        }

        public void TogglePlayerMovement(Player player, bool isEnabled)
        {
            // player.Controls.SetControl("Move", isEnabled);
            // player.Controls.SetControl("Jump", isEnabled);
            // player.Controls.SetControl("Crouch", isEnabled);
            // player.Controls.SetControl("Toggle Walk", isEnabled);
            
            /// with love
            string[] enabledControlsIds = new[]
            {
                "Move", "Jump", "Crouch", "Toggle Walk"
            };

            player.Controls.SetControls(enabledControlsIds, isEnabled);

            /// or
            // string[] disabledControlsIds = new[]
            // {
            //     "Move", "Jump", "Crouch", "Toggle Walk"
            // };

            // player.Controls.SetControls(disabledControlsIds, !isEnabled);
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
        void OnMoveInput(InputAction.CallbackContext context)
        {
            Vehicle.Controller.DriverInput = context.ReadValue<Vector2>();
        }

        void OnHandbrakeInput(InputAction.CallbackContext context)
        {
            Vehicle.Controller.IsHandbraked = context.started;
        }

        void OnBackInputPerform(InputAction.CallbackContext context)
        {
            GameObject playerUI = GetPlayerGameObjectFromContext(context);
            // Testing Only
            Player player = playerUI.GetComponent<PlayerReference>().PlayerEntity;
            Vehicle.SeatManager.ExitVehicle(player);
        }

        void OnSwitchSeatInputPerform(InputAction.CallbackContext context)
        {
            GameObject playerUI = GetPlayerGameObjectFromContext(context);
            // Testing Only
            Player player = playerUI.GetComponent<PlayerReference>().PlayerEntity;
            Vehicle.SeatManager.ChangeSeat(player);
        }

        void OnSwitchViewInputPerform(InputAction.CallbackContext context)
        {
            GameObject playerUI = GetPlayerGameObjectFromContext(context);
            // Testing Only
            Player player = playerUI.GetComponent<PlayerReference>().PlayerEntity;
            Vehicle.CameraManager.ChangeVehicleView(player);
        }

        GameObject GetPlayerGameObjectFromContext(InputAction.CallbackContext context)
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


