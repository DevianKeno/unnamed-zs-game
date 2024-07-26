using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using UZSG.Systems;
using UZSG.Entities;

namespace UZSG.Players
{
    public struct FrameInput
    {
        public Vector2 move;
        public bool pressedJump;
        public bool pressedInteract;
        public bool pressedInventory;
    }

    /// <summary>
    /// Controls the player movement.
    /// </summary>
    public class PlayerControls : MonoBehaviour
    {
        public Player Player;

        #region These should be read as attributes
        /// <summary>
        /// Cached movement values from the Player. This should subscribe to the OnValueChanged event.
        /// </summary>
        public float MoveSpeed;
        public float RunSpeed;
        public float CrouchSpeed;
        public float JumpTime = 5f;
        public float JumpHeight = 2f;
        public float Gravity = -9.81f;
        public float FallSpeedMultiplier = 2f;
        public float CrouchSpeedMultiplier = 0.7f;
        public float LookRotationSpeed = 100.0f;
        #endregion
        
        public float GroundingForce = -1f;
        public float GroundDistance = 0.5f;

        [Header("Controls")]
        public float CameraSensitivity = 0.32f;
        public bool EnableMovementControls = true;
        public bool EnableCameraControls = true;
        /// <summary>
        /// The input values performed in the current frame.
        /// </summary>
        FrameInput _frameInput;
        /// <summary>
        /// The velocity to be applied for the current frame.
        /// </summary>
        Vector3 _frameVelocity;
        Vector3 _previousPosition;
        Vector3 _targetPosition;

        float _currentSpeed;
        bool _isMovePressed;
        float initialJumpVelocity;
        bool _hasJumped;
        float CrouchPosition;
        bool _isTransitioning;
        bool _isRunning;
        /// <summary>
        /// Check if holding [Run] key and speed is greater than run threshold
        /// </summary>
        public bool IsRunning { get => _isRunning; }
        bool _isCrouching;
        public bool isCrouching { get => _isCrouching; }

        public bool IsMoving
        {
            get
            {
                if (Magnitude > 0) return true;
                return false;
            }
        }

        public bool IsGrounded
        {
            get
            {
                return Physics.Raycast(groundChecker.position, Vector3.down, GroundDistance, groundMask);
            }
        }

        public bool IsFalling
        {
            get => _frameVelocity.y <= 0f && !IsGrounded;
        }

        public Vector3 Velocity
        {
            get => rb.velocity;
        }

        /// <summary>
        /// Represents how fast the player is moving in any direction.
        /// </summary>
        public float Speed
        {
            get => rb.velocity.magnitude;
        }

        public float HorizontalSpeed
        {
            get
            {
                var horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                return horizontalVelocity.magnitude;
            }
        }

        public float VerticalSpeed
        {
            get => rb.velocity.y;
        }

        public float Magnitude
        {
            get => new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;   
        }

        #region Events
        public event EventHandler OnMoveStart;
        public event EventHandler OnMoveStop;
        #endregion

        [Header("Components")]
        /// <summary>
        /// The Player's 3D model.
        /// </summary>
        [SerializeField] Transform model;
        [SerializeField] Rigidbody rb;
        [SerializeField] Transform groundChecker;
        [SerializeField] LayerMask groundMask;
        
        InputActionMap actionMap;
        Dictionary<string, InputAction> inputs = new();
        public Dictionary<string, InputAction> Inputs => inputs;
        
        internal void Initialize()
        {
            InitializeInputs();
            RetrieveAttributes();
            
            Game.Tick.OnTick += Tick;
            _previousPosition = transform.position;
        }

        Dictionary<string, Action> inputEvents;

        void InitializeInputs()
        {
            actionMap = Game.Main.GetActionMap("Player");
            inputs = Game.Main.GetActionsFromMap(actionMap);

            foreach (var input in inputs)
            {
                input.Value.performed += OnPerformInput;
            }
            
            inputs["Move"].performed += OnStartMove;
            inputs["Move"].started += OnStartMove;
            inputs["Move"].canceled += OnStartMove;

            inputs["Jump"].performed += OnJumpInput;        // Space (default)

            inputs["Run"].started += OnStartRun;            // Shift (default)
            inputs["Run"].canceled += OnStartRun;           // Shift (default)

            inputs["Crouch"].performed += OnCrouchInput;    // LCtrl (default)

            SetControlsEnabled(true);
        }

        void OnPerformInput(InputAction.CallbackContext context)
        {
            OnInput?.Invoke(context);
        }

        public event Action<InputAction.CallbackContext> OnInput; 
        
        void Awake()
        {
            Player = GetComponent<Player>();
        }

        void Tick(TickInfo e)
        {
            if (IsMoving && IsRunning)
            {
                /// Cache attributes for better performance
                var runStaminaCost = Player.Generic.GetAttributeFromId("run_stamina_cost").Value;
                Player.Vitals.GetAttributeFromId("stamina").Remove(runStaminaCost);
            }
        }

        void ConsoleWindowToggledCallback(bool value)
        {
            SetControlsEnabled(!value);
        }

        void SetControlsEnabled(bool value)
        {
            if (value)
            {
                actionMap.Enable();
            }
            else
            {
                actionMap.Disable();
            }
        }

