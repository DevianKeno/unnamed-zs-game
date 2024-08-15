using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UZSG.Systems;

namespace UZSG.Entities.Vehicles
{
    public class VehicleController : MonoBehaviour
    {
        [Header("Vehicle Variables")]
        [SerializeField] VehicleEntity _vehicle;
        [SerializeField] protected VehicleStateMachine _vehicleStateMachine;
        List<WheelCollider> _frontWheelColliders;
        List<WheelCollider> _rearWheelColliders;

        [Header("Vehicle Input")]
        InputAction _moveInput;
        InputAction _backInput;
        InputAction _switchInput;

        Vector2 _driverInput;

        private void Awake()
        {
            _vehicle = this.GetComponent<VehicleEntity>();
            _frontWheelColliders = _vehicle.FrontVehicleWheels;
            _rearWheelColliders = _vehicle.RearVehicleWheels;
        }

        private void Start()
        {
            _moveInput = Game.Main.GetInputAction("Move", "Player Move");
            _backInput = Game.Main.GetInputAction("Back", "Global");
            _switchInput = Game.Main.GetInputAction("Change Seat", "Player Actions");
        }

        private void FixedUpdate()
        {
            if (_vehicle.Driver != null)
            {
                HandlePlayerPosition();
                HandleGas();
            }
        }

        private void HandlePlayerPosition()
        {
            
        }

        public void HandleGas()
        {
            for (int i = 0; i < _frontWheelColliders.Count; i++)
            {
                _frontWheelColliders[i].motorTorque = _driverInput.y * _vehicle.Vehicle.MaxSpeed;
            }
        }

        public void HandleSteer()
        {

        }

        public void HandleBreak()
        {

        }

        #region Vehicle Control Functions 
        public void EnableGeneralVehicleControls()
        {
            _switchInput.performed += OnSwitchInputPerform;
            _backInput.performed += OnBackInputPerform;
        }

        public void DisableGeneralVehicleControls()
        {
            _switchInput.performed -= OnSwitchInputPerform;
            _backInput.performed -= OnBackInputPerform;
        }

        public void EnableVehicleControls()
        {
            _moveInput.started += OnMoveInput;
            _moveInput.canceled += OnMoveInput;
        }

        public void DisableVehicleControls()
        {
            _moveInput.started -= OnMoveInput;
            _moveInput.canceled -= OnMoveInput;
        }

        private void OnMoveInput(InputAction.CallbackContext context)
        {
            _driverInput = context.ReadValue<Vector2>();
        }

        private void OnBackInputPerform(InputAction.CallbackContext context)
        {
            GameObject playerUI = GetPlayerGameObjectFromContext(context);
            // Testing Only
            Player player = playerUI.GetComponent<PlayerReference>().PlayerEntity;
            _vehicle.ExitVehicle(player);
        }

        private void OnSwitchInputPerform(InputAction.CallbackContext context)
        {
            GameObject playerUI = GetPlayerGameObjectFromContext(context);
            // Testing Only
            Player player = playerUI.GetComponent<PlayerReference>().PlayerEntity;
            _vehicle.ChangeSeat(player);
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
