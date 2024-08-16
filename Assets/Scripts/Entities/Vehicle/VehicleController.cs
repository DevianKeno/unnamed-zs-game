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
        InputAction _handbrakeInput;
        InputAction _switchInput;

        [Header("Vehicle Setup")]
        public GameObject bodyMassCenter;

        // CAR DATA
        [HideInInspector]
        public int maxSpeed; //The maximum speed that the car can reach in km/h.
        [HideInInspector]
        public int maxReverseSpeed; //The maximum speed that the car can reach while going on reverse in km/h.
        [HideInInspector]
        public AnimationCurve powerCurve;   // Experimental Power/Torque Curve for more customized acceleration
        [HideInInspector]
        public bool frontPower;   // Send Power to Front Wheels
        [HideInInspector]
        public bool rearPower;   // Send Power to Rear Wheels
        [HideInInspector]
        public float maxSteeringAngle; // The maximum angle that the tires can reach while rotating the steering wheel.
        [HideInInspector]
        public float steeringSpeed ; // How fast the steering wheel turns.
        [HideInInspector]
        public int brakeForce; // The strength of the wheel brakes.
        [HideInInspector]
        public int decelerationMultiplier; // How fast the car decelerates when the user is not using the throttle.
        [HideInInspector]
        public float carSpeed; // Used to store the current speed of the car.
        [HideInInspector]
        public float powerToWheels; // Used to store final wheel torque
        [HideInInspector]
        public bool isTractionLocked; // Used to know whether the traction of the car is locked or not.
        [HideInInspector]
        public float defaultMaxSteerAngle; // Used to store the original max steering angle.

        /*
       IMPORTANT: The following variables should not be modified manually since their values are automatically given via script.
        */
        Rigidbody _carRigidbody; // Stores the car's rigidbody.
        List<WheelCollider> _wheels; // Store all wheel colliders.
        float _steeringAxis; // Used to know whether the steering wheel has reached the maximum value. It goes from -1 to 1.
        float _throttleAxis; // Used to know whether the throttle has reached the maximum value. It goes from -1 to 1.
        float _localVelocityZ;
        float _localVelocityX;
        bool _deceleratingCar;

        bool _isMoving;
        bool _isHandbraked;
        Vector2 _driverInput;

        private void Awake()
        {
            _vehicle = this.GetComponent<VehicleEntity>();
            _frontWheelColliders = _vehicle.FrontVehicleWheels;
            _rearWheelColliders = _vehicle.RearVehicleWheels;
            _wheels = _frontWheelColliders.Concat(_rearWheelColliders).ToList();
        }

        private void Start()
        {
            _moveInput = Game.Main.GetInputAction("Vehicle Move", "Player Move");
            _backInput = Game.Main.GetInputAction("Back", "Global");
            _handbrakeInput = Game.Main.GetInputAction("Handbrake", "Player Move");
            _switchInput = Game.Main.GetInputAction("Change Seat", "Player Actions");
            _carRigidbody = gameObject.GetComponent<Rigidbody>();

            if (_carRigidbody.automaticCenterOfMass)
            {
                
            }
            else
            {
                _carRigidbody.centerOfMass = bodyMassCenter.transform.localPosition;
            }
            

            // Initialize vehicle setup
            maxSpeed = _vehicle.Vehicle.maxSpeed;
            maxReverseSpeed = _vehicle.Vehicle.maxReverseSpeed;
            powerCurve = _vehicle.Vehicle.powerCurve;
            frontPower = _vehicle.Vehicle.frontPower;
            rearPower = _vehicle.Vehicle.rearPower;
            maxSteeringAngle = _vehicle.Vehicle.maxSteeringAngle;
            steeringSpeed = _vehicle.Vehicle.steeringSpeed;
            brakeForce = _vehicle.Vehicle.brakeForce;
            decelerationMultiplier = _vehicle.Vehicle.decelerationMultiplier;
        }

        private void FixedUpdate()
        {
            // Compute car speed using one of the wheels
            carSpeed = (2 * Mathf.PI * _frontWheelColliders[0].radius * _frontWheelColliders[0].rpm * 60) / 1000;

            // Save the local velocity of the car in the z axis. Used to know if the car is going forward or backwards.
            _localVelocityZ = transform.InverseTransformDirection(_carRigidbody.velocity).z;

            if (_vehicle.Driver != null)
            {
                HandlePlayerPosition();

                // Vehicle Controls
                if (_driverInput.y > 0)
                {
                    CancelInvoke("DecelerateVehicle");
                    _deceleratingCar = false;
                    HandleGas();
                }
                if (_driverInput.y < 0)
                {
                    CancelInvoke("DecelerateVehicle");
                    _deceleratingCar = false;
                    HandleReverse();
                }
                if (_driverInput.x < 0)
                {
                    HandleLeftSteer();
                }
                if (_driverInput.x > 0)
                {
                    HandleRightSteer();
                }
                if (_isHandbraked)
                {
                    CancelInvoke("DecelerateVehicle");
                    _deceleratingCar = false;
                    HandleHandbrake();
                }
                if (_driverInput.y == 0)
                {
                    ThrottleOff();
                }
                if ((_driverInput.x == 0) && _steeringAxis != 0f)
                {
                    ResetSteeringAngle();
                }
                if (Input.GetKey(KeyCode.Tilde))    // Reset pag tumaob, parang di nagana? idk kung tama ba yung tranform na tinatarget ko
                {
                    Quaternion targetRotation = Quaternion.identity;
                    transform.rotation = targetRotation;
                }

            }
        }

        private void HandlePlayerPosition()
        {
            
        }

        public void HandleGas()
        {
            // Sets throttle power to 1 smoothly
            _throttleAxis = _throttleAxis + (Time.deltaTime * 3f);
            if (_throttleAxis > 1f)
            {
                _throttleAxis = 1f;
            }

            //If the car is going backwards, then apply brakes in order to avoid strange
            //behaviours. If the local velocity in the 'z' axis is less than -1f, then it
            //is safe to apply positive torque to go forward.
            if (_localVelocityZ < -1f)
            {
                HandleBrake();
            }
            else
            {
                if(Mathf.RoundToInt(carSpeed) < maxSpeed)
                {
                    //Normalize speed 0 to 1 to evaluate to the PowerCurve
                    float _normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / maxSpeed);
                    float _availableTorque = powerCurve.Evaluate(_normalizedSpeed) * 10;
                    powerToWheels = (_availableTorque * 150f) * _throttleAxis;

                    Drivetrain(powerToWheels);
                }
                else
                {
                    // Stop applying power when max speed is reached
                    for (int i = 0; i < _frontWheelColliders.Count; i++)
                    {
                        _frontWheelColliders[i].motorTorque = 0;
                    }

                    for (int i = 0; i < _rearWheelColliders.Count; i++)
                    {
                        _rearWheelColliders[i].motorTorque = 0;
                    }
                }
            }
            
        }

        public void HandleLeftSteer()
        {
            //The following method turns the front car wheels to the left. The speed of this movement will depend on the steeringSpeed variable.
            _steeringAxis = _steeringAxis - (Time.deltaTime * 10f * steeringSpeed);
            if (_steeringAxis < -1f)
            {
                _steeringAxis = -1f;
            }

            var steeringAngle = _steeringAxis * maxSteeringAngle;
            _frontWheelColliders[0].steerAngle = Mathf.Lerp(_frontWheelColliders[0].steerAngle, steeringAngle, steeringSpeed); // note to charles: sana LEFT WHEEL to
            _frontWheelColliders[1].steerAngle = Mathf.Lerp(_frontWheelColliders[1].steerAngle, steeringAngle, steeringSpeed); // sana RIGHT WHEEL to
        }

        public void HandleRightSteer()
        {
            //The following method turns the front car wheels to the right. The speed of this movement will depend on the steeringSpeed variable.
            _steeringAxis = _steeringAxis - (Time.deltaTime * 10f * steeringSpeed);
            if (_steeringAxis < 1f)
            {
                _steeringAxis = 1f;
            }

            var steeringAngle = _steeringAxis * maxSteeringAngle;
            _frontWheelColliders[0].steerAngle = Mathf.Lerp(_frontWheelColliders[0].steerAngle, steeringAngle, steeringSpeed); // note to charles: sana LEFT WHEEL to
            _frontWheelColliders[1].steerAngle = Mathf.Lerp(_frontWheelColliders[1].steerAngle, steeringAngle, steeringSpeed); // sana RIGHT WHEEL to
        }

        public void HandleBrake()
        {
            for (int i = 0; i < _frontWheelColliders.Count; i++)
            {
                _frontWheelColliders[i].brakeTorque = brakeForce;
            }

            for (int i = 0; i < _rearWheelColliders.Count; i++)
            {
                _rearWheelColliders[i].brakeTorque = brakeForce;
            }
        }

        public void HandleReverse()
        {
            // Sets throttle power to -1 smoothly
            _throttleAxis = _throttleAxis - (Time.deltaTime * 3f);
            if (_throttleAxis < -1f)
            {
                _throttleAxis = -1f;
            }

            //If the car is still going forward, then apply brakes in order to avoid strange
            //behaviours. If the local velocity in the 'z' axis is greater than 1f, then it
            //is safe to apply negative torque to go reverse.
            if (_localVelocityZ > 1f)
            {
                HandleBrake();
            }
            else
            {
                if(Mathf.Abs(Mathf.RoundToInt(carSpeed)) < maxReverseSpeed)
                {
                    //Apply negative torque in all wheels to go in reverse if maxReverseSpeed has not been reached.
                    for (int i = 0; i < _frontWheelColliders.Count; i++)
                    {
                        _frontWheelColliders[i].motorTorque = (decelerationMultiplier * 50f) * _throttleAxis;
                        _frontWheelColliders[i].brakeTorque = 0;
                    }

                    for (int i = 0; i < _rearWheelColliders.Count; i++)
                    {
                        _rearWheelColliders[i].motorTorque = (decelerationMultiplier * 50f) * _throttleAxis;
                        _rearWheelColliders[i].brakeTorque = 0;
                    }
                }
                else
                {
                    //If the maxReverseSpeed has been reached, then stop applying torque to the wheels.
                    // IMPORTANT: The maxReverseSpeed variable should be considered as an approximation; the speed of the car
                    // could be a bit higher than expected.

                    for (int i = 0; i < _frontWheelColliders.Count; i++)
                    {
                        _frontWheelColliders[i].motorTorque = 0;
                    }

                    for (int i = 0; i < _rearWheelColliders.Count; i++)
                    {
                        _rearWheelColliders[i].motorTorque = 0;
                    }
                }
            }
        }

        public void HandleHandbrake()
        {
            for (int i = 0; i < _rearWheelColliders.Count; i++)
            {
                _rearWheelColliders[i].brakeTorque = 2000f;
            }

            // Check for slip during handbrake
            WheelHit[] _wheelHits = new WheelHit[4];

            for(int i = 0; i < _wheels.Count; i++)
            {
                _wheels[i].GetGroundHit(out _wheelHits[i]);
                float lateralSlip = _wheelHits[i].sidewaysSlip;

                if (Math.Abs(lateralSlip) > 0.2f)
                {
                    // TO DO: sliding/skidding mechanic
                }
            }
        }

        public void DecelerateVehicle()
        {
            if (_throttleAxis != 0f)
            {
                if (_throttleAxis > 0f)
                {
                    _throttleAxis = _throttleAxis - (Time.deltaTime * 10f);
                }
                else if (_throttleAxis < 0f)
                {
                    _throttleAxis = _throttleAxis + (Time.deltaTime * 10f);
                }
                if (Mathf.Abs(_throttleAxis) < 0.15f)
                {
                    _throttleAxis = 0f;
                }
            }
            _carRigidbody.velocity = _carRigidbody.velocity * (1f / (1f + (0.025f * decelerationMultiplier)));
            // Since we want to decelerate the car, we are going to remove the torque from the wheels of the car.
            for (int i = 0; i < _frontWheelColliders.Count; i++)
            {
                _frontWheelColliders[i].motorTorque = 0;
            }

            for (int i = 0; i < _rearWheelColliders.Count; i++)
            {
                _rearWheelColliders[i].motorTorque = 0;
            }

            // If the magnitude of the car's velocity is less than 0.25f (very slow velocity), then stop the car completely and
            // also cancel the invoke of this method.
            if (_carRigidbody.velocity.magnitude < 0.25f)
            {
                _carRigidbody.velocity = Vector3.zero;
                CancelInvoke("DecelerateVehicle");
            }
        }

        public void ThrottleOff()
        {
            for (int i = 0; i < _frontWheelColliders.Count; i++)
            {
                _frontWheelColliders[i].motorTorque = 0;
            }

            for (int i = 0; i < _rearWheelColliders.Count; i++)
            {
                _rearWheelColliders[i].motorTorque = 0;
            }
        }

        public void ResetSteeringAngle()
        {
            if (_steeringAxis < 0f)
            {
                _steeringAxis = _steeringAxis + (Time.deltaTime * 10f * steeringSpeed);
            }
            else if (_steeringAxis > 0f)
            {
                _steeringAxis = _steeringAxis - (Time.deltaTime * 10f * steeringSpeed);
            }
            if (Mathf.Abs(_frontWheelColliders[0].steerAngle) < 1f)
            {
                _steeringAxis = 0f;
            }

            var steeringAngle = _steeringAxis * maxSteeringAngle;
            _frontWheelColliders[0].steerAngle = Mathf.Lerp(_frontWheelColliders[0].steerAngle, steeringAngle, steeringSpeed); // note to charles: sana LEFT WHEEL to
            _frontWheelColliders[1].steerAngle = Mathf.Lerp(_frontWheelColliders[1].steerAngle, steeringAngle, steeringSpeed); // sana RIGHT WHEEL to
        }

        public void Drivetrain(float _wheelTorque)
        {
            if (frontPower == true && rearPower == true)
            {
                for (int i = 0; i < _frontWheelColliders.Count; i++)
                {
                    _frontWheelColliders[i].motorTorque = _wheelTorque / 2;
                    _frontWheelColliders[i].brakeTorque = 0;
                }

                for (int i = 0; i < _rearWheelColliders.Count; i++)
                {
                    _rearWheelColliders[i].motorTorque = _wheelTorque / 2;
                    _rearWheelColliders[i].brakeTorque = 0;
                }
            }
            //Simulates FWD drivetrain for some reason
            else if (frontPower == false && rearPower == true)
            {
                for (int i = 0; i < _frontWheelColliders.Count; i++)
                {
                    _frontWheelColliders[i].motorTorque = 0;
                    _frontWheelColliders[i].brakeTorque = 0;
                }

                for (int i = 0; i < _rearWheelColliders.Count; i++)
                {
                    _rearWheelColliders[i].motorTorque = _wheelTorque;
                    _rearWheelColliders[i].brakeTorque = 0;
                }
            }
            //Simulates RWD drivetrain for some reason
            else if (frontPower == true && rearPower == false)
            {
                for (int i = 0; i < _frontWheelColliders.Count; i++)
                {
                    _frontWheelColliders[i].motorTorque = _wheelTorque;
                    _frontWheelColliders[i].brakeTorque = 0;
                }

                for (int i = 0; i < _rearWheelColliders.Count; i++)
                {
                    _rearWheelColliders[i].motorTorque = 0;
                    _rearWheelColliders[i].brakeTorque = 0;
                }
            }
            else
            {
                for (int i = 0; i < _frontWheelColliders.Count; i++)
                {
                    _frontWheelColliders[i].motorTorque = 0;
                    _frontWheelColliders[i].brakeTorque = 0;
                }

                for (int i = 0; i < _rearWheelColliders.Count; i++)
                {
                    _rearWheelColliders[i].motorTorque = 0;
                    _rearWheelColliders[i].brakeTorque = 0;
                }
            }
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
            _moveInput.performed += OnMoveInput;
            _moveInput.started += OnMoveInput;
            _moveInput.canceled += OnMoveInput;

            _handbrakeInput.started += OnHandbrakeInput;
            _handbrakeInput.canceled += OnHandbrakeInput;
        }

        public void DisableVehicleControls()
        {
            _moveInput.performed -= OnMoveInput;
            _moveInput.started -= OnMoveInput;
            _moveInput.canceled -= OnMoveInput;

            _handbrakeInput.started -=  OnHandbrakeInput;
            _handbrakeInput.canceled -= OnHandbrakeInput;

        }

        private void OnMoveInput(InputAction.CallbackContext context)
        {
            _driverInput = context.ReadValue<Vector2>();
        }

        private void OnHandbrakeInput(InputAction.CallbackContext context)
        {
            if (context.started)
                _isHandbraked = true;
            else
                _isHandbraked = false;
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