        void RetrieveAttributes()
        {
            /// These can be cached and track changes using events
            MoveSpeed = Player.Generic.GetAttributeFromId("move_speed").Value;
            RunSpeed = Player.Generic.GetAttributeFromId("run_speed").Value;
            CrouchSpeed = Player.Generic.GetAttributeFromId("crouch_speed").Value;
            
            _currentSpeed = MoveSpeed;
        }

        void FixedUpdate()
        {
            HandleMovement();
            UpdateStates();
        }

        void UpdateStates()
        {
            if (_isMovePressed)
            {
                if (IsRunning)
                {
                    Player.MoveStateMachine.ToState(MoveStates.Run);
                }
                else
                {
                    Player.MoveStateMachine.ToState(MoveStates.Walk);
                }
            }
            else
            {
                Player.MoveStateMachine.ToState(MoveStates.Idle);
            }
        }

        void OnStartMove(InputAction.CallbackContext context)
        {
            if (!EnableMovementControls) return;

            _frameInput.move = context.ReadValue<Vector2>();
            _isMovePressed = _frameInput.move.x != 0 || _frameInput.move.y != 0;
        }

        void OnJumpInput(InputAction.CallbackContext context)
        {
            if (!EnableMovementControls) return;

            if (IsGrounded)
            {
                _hasJumped = true;
            }
        }

        void OnStartRun(InputAction.CallbackContext context)
        {            
            if (!EnableMovementControls) return;

            if (_isCrouching)
            {
                Crouch();
            }

            ToggleRun(!_isRunning);
        }

        void ToggleRun(bool value)
        {
            _isRunning = value;
            if (value)
            {
                _currentSpeed = RunSpeed;
                Player.MoveStateMachine.ToState(MoveStates.Run);
            }
            else
            {
                _currentSpeed = MoveSpeed;
                Player.MoveStateMachine.ToState(MoveStates.Idle);
            }
        }

        void OnCrouchInput(InputAction.CallbackContext context)
        {            
            if (!EnableMovementControls) return;

            Crouch();
        }

        Vector3 GetCameraForward()
        {
            Vector3 camForward = Player.MainCamera.transform.forward;
            camForward.y = 0f;
            return camForward;
        }

        void HandleMovement()
        {
            HandleDirection();
            HandleRotation();
            HandleGravity();
            // HandleJump();

            ApplyMovement();
        }

        void HandleDirection()
        {
            _frameVelocity = (_frameInput.move.x * Player.MainCamera.transform.right) + (_frameInput.move.y * GetCameraForward().normalized);
            _frameVelocity.Normalize();
        }

        void HandleRotation()
        {
            model.rotation = Quaternion.LookRotation(GetCameraForward().normalized);
        }

        void HandleJump()
        {
            if (!_hasJumped) return;
            _hasJumped = false;
            
            Player.MoveStateMachine.ToState(MoveStates.Jump);
            /// The time required to reach the highest point of the jump
            float timeToApex = JumpTime / 2;
            _frameVelocity.y = 2 * JumpHeight / timeToApex; /// this should be cached
        }

        void HandleGravity()
        {            
            /// Calculates the player's internal gravity
            Gravity = -2 * JumpHeight / Mathf.Pow(JumpTime / 2, 2); /// this should be cached

            if (IsGrounded) /// grounding force only
            {
                _frameVelocity.y = GroundingForce;

            }
            else if (IsFalling) /// increasing fall speed
            {
                _frameVelocity.y += Gravity * FallSpeedMultiplier * Time.fixedDeltaTime;
            }
            else /// normal gravity
            {
                _frameVelocity.y += Gravity * Time.fixedDeltaTime;
            }
        }
        
        void ApplyMovement()
        {
            _targetPosition = _previousPosition + (_frameVelocity * (_currentSpeed * Time.fixedDeltaTime));

            if (_previousPosition != _targetPosition)
            {
                Vector3 displacement = _targetPosition - _previousPosition;
                rb.velocity = displacement / Game.Tick.SecondsPerTick;
            }
            else
            {
                rb.velocity = Vector3.zero;
            }
        }

        void Crouch()
        {
            if (_isTransitioning) return;

            Player.MoveStateMachine.ToState(MoveStates.Crouch);

            _isTransitioning = !_isTransitioning;
            _isCrouching = !_isCrouching;

            float TransitionSpeed;
            
            if (_isCrouching)
            {            
                _currentSpeed = CrouchSpeed;
                CrouchPosition = Player.FPP.CameraController.transform.position.y - 1f;
                TransitionSpeed = 0.3f;
            }
            else
            {
                _currentSpeed = MoveSpeed;
                CrouchPosition = Player.FPP.CameraController.transform.position.y + 1f;
                TransitionSpeed = 0.3f;
            }

            LeanTween.value(gameObject, Player.FPP.CameraController.transform.position.y, CrouchPosition, TransitionSpeed)
            .setOnUpdate((i) =>
            {   
                Player.FPP.CameraController.transform.position = new Vector3(
                    Player.FPP.CameraController.transform.position.x,
                    i,
                    Player.FPP.CameraController.transform.position.z
                );
            }).setOnComplete(() =>
            {
                _isTransitioning = false;
            }).setEaseOutExpo();
        }

        public void SetControl(string name, bool enabled)
        {
            if (inputs.ContainsKey(name))
            {
                if (enabled)
                {
                    inputs[name].Enable();
                }
                else
                {
                    inputs[name].Disable();
                }
            }
        }
    }
}